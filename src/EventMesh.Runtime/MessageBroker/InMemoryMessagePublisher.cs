﻿using CloudNative.CloudEvents;
using EventMesh.Runtime.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EventMesh.Runtime.MessageBroker
{
    public class InMemoryMessagePublisher : IMessagePublisher
    {
        private readonly ConcurrentBag<InMemoryTopic> _topics;

        public InMemoryMessagePublisher(ConcurrentBag<InMemoryTopic> topics)
        {
            _topics = topics;
        }

        public string BrokerName => Constants.InMemoryBrokername;

        public Task Publish(CloudEvent cloudEvent, string topicName, Client client)
        {
            var topic = _topics.First(t => t.TopicName == topicName);
            topic.PublishMessage(cloudEvent);
            return Task.CompletedTask;
        }
    }
}
