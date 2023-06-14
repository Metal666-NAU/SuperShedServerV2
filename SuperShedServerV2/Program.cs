using Fleck;

using SuperShedServerV2.Networking.Controllers;

using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SuperShedServerV2;

public class Program {

	public static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {

		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

	};

	public static WebSocketServer? Server { get; set; }

	public static Dictionary<string, ControllerBase> Controllers { get; set; } = new() {

		{ "/worker", new WorkerController() },
		{ "/admin", new AdminController() }

	};

	private static void Main(string[] args) {

		Output.Log("The server is running :3");



		if(!Database.Initialize()) {

			return;

		}

		Server = new("ws://0.0.0.0:8181");

		Server.Start(socket => {

			string GetPath() => socket.ConnectionInfo.Path;

			ControllerBase? FindController() {

				string path = GetPath();

				if(!Controllers.TryGetValue(path, out ControllerBase? controller)) {

					Output.Error($"No Controller found for path {path}!");

					return null;

				}

				return controller;

			}

			socket.OnOpen = () => Output.Info($"New client connected: {socket.ConnectionInfo.ClientIpAddress} -> {GetPath()}");
			socket.OnClose = () => {

				Output.Info($"Client disconnected: {socket.ConnectionInfo.ClientIpAddress}");

				FindController()?.OnDisconnected(socket);

			};

			socket.OnMessage = message => {

				Output.Info($"Received auth message: {message} on {GetPath()}");

				FindController()?.OnAuth(socket, message);

			};

			socket.OnBinary = message => {

				using MemoryStream memoryStream = new(message);

				using BinaryReader binaryReader = new(memoryStream);

				string command = binaryReader.ReadString();
				string data = binaryReader.ReadString();

				FindController()?.Handle(command, type => {

					if(!typeof(ITuple).IsAssignableFrom(type)) {

						return null;

					}

					return JsonSerializer.Deserialize(data, type, JSON_SERIALIZER_OPTIONS) as ITuple;

				});

			};

			/*socket.OnMessage = message => {

				Messages.AuthRequest? authRequest = JsonSerializer.Deserialize<Messages.AuthRequest>(message);

				if(authRequest == null || (!authRequest.HasToken && !authRequest.HasCredentials)) {

					socket.Send(JsonSerializer.Serialize(new Messages.AuthResponse() {

						Success = false

					}));

					socket.Close(WebSocketStatusCodes.PolicyViolation);

					return;

				}

				if(authRequest.HasCredentials &&
					Database.TryGetUser(authRequest.Username!,
										authRequest.Password!,
										out Database.Collections.User? user1)) {

					string loginToken = Database.LogUserIn(user1!);

					Clients.Add(new WorkerClient(socket));

					socket.Send(JsonSerializer.Serialize(new Messages.AuthResponse() {

						Success = true,
						LoginToken = loginToken

					}));

				}

				if(authRequest.HasToken &&
					Database.ValidateLoginToken(authRequest.LoginToken!,
												out Database.Collections.User? user2)) {

					Clients.Add(new WorkerClient(socket));

					socket.Send(JsonSerializer.Serialize(new Messages.AuthResponse() {

						Success = true,
						LoginToken = authRequest.LoginToken!

					}));

				}

			};

			socket.OnBinary = message => {

				using MemoryStream memoryStream = new(message);

				using BinaryReader binaryReader = new(memoryStream);

				string command = binaryReader.ReadString();
				string data = binaryReader.ReadString();

				new AdminController().Handle(command, type => {

					if(!typeof(ITuple).IsAssignableFrom(type)) {

						return null;

					}

					return JsonSerializer.Deserialize(data, type) as ITuple;

				});

			};*/

		});

		UI.Start();

		Server.Dispose();

	}

	public static class Messages {

		public class AuthRequest {

			public virtual string? LoginToken { get; set; }
			public virtual string? Username { get; set; }
			public virtual string? Password { get; set; }

			public virtual bool HasToken => LoginToken != null;
			public virtual bool HasCredentials => Username != null && Password != null;

		}

		public class AuthResponse {

			public virtual bool Success { get; set; }
			public virtual string? LoginToken { get; set; }

		}

	}

}