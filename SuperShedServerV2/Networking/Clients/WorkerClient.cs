using Fleck;

namespace SuperShedServerV2.Networking.Clients;

public class WorkerClient(Database.Collections.Worker worker, IWebSocketConnection socket) : ClientBase(socket) {

	public virtual Database.Collections.Worker Worker { get; set; } = worker;

}