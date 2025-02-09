﻿using FaasNet.EventMesh.Runtime.Messages;
using System.Collections.Generic;

namespace FaasNet.EventMesh.Runtime
{
    public static class Constants
    {
        public const string DefaultUrl = "localhost";
        public const string DefaultIPAddress = "127.0.0.1";
        public const int DefaultPort = 4889;
        public const string InMemoryBrokername = "inmemory";

        public static class LockNames
        {
            public const string Client = "client-{0}";
        }

        public static Dictionary<Commands, Commands> MappingRequestToResponse = new Dictionary<Commands, Commands>
        {
            { Commands.ADD_BRIDGE_REQUEST, Commands.ADD_BRIDGE_RESPONSE },
            { Commands.DISCONNECT_REQUEST, Commands.DISCONNECT_RESPONSE },
            { Commands.HEARTBEAT_REQUEST, Commands.HEARTBEAT_RESPONSE },
            { Commands.HELLO_REQUEST, Commands.HELLO_RESPONSE },
            { Commands.SUBSCRIBE_REQUEST, Commands.SUBSCRIBE_RESPONSE },
            { Commands.PUBLISH_MESSAGE_REQUEST, Commands.PUBLISH_MESSAGE_RESONSE }
        };
    }
}
