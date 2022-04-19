﻿using Elasticsearch.Net;
using FaasNet.Common;
using FaasNet.EventStore.EF;
using FaasNet.StateMachine.Exporter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.IO;

namespace FaasNet.StateMachine.ExporterHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StateMachineExporter.db");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var config = builder.Build();
            // RemoveAllIndexes(config);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddStateMachineExporter()
                .UseEventStoreDB(opt =>
                {
                    opt.ConnectionString = config["EventStoreDBConnectionString"];
                })
                .UseStateMachineInstanceElasticSearchStore(opt =>
                {
                    var connectionSettings = new ConnectionSettings(new Uri(config["ElasticSearchUrl"]));
                    connectionSettings.ServerCertificateValidationCallback((o, certificate, chain, errors) => true);
                    connectionSettings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
                    opt.Settings = connectionSettings;
                })
                .UseEF(opt =>
                {
                    opt.UseSqlite($"Data Source={dbPath}", s => s.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
                });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<EventStoreDBContext>())
                {
                    context.Database.Migrate();
                    context.SaveChanges();
                }
            }

            var hostedJob = serviceProvider.GetRequiredService<IProjectionHostedJob>();
            hostedJob.Start();
            Console.WriteLine("Please press enter to quit the application...");
            Console.ReadLine();
            hostedJob.Stop();
        }

        private static void RemoveAllIndexes(IConfigurationRoot config)
        {
            var connectionSettings = new ConnectionSettings(new Uri(config["ElasticSearchUrl"]));
            connectionSettings.ServerCertificateValidationCallback((o, certificate, chain, errors) => true);
            connectionSettings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            var esClient = new ElasticClient(connectionSettings);
            esClient.Indices.Delete("statemachineinstance");
        }
    }
}
