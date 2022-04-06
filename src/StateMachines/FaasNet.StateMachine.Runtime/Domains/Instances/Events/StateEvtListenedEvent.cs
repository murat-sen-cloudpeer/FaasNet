﻿using FaasNet.Domain;
using System.Diagnostics;

namespace FaasNet.StateMachine.Runtime.Domains.Instances.Events
{
    [DebuggerDisplay("State {StateId} listen Event {EvtName}")]
    public class StateEvtListenedEvent : DomainEvent
    {
        public StateEvtListenedEvent(string id, string aggregateId, string stateId, string evtName, string evtSource, string evtType) : base(id, aggregateId)
        {
            StateId = stateId;
            EvtName = evtName;
            EvtSource = evtSource;
            EvtType = evtType;
        }

        public string StateId { get; set; }
        public string EvtName { get; set; }
        public string EvtSource { get; set; }
        public string EvtType {  get; set; }
    }
}
