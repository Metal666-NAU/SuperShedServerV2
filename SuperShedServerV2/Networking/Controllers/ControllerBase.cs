using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SuperShedServerV2.Networking.Controllers;

public abstract class ControllerBase {

	public virtual Dictionary<Type, Delegate> Handlers { get; set; } = new();

	public abstract Dictionary<string, Type> Messages { get; set; }

	public virtual void On<TMessage>(Action<TMessage> handler)
		where TMessage : ITuple {

		Handlers.Add(typeof(TMessage), handler);

	}

	public virtual void Handle(string command, Func<Type, ITuple?> getData) {

		Type? messageType = Messages.GetValueOrDefault(command);

		if(messageType == null) {

			return;

		}

		Delegate? handler = Handlers.GetValueOrDefault(messageType);

		if(handler == null) {

			return;

		}

		handler?.DynamicInvoke(getData(messageType));

	}

}