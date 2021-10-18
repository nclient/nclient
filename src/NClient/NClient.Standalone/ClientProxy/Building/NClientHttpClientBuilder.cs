﻿using NClient.Abstractions.Building;
using NClient.Abstractions.HttpClients;
using NClient.Common.Helpers;
using NClient.Standalone.ClientProxy.Building.Context;

namespace NClient.Standalone.ClientProxy.Building
{
    internal class NClientHttpClientBuilder<TClient> : INClientHttpClientBuilder<TClient> where TClient : class
    {
        private readonly string _host;
        
        public NClientHttpClientBuilder(string host)
        {
            _host = host;
        }
        
        public INClientSerializerBuilder<TClient, TRequest, TResponse> UsingCustomHttpClient<TRequest, TResponse>(
            IHttpClientProvider<TRequest, TResponse> httpClientProvider, 
            IHttpMessageBuilderProvider<TRequest, TResponse> httpMessageBuilderProvider)
        {
            Ensure.IsNotNull(httpClientProvider, nameof(httpClientProvider));
            Ensure.IsNotNull(httpMessageBuilderProvider, nameof(httpMessageBuilderProvider));
            
            return new NClientSerializerBuilder<TClient, TRequest, TResponse>(new BuilderContext<TRequest, TResponse>()
                .WithHost(_host)
                .WithHttpClientProvider(httpClientProvider, httpMessageBuilderProvider));
        }
    }
}
