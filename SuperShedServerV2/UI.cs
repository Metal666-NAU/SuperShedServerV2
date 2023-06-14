using Spectre.Console;
using Spectre.Console.Rendering;

namespace SuperShedServerV2;

public static class UI {

	public static Layout? Root { get; set; }


	public static void Start() {

		/*Layout root =
			new Layout().SplitColumns(new Layout(new Text("left")),
										new Layout(new Text("right")));*/

		AnsiConsole.Write(CreateRoot());

		while(true) {

			string command = AnsiConsole.Ask<string>(">");

		}

	}

	public static IRenderable CreateRoot() => Root = new();



}