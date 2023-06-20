using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperShedServerV2;

public static class Output {

	public static List<(string Message, Severity Severity)> Logs { get; set; } =
		new();

	public static event Action<string, Severity>? OnLog;

	public static void Debug(string message) => Write(message, Severity.Debug);
	public static void Log(string message) => Write(message, Severity.Log);
	public static void Info(string message) => Write(message, Severity.Info);
	public static void Error(string message) => Write(message, Severity.Error);

	public static void Write(string message, Severity severity) {

		message = $"[{DateTime.Now.TimeOfDay}] {message}";

		Console.ForegroundColor = typeof(Severity).GetMember(severity.ToString())
													.First()
													.GetCustomAttribute<SeverityAttribute>()!
													.Color;

		Console.WriteLine(message);

		Console.ForegroundColor = ConsoleColor.DarkGray;

		Logs.Add((message, severity));

		OnLog?.Invoke(message, severity);

	}

	public enum Severity {

		[Severity(ConsoleColor.White)]
		Log,
		[Severity(ConsoleColor.Green)]
		Info,
		[Severity(ConsoleColor.Red)]
		Error,
		[Severity(ConsoleColor.DarkGray)]
		Debug

	}

	[AttributeUsage(AttributeTargets.Field)]
	public class SeverityAttribute(ConsoleColor color) : Attribute {

		public virtual ConsoleColor Color { get; set; } = color;

	}

}