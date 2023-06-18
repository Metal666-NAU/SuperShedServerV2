﻿using Fleck;

using MongoDB.Bson;

using SuperShedServerV2.Networking.Clients;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;

namespace SuperShedServerV2.Networking.Controllers;

public class AdminController : ControllerBase<AdminClient> {

	public override void Initialize() {

		Output.OnLog += (message, severity) => {

			foreach(AdminClient adminClient in Clients) {

				adminClient.SendLog(message, severity);

			}

		};

		Program.GetController<WorkerController>().WorkerStatusChanged += (workerClient, online) => {

			foreach(AdminClient adminClient in Clients) {

				adminClient.SendWorkerStatus(workerClient.Worker.StringId, online);

			}

		};

		On((byte) Message.StartWorkerAuth, (client, data) => {

			ObjectId workerId = new(data.ReadString());

			List<string> loginCodeCharacters = new();

			for(int i = 0; i < 6; i++) {

				loginCodeCharacters.Add(RandomNumberGenerator.GetInt32(10).ToString());

			}

			string loginCode = string.Join("", loginCodeCharacters);

			client.WorkerPendingAuth = (loginCode, workerId);

			client.SendWorkerLoginCode(loginCode);

		});

		On((byte) Message.CancelWorkerAuth, (client, data) => {

			client.WorkerPendingAuth = null;

		});

	}

	protected override void OnAuth(IWebSocketConnection socket, string message, Action<string> reject) {

		void Accept(string authToken, ObjectId adminId) {

			Database.Collections.Admin? admin = Database.GetAdmin(adminId);

			if(admin == null) {

				Output.Error($"Failed to authenticate admin {adminId}: Admin was not found!");

				// TODO: Notify client

				return;

			}

			Output.Log($"Authenticated client: {socket.ConnectionInfo.ClientIpAddress} on {socket.ConnectionInfo.Path}");

			AdminClient adminClient = new() {

				Admin = admin,
				Socket = socket

			};

			Clients.Add(adminClient);

			socket.Send(JsonSerializer.Serialize(new AuthResponse() {

				Success = true,
				AuthToken = authToken

			}, Program.JSON_SERIALIZER_OPTIONS));

			foreach((string Message, Output.Severity Severity) log in Output.Logs) {

				adminClient.SendLog(log.Message, log.Severity);

			}

			foreach(Database.Collections.Worker worker in Database.GetWorkers()) {

				adminClient.SendWorker(worker.StringId, worker.Name ?? "[null]");

			}

			WorkerController workerController = Program.GetController<WorkerController>();

			foreach(WorkerClient workerClient in workerController.Clients) {

				adminClient.SendWorkerStatus(workerClient.Worker.StringId, true);

			}

		}

		AuthRequest? authRequest = JsonSerializer.Deserialize<AuthRequest>(message,
																			Program.JSON_SERIALIZER_OPTIONS);

		if(authRequest == null) {

			reject($"Failed to parse authentication request ({message})");

			return;

		}

		if(authRequest.Username != null &&
			authRequest.Password != null) {

			ObjectId? adminId = Database.GetAdmin(authRequest.Username, authRequest.Password)?.Id;

			if(adminId == null) {

				reject($"Failed to authenticate using Login Credentials: credentials are invalid (Username: {authRequest.Username}, Password: {authRequest.Password}).");

				return;

			}

			Accept(Database.FindOrCreateAuthToken(adminId.Value), adminId.Value);

			return;

		}

		if(authRequest.AuthToken != null) {

			string authToken = authRequest.AuthToken;

			ObjectId? adminId = Database.GetUserId(authToken);

			if(adminId == null) {

				reject($"Failed to authenticate using Auth Token: Provided token is invalid ({authToken}).");

				return;

			}

			Accept(authToken, adminId.Value);

			return;

		}

		reject("Failed to authenticate: No auth data provided.");

	}

	public virtual ObjectId? LogWorkerIn(string loginCode) {

		(AdminClient AdminClient, ObjectId WorkerId)? pendingAuth = null;

		foreach(AdminClient adminClient in Clients) {

			if(!adminClient.WorkerPendingAuth.HasValue) {

				continue;

			}

			(string LoginCode, ObjectId WorkerId) = adminClient.WorkerPendingAuth.Value;

			if(!LoginCode.Equals(loginCode)) {

				continue;

			}

			pendingAuth = (adminClient, WorkerId);

		}

		if(pendingAuth.HasValue) {

			pendingAuth.Value.AdminClient.WorkerPendingAuth = null;

			pendingAuth.Value.AdminClient.SendWorkerAuthSuccess();

		}

		return pendingAuth?.WorkerId;

	}

	public class AuthRequest {

		public virtual string? Username { get; set; }
		public virtual string? Password { get; set; }
		public virtual string? AuthToken { get; set; }

	}

	public enum Message {

		StartWorkerAuth,
		CancelWorkerAuth

	}

}