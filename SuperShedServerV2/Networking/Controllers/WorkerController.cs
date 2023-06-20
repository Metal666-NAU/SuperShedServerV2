using Fleck;

using MongoDB.Bson;

using SuperShedServerV2.Networking.Clients;

using System;
using System.Linq;
using System.Text.Json;

namespace SuperShedServerV2.Networking.Controllers;

public class WorkerController : ControllerBase<WorkerClient> {

	public event Action<WorkerClient, bool>? WorkerStatusChanged;

	public override void Initialize() {

		base.Initialize();

		On((byte) Message.ProductInfo, (client, data) => {

			string productId = data.ReadString();

			Database.Collections.Product? product = Database.FindProduct(productId);

			if(product == null) {

				client.SendProductNotFound();

				return;

			}

			Database.Collections.Manufacturer? manufacturer = Database.GetManufacturer(product.ManufacturerId!.Value);

			client.SendProductInfo(manufacturer?.Name ?? "");

		});

	}

	protected override void OnAuth(IWebSocketConnection socket, string message, Action<string, string?> reject) {

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

			reject($"Failed to parse authentication request!", message);

			return;

		}

		if(authRequest.LoginCode != null) {

			ObjectId? workerId = Program.GetController<AdminController>()
										.LogWorkerIn(authRequest.LoginCode);

			if(workerId == null) {

				reject($"Failed to authenticate using Login Code: No pending auth request with this code!", authRequest.LoginCode);

				return;

			}

			Accept(Database.FindOrCreateAuthToken(workerId.Value), workerId.Value);

			return;

		}

		if(authRequest.AuthToken != null) {

			string authToken = authRequest.AuthToken;

			ObjectId? workerId = Database.GetUserId(authToken);

			if(workerId == null) {

				reject($"Failed to authenticate using Auth Token: Provided Token is invalid!", authToken);

				return;

			}

			Accept(authToken, workerId.Value);

			return;

		}

		reject("Failed to authenticate: No auth data provided!", null);

	}

	public override void OnDisconnected(IWebSocketConnection socket) {

		WorkerStatusChanged?.Invoke(Clients.Single(workerClient => workerClient.Socket == socket), false);

		base.OnDisconnected(socket);

	}

	public virtual void RevokeWorkerAuth(string workerId) {

		Database.DeleteAuthToken(workerId);

		Clients.FirstOrDefault(workerClient => workerClient.Worker.StringId.Equals(workerId))?.Socket.Close();

	}

	public class AuthRequest {

		public virtual string? LoginCode { get; set; }
		public virtual string? AuthToken { get; set; }

	}

	public enum Message {

		ProductInfo,
		ShelfInfo,
		ScanResult

	}

}