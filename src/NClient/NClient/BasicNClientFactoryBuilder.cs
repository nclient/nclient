﻿using System.Net.Http;
using NClient.Abstractions.Building;

namespace NClient
{
    public interface IBasicNClientFactoryBuilder
    {
        INClientFactoryOptionalBuilder<HttpRequestMessage, HttpResponseMessage> For(string factoryName);
    }
    
    /// <summary>
    /// The builder used to create the client factory.
    /// </summary>
    public class BasicNClientFactoryBuilder : IBasicNClientFactoryBuilder
    {
        public INClientFactoryOptionalBuilder<HttpRequestMessage, HttpResponseMessage> For(string factoryName)
        {
            return new CustomNClientFactoryBuilder()
                .For(factoryName)
                .UsingHttpClient()
                .UsingJsonSerializer()
                .EnsuringSuccess()
                .WithoutHandling()
                .WithoutResilience()
                .WithResults()
                .WithoutLogging();
        }
    }
}
