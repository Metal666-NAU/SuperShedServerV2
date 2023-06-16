using Fleck;

using System.IO;
using System.Threading.Tasks;

namespace SuperShedServerV2.Networking.Clients;

public abstract class ClientBase/*<TMessageBase>*/()
	/*where TMessageBase : MessageFromServerToClientBase*/ {

	public required virtual IWebSocketConnection Socket { get; set; }

	/*public virtual Task Send<TMessage>(TMessage message)
		where TMessage : MessageBase =>
		Socket.Send(JsonSerializer.Serialize(message));*/

	public virtual async Task Send(byte command, params object[] data) {

		//BinaryWriter binaryWriter = new(new MemoryStream());

		//binaryWriter.Write(data.GetType().Name);

		//binaryWriter.Write(MessagePackSerializer.Serialize(data));

		//byte[] name = Encoding.UTF8.GetBytes(data.GetType().Name);

		//byte[] serializedData = MessagePackSerializer.Serialize(data);

		if(!Socket.IsAvailable) {

			return;

		}

		using MemoryStream memoryStream = new();

		using BinaryWriter binaryWriter = new(memoryStream);

		binaryWriter.Write(command);

		foreach(dynamic element in data) {

			binaryWriter.Write(element);

		}

		await Socket.Send(memoryStream.ToArray()/*name.Concat(serializedData).ToArray()*/);

	}

}