﻿using System.Net.Http;
using NClient.Providers.Results.HttpResponses;

// ReSharper disable once CheckNamespace
namespace NClient
{
    public static class HttpResultsExtensions
    {
        public static INClientOptionalBuilder<TClient, HttpRequestMessage, HttpResponseMessage> WithHttpResults<TClient>(
            this INClientOptionalBuilder<TClient, HttpRequestMessage, HttpResponseMessage> optionalBuilder) 
            where TClient : class
        {
            return optionalBuilder.WithAdvancedResults(x => x
                .ForTransport().Use(new HttpResponseBuilderProvider()));
        }
        
        public static INClientFactoryOptionalBuilder<HttpRequestMessage, HttpResponseMessage> WithHttpResults(
            this INClientFactoryOptionalBuilder<HttpRequestMessage, HttpResponseMessage> optionalBuilder)
        {
            return optionalBuilder.WithAdvancedResults(x => x
                .ForTransport().Use(new HttpResponseBuilderProvider()));
        }
    }
}
