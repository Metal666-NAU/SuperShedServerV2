using Fleck;

namespace SuperShedServerV2.Networking.Clients;

public class AdminClient(Database.Collections.Admin admin, IWebSocketConnection socket) : ClientBase(socket) {

	public virtual Database.Collections.Admin Admin { get; set; } = admin;

}