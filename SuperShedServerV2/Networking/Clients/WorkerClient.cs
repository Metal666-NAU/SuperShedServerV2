using Fleck;

namespace SuperShedServerV2.Networking.Clients;

public class WorkerClient(IWebSocketConnection socket) : ClientBase(socket) { }