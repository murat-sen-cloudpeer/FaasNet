﻿using EventMesh.Runtime.EF;
using EventMesh.Runtime.EF.Stores;
using EventMesh.Runtime.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRuntimeEF(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> options = null, bool isScoped = false)
        {
            var clientStoreType = serviceCollection.FirstOrDefault(s => s.ServiceType == typeof(IClientStore));
            var bridgeServerStoreType = serviceCollection.FirstOrDefault(s => s.ServiceType == typeof(IBridgeServerStore));
            var brokerConfigurationStoreType = serviceCollection.FirstOrDefault(s => s.ServiceType == typeof(IBrokerConfigurationStore));
            if (clientStoreType != null)
                serviceCollection.Remove(clientStoreType);
            if (bridgeServerStoreType != null)
                serviceCollection.Remove(bridgeServerStoreType);
            if (brokerConfigurationStoreType != null)
                serviceCollection.Remove(brokerConfigurationStoreType);
            if (!isScoped)
            {
                serviceCollection.AddTransient<IClientStore, EFClientStore>();
                serviceCollection.AddTransient<IBridgeServerStore, EFBridgeServerStore>();
                serviceCollection.AddTransient<IBrokerConfigurationStore, EFBrokerConfigurationStore>();
            }
            else
            {
                serviceCollection.AddScoped<IClientStore, EFClientStore>();
                serviceCollection.AddScoped<IBridgeServerStore, EFBridgeServerStore>();
                serviceCollection.AddScoped<IBrokerConfigurationStore, EFBrokerConfigurationStore>();
            }

            serviceCollection.AddDbContext<EventMeshDBContext>(options);
            return serviceCollection;
        }
    }
}
