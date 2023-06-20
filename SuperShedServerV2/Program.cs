using Fleck;

using SuperShedServerV2.Networking.Controllers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

	public static ConcurrentQueue<(IWebSocketConnection, byte[])> MessageQueue { get; set; }
		= new();

	private static async Task Main(string[] args) {

		Output.Log("The server is running :3");

		if(!Database.Initialize()) {

			return;

		}

		Server = new("ws://0.0.0.0:8181");

		foreach(ControllerBase controller in Controllers.Values) {

			controller.Initialize();

		}

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

				Output.Log($"Received auth message from {socket.ConnectionInfo.ClientIpAddress} on {GetPath()}");
				Output.Debug(message);

				FindController()?.Auth(socket, message);

			};

			socket.OnBinary = message => {

				using MemoryStream memoryStream = new(message);

				using BinaryReader binaryReader = new(memoryStream);

				byte command = binaryReader.ReadByte();

				FindController()?.Handle(command, socket, binaryReader);

			};

		});

		bool shouldClose = false;

		AppDomain.CurrentDomain.ProcessExit += (sender, args) => shouldClose = true;

		while(!shouldClose) {

			if(!MessageQueue.TryDequeue(out (IWebSocketConnection Client, byte[] Data) message)) {

				continue;

			}

			await message.Client.Send(message.Data);

		}

		Output.Log("The server is shutting down >.<");

		while(MessageQueue.TryDequeue(out (IWebSocketConnection Client, byte[] Data) message)) {

			await message.Client.Send(message.Data);

		}

		Dispose();

	}

	public static TController GetController<TController>() where TController : ControllerBase =>
		Controllers.Values.OfType<TController>().Single();

	public static void Dispose() {

		Server!.Dispose();

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