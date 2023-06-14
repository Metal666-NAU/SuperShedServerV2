using Fleck;

namespace SuperShedServerV2.Networking.Clients;

public abstract class ClientBase/*<TMessageBase>*/(IWebSocketConnection socket)
	/*where TMessageBase : MessageFromServerToClientBase*/ {

	public virtual IWebSocketConnection Socket { get; set; } = socket;

	/*public virtual Task Send<TMessage>(TMessage message)
		where TMessage : MessageBase =>
		Socket.Send(JsonSerializer.Serialize(message));*/

}