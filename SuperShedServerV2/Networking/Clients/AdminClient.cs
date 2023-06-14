using Fleck;

namespace SuperShedServerV2.Networking.Clients;

public class AdminClient(IWebSocketConnection socket) : ClientBase(socket) { }