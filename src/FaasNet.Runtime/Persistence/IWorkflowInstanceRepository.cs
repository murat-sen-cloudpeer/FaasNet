﻿using FaasNet.Runtime.Domains;
using FaasNet.Runtime.Domains.Instances;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FaasNet.Runtime.Persistence
{
    public interface IWorkflowInstanceRepository
    {
        IQueryable<WorkflowInstanceAggregate> Query();
        Task Add(WorkflowInstanceAggregate workflowInstance, CancellationToken cancellationToken);
        Task Update(WorkflowInstanceAggregate workflowInstance, CancellationToken cancellationToken);
        Task<int> SaveChanges(CancellationToken cancellationToken);
    }
}
