using MongoDB.Bson;

namespace SuperShedServerV2;

public static class GlobalState {

	public static (string LoginCode, ObjectId WorkerId)? PendingWorkerAuth { get; set; }

}