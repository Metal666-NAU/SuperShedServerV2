using System.Threading.Tasks;

namespace SuperShedServerV2.Networking.Clients;

public class AdminClient : ClientBase {

	public required virtual Database.Collections.Admin Admin { get; set; }

	public virtual async Task SendLog(string message, Output.Severity severity) =>
		await Send((byte) Message.Log, message, (byte) severity);

	public enum Message {

		Log

	}

}