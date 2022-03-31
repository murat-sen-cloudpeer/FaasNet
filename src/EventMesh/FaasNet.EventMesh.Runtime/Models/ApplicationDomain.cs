﻿using FaasNet.Domain;
using System;
using System.Collections.Generic;

namespace FaasNet.EventMesh.Runtime.Models
{
    public class ApplicationDomain : AggregateRoot
    {
        public ApplicationDomain()
        {
            Applications = new List<Application>();
            IntegrationEvents = new List<IntegrationEvent>();
        }

        #region Properties

        public string Vpn { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RootTopic { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public ICollection<Application> Applications { get; set; }
        public override string Topic => "";

        #endregion

        public static ApplicationDomain Create(string vpn, string name, string description, string rootTopic)
        {
            var result = new ApplicationDomain
            {
                Id = Guid.NewGuid().ToString(),
                Vpn = vpn,
                Name = name,
                Description = description,
                RootTopic = rootTopic,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            };
            return result;
        }

        public override void Handle(dynamic evt)
        {
        }
    }
}
