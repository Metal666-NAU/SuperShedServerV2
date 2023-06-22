namespace SuperShedServerV2.Networking.Clients;

public class WorkerClient : ClientBase {

	public required virtual Database.Collections.Worker Worker { get; set; }

	public virtual void SendProductInfo(float productWidth,
										float productHeight,
										float productLength,
										string productManufacturer,
										string rackId,
										int productShelf,
										int productSpot,
										string productCategory,
										string productName) =>
		Send((byte) Message.ProductInfo, productWidth, productHeight, productLength, productManufacturer, rackId, productShelf, productSpot, productCategory, productName);

	public enum Message {

		ProductInfo

	}

}