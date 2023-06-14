using System;
using System.Collections.Generic;

using Test = (int a, int b);

namespace SuperShedServerV2.Networking.Controllers;

public class AdminController : ControllerBase {

	public override Dictionary<string, Type> Messages { get; set; } = new() {

		{ "test", typeof(Test) }

	};

	public AdminController() {

		On<Test>(test => { });

	}

}