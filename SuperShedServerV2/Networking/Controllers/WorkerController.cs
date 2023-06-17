using Fleck;

using MongoDB.Bson;

using SuperShedServerV2.Networking.Clients;

using System;
using System.Linq;
using System.Text.Json;

namespace SuperShedServerV2.Networking.Controllers;

public class WorkerController : ControllerBase<WorkerClient> {

	public event Action<WorkerClient, bool>? WorkerStatusChanged;

	protected override void OnAuth(IWebSocketConnection socket, string message, Action<string> reject) {

		void Accept(string authToken, ObjectId workerId) {

			Database.Collections.Worker? worker = Database.GetWorker(workerId);

			if(worker == null) {

				Output.Error($"Failed to authenticate worker {workerId}: Worker was not found!");

				// TODO: Notify client

				return;

			}

			Output.Log($"Authenticated client: {socket.ConnectionInfo.ClientIpAddress} on {socket.ConnectionInfo.Path}");

			WorkerClient workerClient = new() {

				Worker = worker,
				Socket = socket

			};

			Clients.Add(workerClient);

			socket.Send(JsonSerializer.Serialize(new AuthResponse() {

				Success = true,
				AuthToken = authToken

			}, Program.JSON_SERIALIZER_OPTIONS));

			WorkerStatusChanged?.Invoke(workerClient, true);

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

			Accept(Database.FindOrCreateAuthToken(workerId), workerId);

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

	public override void OnDisconnected(IWebSocketConnection socket) {

		WorkerStatusChanged?.Invoke(Clients.Single(workerClient => workerClient.Socket == socket), false);

		base.OnDisconnected(socket);

	}

	public class AuthRequest {

		public virtual string? LoginCode { get; set; }
		public virtual string? AuthToken { get; set; }

	}

}