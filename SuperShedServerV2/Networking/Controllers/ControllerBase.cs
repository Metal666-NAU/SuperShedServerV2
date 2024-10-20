using Fleck;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Timers;

namespace SuperShedServerV2.Networking.Controllers;

public abstract class ControllerBase<TClient> : ControllerBase
	where TClient : Clients.ClientBase {

	public virtual Dictionary<byte, Action<TClient, BinaryReader>> TypedHandlers { get; set; } =
		[];

	public virtual List<TClient> Clients { get; set; } =
		[];

	public override void Initialize() {

		base.Initialize();

		Timer timer = new() {

			Interval = TimeSpan.FromMinutes(1).TotalMilliseconds,
			AutoReset = true

		};

		timer.Elapsed += (sender, args) => {

			Output.Debug("Pinging clients...");

			foreach(TClient client in Clients) {

				client.SendPing();

			}

		};

		timer.Start();

	}

	protected virtual void On(byte command, Action<TClient, BinaryReader> handler) {

		TypedHandlers.Add(command, handler);

		On(command, (IWebSocketConnection socket, BinaryReader data) => {

			TClient? client = Clients.FirstOrDefault(client => client.Socket == socket);

			if(client == null) {

				return;

			}

			TypedHandlers[command].Invoke(client, data);

		});

	}

	public override void OnDisconnected(IWebSocketConnection socket) =>
		Clients.RemoveAll(client => client.Socket == socket);

}

public abstract class ControllerBase {

	public virtual Dictionary<byte, Action<IWebSocketConnection, BinaryReader>> Handlers { get; set; } =
		[];

	public virtual void Initialize() { }

	protected virtual void On(byte command, Action<IWebSocketConnection, BinaryReader> handler) =>
		Handlers.Add(command, handler);

	public virtual void Auth(IWebSocketConnection socket, string message) {

		OnAuth(socket,
				message,
				(reason, sensitiveData) => {

					Output.Error(reason);

					if(sensitiveData != null) {

						Output.Debug(sensitiveData);

					}

					socket.Send(JsonSerializer.Serialize(new AuthResponse() {

						Success = false

					}, Program.JsonSerializerOptions));

				});

	}

	protected abstract void OnAuth(IWebSocketConnection socket, string message, Action<string, string?> reject);

	public abstract void OnDisconnected(IWebSocketConnection socket);

	public virtual void Handle(byte command, IWebSocketConnection socket, BinaryReader data) {

		Action<IWebSocketConnection, BinaryReader>? handler = Handlers.GetValueOrDefault(command);

		if(handler == null) {

			return;

		}

		handler?.Invoke(socket, data);

	}

	public class AuthResponse {

		public virtual bool? Success { get; set; }
		public virtual string? AuthToken { get; set; }

	}

}