using MessagePack;

namespace SuperShedServerV2.Networking;

[MessagePackObject(true)]
public record Message {

	public virtual string? Name { get; set; }

	public virtual byte[]? Data { get; set; }

	public Message(byte data) {

		Name = nameof(data);

	}

}

public abstract record MessageData { }