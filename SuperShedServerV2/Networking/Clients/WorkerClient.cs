namespace SuperShedServerV2.Networking.Clients;

public class WorkerClient : ClientBase {

	public required virtual Database.Collections.Worker Worker { get; set; }

}