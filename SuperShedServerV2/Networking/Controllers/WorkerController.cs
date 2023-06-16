using Fleck;

using MongoDB.Bson;

using System;
using System.Collections.Generic;
using System.Text.Json;

using Test = (int a, int b);

namespace SuperShedServerV2.Networking.Controllers;

public class WorkerController : ControllerBase<Clients.WorkerClient> {

	public override Dictionary<string, Type> Messages { get; set; } = new() {

		{ "test", typeof(Test) }

	};

	public WorkerController() {

		On<Test>(test => { });

	}

	protected override void OnAuth(IWebSocketConnection socket, string message, Action<string> reject) {

		void Accept(string authToken, ObjectId workerId) {

			Database.Collections.Worker? worker = Database.GetWorker(workerId);

			if(worker == null) {

				Output.Error($"Failed to authenticate worker {workerId}: Worker was not found!");

				// TODO: Notify client

				return;

			}

			Output.Log($"Authenticated client: {socket.ConnectionInfo.ClientIpAddress} on {socket.ConnectionInfo.Path}");

			Clients.Add(new(worker, socket));

			socket.Send(JsonSerializer.Serialize(new AuthResponse() {

				Success = true,
				AuthToken = authToken

			}, Program.JSON_SERIALIZER_OPTIONS));

		}

		AuthRequest? authRequest = JsonSerializer.Deserialize<AuthRequest>(message, Program.JSON_SERIALIZER_OPTIONS);

		if(authRequest == null) {

			reject($"Failed to parse authentication request ({message})");

			return;

		}

		if(authRequest.LoginCode != null) {

			if(GlobalState.PendingWorkerAuth == null) {

				reject("Failed to authenticate using Login Code: No Worker is pending auth.");

				return;

			}

			if(GlobalState.PendingWorkerAuth.Value.LoginCode.Equals(authRequest.LoginCode)) {

				reject($"Failed to authenticate using Login Code: Provided code is incorrect ({authRequest.LoginCode}).");

				return;

			}

			ObjectId workerId = GlobalState.PendingWorkerAuth.Value.WorkerId;

			Accept(Database.CreateAuthToken(workerId), workerId);

			return;

		}

		if(authRequest.AuthToken != null) {

			string authToken = authRequest.AuthToken;

			ObjectId? workerId = Database.GetUserId(authToken);

			if(workerId == null) {

				reject($"Failed to authenticate using Auth Token: Provided token is invalid ({authToken}).");

				return;

			}

			Accept(authToken, workerId.Value);

			return;

		}

		reject("Failed to authenticate: No auth data provided.");

	}

	public class AuthRequest {

		public virtual string? LoginCode { get; set; }
		public virtual string? AuthToken { get; set; }

	}

}