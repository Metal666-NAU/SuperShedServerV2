namespace SuperShedServerV2.Networking.Clients;

public class WorkerClient : ClientBase {

	public required virtual Database.Collections.Worker Worker { get; set; }

	public virtual void SendProductNotFound() =>
		Send((byte) Message.ProductNotFound);

	public virtual void SendProductInfo(string manufacturer) =>
		Send((byte) Message.ProductInfo, manufacturer);

	public enum Message {

		ProductNotFound,
		ProductInfo,
		ShelfInfo

	}

}