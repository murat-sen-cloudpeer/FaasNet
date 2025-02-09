﻿using FaasNet.Runtime.Domains.Definitions;
using FaasNet.Runtime.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaasNet.Runtime.EF.Configurations
{
    public class BaseEventConditionConfiguration : IEntityTypeConfiguration<BaseEventCondition>
    {
        public void Configure(EntityTypeBuilder<BaseEventCondition> builder)
        {
            builder.Property<int>("Id").ValueGeneratedOnAdd();
            builder.HasKey("Id");
            builder.HasDiscriminator(_ => _.ConditionType)
                .HasValue<WorkflowDefinitionSwitchDataCondition>(WorkflowDefinitionEventConditionTypes.DATA)
                .HasValue<WorkflowDefinitionSwitchEventCondition>(WorkflowDefinitionEventConditionTypes.EVENT);
        }
    }
}
