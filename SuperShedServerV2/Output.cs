using System;

namespace SuperShedServerV2;

public static class Output {

	public static void Log(string message) => WriteWithColor(message);
	public static void Info(string message) => WriteWithColor(message, ConsoleColor.Green);
	public static void Error(string message) => WriteWithColor(message, ConsoleColor.Red);

	private static void WriteWithColor(string message, ConsoleColor color = ConsoleColor.White) {

		Console.ForegroundColor = color;

		Console.WriteLine(message);

		Console.ForegroundColor = ConsoleColor.DarkGray;

	}

}