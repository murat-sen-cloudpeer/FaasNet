﻿using CloudNative.CloudEvents;
using FaasNet.EventMesh.Runtime.Exceptions;
using FaasNet.EventMesh.Runtime.MessageBroker;
using FaasNet.EventMesh.Runtime.Messages;
using FaasNet.EventMesh.Runtime.Models;
using FaasNet.EventMesh.Runtime.Stores;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FaasNet.EventMesh.Runtime.Tests
{
    public class EventMeshRuntimeHostFixture
    {
        [Fact]
        public async Task When_SendHeartBeat_Then_SuccessfulResponseIsReturned()
        {
            // ARRANGE
            var builder = new RuntimeHostBuilder();
            var host = builder.Build();
            host.Run();

            // ACT
            var client = new RuntimeClient();
            var response = await client.HeartBeat();
            host.Stop();

            // ASSERT
            Assert.Equal(Commands.HEARTBEAT_RESPONSE, response.Header.Command);
            Assert.Equal(HeaderStatus.SUCCESS.Code, response.Header.Status.Code);
            Assert.Equal(HeaderStatus.SUCCESS.Desc, response.Header.Status.Desc);
        }

        #region Hello Request

        [Fact]
        public async Task When_SendHello_And_VpnDoesntExist_Then_ErrorIsReturned()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.SUB });
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5010;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 5010);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                Vpn = "wrongVpn"
            }));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.UNKNOWN_VPN, exception.Error);

        }

        [Fact]
        public async Task When_SendHello_And_ClientDoesntExist_Then_Error_IsReturned()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.SUB });
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5011;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 5011);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.Hello(new UserAgent
            {
                ClientId = "wrongClientId",
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                Vpn = vpn.Name
            }));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.INVALID_CLIENT, exception.Error);
        }

        [Fact]
        public async Task When_SendHello_And_ClientDoesntHaveCorrectPurpose_Then_Error_IsReturned()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.SUB });
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5012;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 5012);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.PUB,
                Version = "0",
                Vpn = vpn.Name
            }));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.NOT_AUTHORIZED, exception.Error);
        }

        [Fact]
        public async Task When_SendHello_Then_SessionIsCreated()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.SUB });
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4990;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 4990);
            var response = await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                Vpn = vpn.Name
            });
            host.Stop();

            // ASSERT
            Assert.Equal(Commands.HELLO_RESPONSE, response.Header.Command);
            Assert.Equal(HeaderStatus.SUCCESS.Code, response.Header.Status.Code);
            Assert.Equal(HeaderStatus.SUCCESS.Desc, response.Header.Status.Desc);
        }

        #endregion

        #region Disconnect

        [Fact]
        public async Task When_Disconnect_And_ThereIsNoActiveSession_Then_ErrorIsReturned()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn");
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4990;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 4990);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.Disconnect(newClient.ClientId, "sessionid"));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.INVALID_SESSION, exception.Error);
        }

        [Fact]
        public async Task When_Disconnect_Then_SessionIsNotActive()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn");
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4995;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 4995);
            var helloResponse = await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                Vpn = vpn.Name
            });
            var disconnectResponse = await client.Disconnect(newClient.ClientId, helloResponse.SessionId);
            host.Stop();

            // ASSERT
            Assert.Equal(Commands.DISCONNECT_RESPONSE, disconnectResponse.Header.Command);
            Assert.Equal(HeaderStatus.SUCCESS, disconnectResponse.Header.Status);
        }

        #endregion

        #region Add Bridge

        [Fact]
        public async Task When_Add_NotReachableBridge_Then_ErrorIsReturned()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn");
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4996;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 4996);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.AddBridge(vpn.Name, "urn", 4997, vpn.Name, CancellationToken.None));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.INVALID_BRIDGE, exception.Error);
        }

        [Fact]
        public async Task When_AddSameBridgeTwice_Then_ErrorIsReturned()
        {
            // ARRANGE
            var firstVpn = Vpn.Create("default", "default");
            var secondVpn = Vpn.Create("default", "default");
            var firstVpnClient = Client.Create(firstVpn.Name, "clientId", "urn");
            var secondVpnClient = Client.Create(secondVpn.Name, "clientId", "urn");
            var firstBuilder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4997;
                opt.Urn = "localhost";
            });
            var secondBuilder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4998;
                opt.Urn = "localhost";
            });
            var firstHost = firstBuilder.AddVpns(new List<Vpn> { firstVpn }).AddClients(new List<Client> { firstVpnClient }).Build();
            firstHost.Run();
            var secondHost = secondBuilder.AddVpns(new List<Vpn> { secondVpn }).AddClients(new List<Client> { secondVpnClient }).Build();
            secondHost.Run();

            // ACT
            var client = new RuntimeClient(port: 4997);
            await client.AddBridge(firstVpn.Name, "localhost", 4998, secondVpn.Name, CancellationToken.None);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.AddBridge(firstVpn.Name, "localhost", 4998, secondVpn.Name, CancellationToken.None));
            firstHost.Stop();
            secondHost.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.BRIDGE_EXISTS, exception.Error);
        }

        #endregion

        #region Subscribe

        [Fact]
        public async Task When_Subscribe_And_CloseUdpClient_Then_SessionIsRemoved()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn");
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4991;
            });
            var serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 4991);
            var response = await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                Vpn = vpn.Name
            });
            client.UdpClient.Close();
            var exception = await Assert.ThrowsAsync<RuntimeClientSessionClosedException>(async () => await client.Subscribe(newClient.ClientId, response.SessionId, new List<SubscriptionItem>
            {
                new SubscriptionItem
                {
                    Topic = "topic"
                }
            }));
            host.Stop();

            // ASSERT
            var lastSession = newClient.Sessions.First();
            Assert.False(newClient.ActiveSessions.Any());
            Assert.True(newClient.Sessions.Count() == 1);
            Assert.Equal(ClientSessionState.FINISH, lastSession.State);
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task When_Subscribe_And_ThereIsNoActiveSession_Then_ExceptionIsThrown()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn");
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4992;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 4992);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.Subscribe(newClient.ClientId, "sessionId", new List<SubscriptionItem>
            {
                new SubscriptionItem
                {
                    Topic = "topic"
                }
            }));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.INVALID_SESSION, exception.Error);
        }

        [Fact]
        public async Task When_Subscribe_And_SessionDoesntHaveCorrectPurpose_ThenExceptionIsThrown()
        {
            // ARRANGE
            const string topicName = "Test.COUCOU";
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.PUB });
            AsyncMessageToClient msg = null;
            var cloudEvent = new CloudEvent
            {
                Type = "com.github.pull.create",
                Source = new Uri("https://github.com/cloudevents/spec/pull"),
                Subject = "123",
                Id = "A234-1234-1234",
                Time = new DateTimeOffset(2018, 4, 5, 17, 31, 0, TimeSpan.Zero),
                DataContentType = "application/json",
                Data = "testttt",
                ["comexampleextension1"] = "value"
            };
            var topics = new ConcurrentBag<InMemoryTopic>
            {
                new InMemoryTopic
                {
                    TopicName = topicName
                }
            };
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5003;
            });
            builder.AddInMemoryMessageBroker(topics);
            var serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            var messagePublisher = serviceProvider.GetRequiredService<IMessagePublisher>();
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 5003);
            var helloResponse = await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.PUB,
                Version = "0",
                BufferCloudEvents = 1,
                Vpn = vpn.Name
            });
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.Subscribe(newClient.ClientId, helloResponse.SessionId, new List<SubscriptionItem>
            {
                new SubscriptionItem
                {
                    Topic = topicName
                }
            }));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.UNAUTHORIZED_SUBSCRIBE, exception.Error);
        }

        [Fact]
        public async Task When_Subscribe_ToOneTopic_Then_MessagesAreReceived()
        {
            // ARRANGE
            const string topicName = "Test.COUCOU";
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.SUB });
            AsyncMessageToClient msg = null;
            var cloudEvent = new CloudEvent
            {
                Type = "com.github.pull.create",
                Source = new Uri("https://github.com/cloudevents/spec/pull"),
                Subject = "123",
                Id = "A234-1234-1234",
                Time = new DateTimeOffset(2018, 4, 5, 17, 31, 0, TimeSpan.Zero),
                DataContentType = "application/json",
                Data = "testttt",
                ["comexampleextension1"] = "value"
            };
            var topics = new ConcurrentBag<InMemoryTopic>
            {
                new InMemoryTopic
                {
                    TopicName = topicName
                }
            };
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 4993;
            });
            builder.AddInMemoryMessageBroker(topics);
            var serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            var messagePublisher = serviceProvider.GetRequiredService<IMessagePublisher>();
            var vpnStore = serviceProvider.GetRequiredService<IVpnStore>();
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 4993);
            var helloResponse = await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                BufferCloudEvents = 1,
                Vpn = vpn.Name
            });
            await client.Subscribe(newClient.ClientId, helloResponse.SessionId, new List<SubscriptionItem>
            {
                new SubscriptionItem
                {
                    Topic = topicName
                }
            }, (m) =>
            {
                msg = m;
            });
            await messagePublisher.Publish(cloudEvent, topicName, newClient);
            while (msg == null)
            {
                Thread.Sleep(100);
            }

            // ASSERT
            var topic = newClient.Topics.First();
            Assert.NotNull(msg);
            Assert.Equal(Constants.InMemoryBrokername, topic.BrokerName);
            Assert.Equal(1, topic.Offset);
            Assert.Equal(topicName, topic.Name);
        }

        [Fact]
        public async Task When_AddBridge_And_Subscribe_ToOneTopic_Then_MessageIsReceivedFromSecondServer()
        {
            // ARRANGE
            const string topicName = "Test.COUCOU";
            var firstVpn = Vpn.Create("default", "default");
            var secondVpn = Vpn.Create("default", "default");
            var firstVpnClient = Client.Create(firstVpn.Name, "clientId", "urn");
            var secondVpnClient = Client.Create(secondVpn.Name, "clientId", "urn");
            AsyncMessageToClient msg = null;
            var cloudEvent = new CloudEvent
            {
                Type = "com.github.pull.create",
                Source = new Uri("https://github.com/cloudevents/spec/pull"),
                Subject = "123",
                Id = "A234-1234-1234",
                Time = new DateTimeOffset(2018, 4, 5, 17, 31, 0, TimeSpan.Zero),
                DataContentType = "application/json",
                Data = "testttt",
                ["comexampleextension1"] = "value"
            };
            var topics = new ConcurrentBag<InMemoryTopic>
            {
                new InMemoryTopic
                {
                    TopicName = topicName
                }
            };
            var firstBuilder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5001;
                opt.Urn = "localhost";
            });
            var secondBuilder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5002;
                opt.Urn = "localhost";
            }).AddInMemoryMessageBroker(topics);
            var firstServiceProvider = firstBuilder.ServiceCollection.BuildServiceProvider();
            var secondServiceProvider = secondBuilder.ServiceCollection.BuildServiceProvider();
            var messagePublisher = secondServiceProvider.GetRequiredService<IMessagePublisher>();
            var firstHost = firstBuilder.AddVpns(new List<Vpn> { firstVpn }).AddClients(new List<Client> { firstVpnClient }).Build();
            firstHost.Run();
            var secondHost = secondBuilder.AddVpns(new List<Vpn> { secondVpn }).AddClients(new List<Client> { secondVpnClient }).Build();
            secondHost.Run();

            // ACT
            var client = new RuntimeClient(port: 5001);
            await client.AddBridge(firstVpn.Name, "localhost", 5002, secondVpn.Name, CancellationToken.None);
            var helloResponse = await client.Hello(new UserAgent
            {
                ClientId = firstVpnClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                BufferCloudEvents = 1,
                Vpn = firstVpn.Name
            });
            var subscriptionResult = await client.Subscribe(firstVpnClient.ClientId, helloResponse.SessionId, new List<SubscriptionItem>
            {
                new SubscriptionItem
                {
                    Topic = topicName
                }
            }, (m) =>
            {
                msg = m;
            });
            await messagePublisher.Publish(cloudEvent, topicName, secondVpnClient);
            while (msg == null)
            {
                Thread.Sleep(100);
            }
            subscriptionResult.Stop();

            await client.Disconnect(firstVpnClient.ClientId, helloResponse.SessionId);
            firstHost.Stop();
            secondHost.Stop();

            // ASSERT
            var firstServerClient = firstVpnClient;
            var secondServerClient = secondVpnClient;
            var secondTopic = secondServerClient.Topics.First();
            var firstSession = firstServerClient.Sessions.First();
            var secondSession = secondServerClient.Sessions.First();
            Assert.Equal(ClientSessionState.FINISH, firstSession.State);
            Assert.Equal(ClientSessionState.FINISH, secondSession.State);
            Assert.Equal(1, secondTopic.Offset);
            Assert.Equal("inmemory", secondTopic.BrokerName);
            Assert.Equal("Test.COUCOU", secondTopic.Name);
        }

        #endregion

        #region Publish Message

        [Fact]
        public async Task When_PublishMessage_And_ThereIsNoActiveSession_Then_ExceptionIsThrown()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn");
            var cloudEvent = new CloudEvent
            {
                Type = "com.github.pull.create",
                Source = new Uri("https://github.com/cloudevents/spec/pull"),
                Subject = "123",
                Id = "A234-1234-1234",
                Time = new DateTimeOffset(2018, 4, 5, 17, 31, 0, TimeSpan.Zero),
                DataContentType = "application/json",
                Data = "testttt",
                ["comexampleextension1"] = "value"
            };
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5004;
            });
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 5004);
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.PublishMessage(newClient.ClientId, "sessionId", "topic", cloudEvent));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.INVALID_SESSION, exception.Error);
        }

        [Fact]
        public async Task When_PublishMessage_And_SessionDoesntHaveCorrectPurpose_ThenExceptionIsThrown()
        {
            // ARRANGE
            var vpn = Vpn.Create("default", "default");
            var newClient = Client.Create(vpn.Name, "clientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.SUB });
            const string topicName = "Test.COUCOU";
            var cloudEvent = new CloudEvent
            {
                Type = "com.github.pull.create",
                Source = new Uri("https://github.com/cloudevents/spec/pull"),
                Subject = "123",
                Id = "A234-1234-1234",
                Time = new DateTimeOffset(2018, 4, 5, 17, 31, 0, TimeSpan.Zero),
                DataContentType = "application/json",
                Data = "testttt",
                ["comexampleextension1"] = "value"
            };
            var topics = new ConcurrentBag<InMemoryTopic>
            {
                new InMemoryTopic
                {
                    TopicName = topicName
                }
            };
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5005;
            });
            builder.AddInMemoryMessageBroker(topics);
            var serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            var messagePublisher = serviceProvider.GetRequiredService<IMessagePublisher>();
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { newClient }).Build();
            host.Run();

            // ACT
            var client = new RuntimeClient(port: 5005);
            var helloResponse = await client.Hello(new UserAgent
            {
                ClientId = newClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                BufferCloudEvents = 1,
                Vpn = vpn.Name
            });
            var exception = await Assert.ThrowsAsync<RuntimeClientResponseException>(async () => await client.PublishMessage(newClient.ClientId, helloResponse.SessionId, "topic", cloudEvent));
            host.Stop();

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(HeaderStatus.FAIL, exception.Status);
            Assert.Equal(Errors.UNAUTHORIZED_PUBLISH, exception.Error);
        }

        [Fact]
        public async Task When_PublishMessage_And_SubscribeToOneTopic_Then_MessageIsReceived()
        {
            // ARRANGE
            const string topicName = "Test.COUCOU";
            var vpn = Vpn.Create("default", "default");
            var firstSubClient = Client.Create(vpn.Name, "subClientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.SUB });
            var firstPubClient = Client.Create(vpn.Name, "pubClientId", "urn", new List<UserAgentPurpose> { UserAgentPurpose.PUB });
            AsyncMessageToClient msg = null;
            var cloudEvent = new CloudEvent
            {
                Type = "com.github.pull.create",
                Source = new Uri("https://github.com/cloudevents/spec/pull"),
                Subject = "123",
                Id = "A234-1234-1234",
                Time = new DateTimeOffset(2018, 4, 5, 17, 31, 0, TimeSpan.Zero),
                DataContentType = "application/json",
                Data = "testttt",
                ["comexampleextension1"] = "value"
            };
            var topics = new ConcurrentBag<InMemoryTopic>
            {
                new InMemoryTopic
                {
                    TopicName = topicName
                }
            };
            var builder = new RuntimeHostBuilder(opt =>
            {
                opt.Port = 5006;
            });
            builder.AddInMemoryMessageBroker(topics);
            var serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            var host = builder.AddVpns(new List<Vpn> { vpn }).AddClients(new List<Client> { firstSubClient, firstPubClient }).Build();
            host.Run();

            // ACT
            var subClient = new RuntimeClient(port: 5006);
            var subClientHelloResponse = await subClient.Hello(new UserAgent
            {
                ClientId = firstSubClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.SUB,
                Version = "0",
                BufferCloudEvents = 1,
                Vpn = vpn.Name
            });
            await subClient.Subscribe(firstSubClient.ClientId, subClientHelloResponse.SessionId, new List<SubscriptionItem>
            {
                new SubscriptionItem
                {
                    Topic = topicName
                }
            }, (m) =>
            {
                msg = m;
            });
            var pubClient = new RuntimeClient(port: 5006);
            var pubClientHelloResponse = await pubClient.Hello(new UserAgent
            {
                ClientId = firstPubClient.ClientId,
                Environment = "TST",
                Password = "password",
                Pid = 2000,
                Purpose = UserAgentPurpose.PUB,
                Version = "0",
                BufferCloudEvents = 1,
                Vpn = vpn.Name
            });
            await pubClient.PublishMessage(firstPubClient.ClientId, pubClientHelloResponse.SessionId, topicName, cloudEvent);
            while (msg == null)
            {
                Thread.Sleep(100);
            }

            host.Stop();

            // ASSERT
            var topic = firstSubClient.Topics.First();
            Assert.NotNull(msg);
            Assert.Equal(Constants.InMemoryBrokername, topic.BrokerName);
            Assert.Equal(1, topic.Offset);
            Assert.Equal(topicName, topic.Name);
        }

        #endregion
    }
}
