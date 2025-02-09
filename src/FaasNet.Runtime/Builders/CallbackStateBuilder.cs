﻿using FaasNet.Runtime.Domains.Definitions;
using System;

namespace FaasNet.Runtime.Builders
{
    public class CallbackStateBuilder : BaseStateBuilder<WorkflowDefinitionCallbackState>
    {
        public CallbackStateBuilder() : base(WorkflowDefinitionCallbackState.Create())
        {
        }

        public CallbackStateBuilder SetAction(string name, Action<ActionBuilder> callback)
        {
            var builder = new ActionBuilder(name);
            callback(builder);
            StateDef.Action = builder.Build();
            return this;
        }

        public CallbackStateBuilder SetEventRef(string evtRef)
        {
            StateDef.EventRef = evtRef;
            return this;
        }

        public CallbackStateBuilder End()
        {
            StateDef.End = true;
            return this;
        }
    }
}
