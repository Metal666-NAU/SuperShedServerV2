namespace SuperShedServerV2.Networking.Clients;

public class WorkerClient : ClientBase {

	public required virtual Database.Collections.Worker Worker { get; set; }

	public virtual void SendProductInfo(float productWidth,
										float productLength,
										float productHeight,
										string productManufacturer,
										string rackId,
										int productShelf,
										int productSpot,
										string productCategory,
										string productName) =>
		Send((byte) Message.ProductInfo,
				productWidth, productLength, productHeight, productManufacturer, rackId, productShelf, productSpot, productCategory, productName);

	public enum Message {

		ProductInfo

	}

}