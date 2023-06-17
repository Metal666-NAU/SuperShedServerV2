﻿using System.Threading.Tasks;

namespace SuperShedServerV2.Networking.Clients;

public class AdminClient : ClientBase {

	public required virtual Database.Collections.Admin Admin { get; set; }

	public virtual async Task SendLog(string message, Output.Severity severity) =>
		await Send((byte) Message.Log, message, (byte) severity);

	public virtual async Task SendWorker(string workerId, string workerName) =>
		await Send((byte) Message.Worker, workerId, workerName);

	public virtual async Task SendWorkerStatus(string workerId, bool isOnline) =>
		await Send((byte) Message.WorkerStatus, workerId, isOnline);

	public enum Message {

		Log,
		Worker,
		WorkerStatus

	}

}