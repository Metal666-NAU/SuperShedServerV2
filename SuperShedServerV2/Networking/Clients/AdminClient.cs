using MongoDB.Bson;

namespace SuperShedServerV2.Networking.Clients;

public class AdminClient : ClientBase {

	public required virtual Database.Collections.Admin Admin { get; set; }

	public virtual (string LoginCode, ObjectId WorkerId)? WorkerPendingAuth { get; set; }

	public virtual void SendLog(string message, Output.Severity severity) =>
		Send((byte) Message.Log, message, (byte) severity);

	public virtual void SendWorker(string workerId, string workerName) =>
		Send((byte) Message.Worker, workerId, workerName);

	public virtual void SendWorkerStatus(string workerId, bool isOnline) =>
		Send((byte) Message.WorkerStatus, workerId, isOnline);

	public virtual void SendWorkerLoginCode(string loginCode) =>
		Send((byte) Message.WorkerLoginCode, loginCode);

	public virtual void SendWorkerAuthSuccess() =>
		Send((byte) Message.WorkerAuthSuccess);

	public enum Message {

		Log,
		Worker,
		WorkerStatus,
		WorkerLoginCode,
		WorkerAuthSuccess

	}

}