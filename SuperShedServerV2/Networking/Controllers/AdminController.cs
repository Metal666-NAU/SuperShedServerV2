using Fleck;

using MongoDB.Bson;

using SuperShedServerV2.Networking.Clients;

using System;
using System.Text.Json;

namespace SuperShedServerV2.Networking.Controllers;

public class AdminController : ControllerBase<AdminClient> {

	public AdminController() {

		Output.OnLog += async (message, severity) => {

			foreach(AdminClient adminClient in Clients) {

				await adminClient.SendLog(message, severity);

			}

		};

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

				_ = adminClient.SendLog(log.Message, log.Severity);

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

	public class AuthRequest {

		public virtual string? Username { get; set; }
		public virtual string? Password { get; set; }
		public virtual string? AuthToken { get; set; }

	}

	public static class Messages {

		#region Incoming
		#endregion

		#region Outgoing
		public record Log(string Message, Output.Severity Severity) : MessageData;
		#endregion

	}

}