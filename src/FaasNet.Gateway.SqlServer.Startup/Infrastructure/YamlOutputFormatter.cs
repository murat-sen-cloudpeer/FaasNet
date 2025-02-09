﻿using FaasNet.Runtime.Domains.Definitions;
using FaasNet.Runtime.Serializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FaasNet.Gateway.SqlServer.Startup.Infrastructure
{
    public class YamlOutputFormatter : TextOutputFormatter
    {
        public YamlOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/yaml"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return context.ContentType == "text/yaml";
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            string yaml = null;
            if(context.Object is WorkflowDefinitionAggregate)
            {
                var serializer = new RuntimeSerializer();
                yaml = serializer.SerializeYaml(context.Object as WorkflowDefinitionAggregate);

            }
            else
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                yaml = serializer.Serialize(context.Object);
            }

            await context.HttpContext.Response.WriteAsync(yaml, selectedEncoding);
        }
    }
}
