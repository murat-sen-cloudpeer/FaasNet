using AsyncapiEventMeshClient.Models;
using CloudNative.CloudEvents;
using FaasNet.EventMesh.Client;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncapiEventMeshClient
{
	public class EvtMeshClient : EventMeshClient
	{
		public EvtMeshClient(string clientId, string password, string vpn = Constants.DefaultVpn, string url = Constants.DefaultUrl, int port = Constants.DefaultPort, int bufferCloudEvents = 1) : base(clientId, password, vpn, url, port, bufferCloudEvents) { }
	
		
			public Task Publish(AnonymousSchema_1 parameter, CancellationToken cancellationToken = default(CancellationToken))
			{
				const string topicName = "877309ea-d00d-4f47-9df2-db6a41a632ee/user";				
				var cloudEvt = new CloudEvent
				{
					Id = Guid.NewGuid().ToString(),
					Subject = topicName,
					Source = new Uri(parameter.Source),
					Type = parameter.Type,
					DataContentType = "application/json",
					Data = JsonSerializer.Serialize(parameter),
					Time = DateTimeOffset.UtcNow
				};
				return Publish(topicName, cloudEvt, cancellationToken);
			}
			
	}
}