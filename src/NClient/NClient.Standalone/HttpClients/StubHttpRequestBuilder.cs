﻿using System.Threading.Tasks;
using NClient.Abstractions.HttpClients;

namespace NClient.Standalone.HttpClients
{
    public class StubHttpRequestBuilder : IHttpMessageBuilder<IHttpRequest>
    {
        public Task<IHttpRequest> BuildRequestAsync(IHttpRequest httpRequest)
        {
            return Task.FromResult(httpRequest);
        }
    }
}
