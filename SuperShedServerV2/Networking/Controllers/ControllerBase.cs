using Fleck;

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SuperShedServerV2.Networking.Controllers;

public abstract class ControllerBase<TClient> : ControllerBase
	where TClient : Clients.ClientBase {

	public virtual List<TClient> Clients { get; set; } = new();

	public override void OnDisconnected(IWebSocketConnection socket) =>
		Clients.RemoveAll(client => client.Socket == socket);

}

public abstract class ControllerBase {

	public virtual Dictionary<short, Action<byte[]>> Handlers { get; set; } = new();

	//public abstract Dictionary<string, Type> Messages { get; set; }

	protected virtual void On(byte command, Action<byte[]> handler) =>
		Handlers.Add(command, handler);

	public virtual void Auth(IWebSocketConnection socket, string message) {

		OnAuth(socket, message, reason => {

			Output.Error(reason);

			socket.Send(JsonSerializer.Serialize(new AuthResponse() {

				Success = false

			}, Program.JSON_SERIALIZER_OPTIONS));

		});

	}

	protected abstract void OnAuth(IWebSocketConnection socket, string message, Action<string> reject);

	public abstract void OnDisconnected(IWebSocketConnection socket);

	public virtual void Handle(short command, byte[] data/*string command, Func<Type, ITuple?> getData*/) {

		Action<byte[]>? handler = Handlers.GetValueOrDefault(command);

		if(handler == null) {

			return;

		}

		handler?.DynamicInvoke(data);

		/*Type? messageType = Messages.GetValueOrDefault(command);

		if(messageType == null) {

			return;

		}

		Delegate? handler = Handlers.GetValueOrDefault(messageType);

		if(handler == null) {

			return;

		}

		handler?.DynamicInvoke(getData(messageType));*/

	}

	public class AuthResponse {

		public virtual bool? Success { get; set; }
		public virtual string? AuthToken { get; set; }

	}

}