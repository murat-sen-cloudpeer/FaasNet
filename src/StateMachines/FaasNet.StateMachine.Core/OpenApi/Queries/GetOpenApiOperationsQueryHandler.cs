﻿using FaasNet.Domain.Exceptions;
using FaasNet.StateMachine.Core.Resources;
using FaasNet.StateMachine.Runtime.OpenAPI;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FaasNet.StateMachine.Core.OpenApi.Queries
{
    public class GetOpenApiOperationsQueryHandler : IRequestHandler<GetOpenApiOperationsQuery, IEnumerable<OpenApiOperationResult>>
    {
        private readonly IOpenAPIParser _openAPIParser;

        public GetOpenApiOperationsQueryHandler(IOpenAPIParser openAPIParser)
        {
            _openAPIParser = openAPIParser;
        }

        public async Task<IEnumerable<OpenApiOperationResult>> Handle(GetOpenApiOperationsQuery request, CancellationToken cancellationToken)
        {
            if (_openAPIParser.TryParseUrl(request.Endpoint, out OpenAPIUrlResult result))
            {
                throw new BadRequestException(ErrorCodes.INVALID_OPENAPI_URL, Global.InvalidOpenApiUrl);
            }

            var configuration = await _openAPIParser.GetConfiguration(request.Endpoint, cancellationToken);
            var operations = configuration.Paths.SelectMany(p => p.Value.Select(kvp => kvp.Value));
            return operations.Select(o => new OpenApiOperationResult
            {
                OperationId = o.OperationId,
                Summary = o.Summary
            });
        }
    }

    public class GetOpenApiOperationsQuery : IRequest<IEnumerable<OpenApiOperationResult>>
    {
        public string Endpoint { get; set; }
    }

    public class OpenApiOperationResult
    {
        public string OperationId { get; set; }
        public string Summary { get; set; }
    }
}