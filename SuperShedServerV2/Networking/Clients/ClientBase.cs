using Fleck;

using System.IO;

namespace SuperShedServerV2.Networking.Clients;

public abstract class ClientBase {

	public required virtual IWebSocketConnection Socket { get; set; }

	public virtual void SendPing() =>
		Send(byte.MaxValue);

	public virtual void Send(byte command, params object[] data) {

		if(!Socket.IsAvailable) {

			return;

		}

		using MemoryStream memoryStream = new();

		using BinaryWriter binaryWriter = new(memoryStream);

		binaryWriter.Write(command);

		foreach(dynamic element in data) {

			binaryWriter.Write(element);

		}

		Program.MessageQueue.Enqueue((Socket, memoryStream.ToArray()));

	}

}