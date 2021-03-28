﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NClient.Abstractions.HttpClients;
using NClient.Abstractions.Resilience;
using NClient.AspNetProxy;
using NClient.AspNetProxy.Extensions;
using NClient.InterfaceProxy;
using NClient.InterfaceProxy.Extensions;
using NClient.Providers.HttpClient.RestSharp;
using RestSharp.Authenticators;
using Polly;

namespace NClient.Extensions.DependencyInjection
{
    public static class AddNClientExtensions
    {
        public static IServiceCollection AddNClient<TInterface>(this IServiceCollection serviceCollection,
            string host, IAuthenticator authenticator, IAsyncPolicy asyncPolicy)
            where TInterface : class
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientBuilder()
                    .Use<TInterface>(host, authenticator)
                    .WithResiliencePolicy(asyncPolicy)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface>(this IServiceCollection serviceCollection,
            string host, IAuthenticator authenticator)
            where TInterface : class
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientBuilder()
                    .Use<TInterface>(host, authenticator)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface>(this IServiceCollection serviceCollection,
            string host, IAsyncPolicy asyncPolicy)
            where TInterface : class
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientBuilder()
                    .Use<TInterface>(host)
                    .WithResiliencePolicy(asyncPolicy)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface>(this IServiceCollection serviceCollection,
            string host)
            where TInterface : class
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientBuilder()
                    .Use<TInterface>(host, new RestSharpHttpClientProvider())
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface>(this IServiceCollection serviceCollection,
            Func<INClientBuilder, INClientBuilder<TInterface>> configure)
            where TInterface : class
        {
            return serviceCollection.AddSingleton(_ => configure(new NClientBuilder()).Build());
        }

        public static IServiceCollection AddNClient<TInterface>(this IServiceCollection serviceCollection,
            string host, IHttpClientProvider httpClientProvider, IResiliencePolicyProvider resiliencePolicyProvider)
            where TInterface : class
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientBuilder()
                    .Use<TInterface>(host, httpClientProvider)
                    .WithResiliencePolicy(resiliencePolicyProvider)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface>(this IServiceCollection serviceCollection,
            string host, IHttpClientProvider httpClientProvider)
            where TInterface : class
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientBuilder()
                    .Use<TInterface>(host, httpClientProvider)
                    .WithLogging(logger)
                    .Build();
            });
        }



        public static IServiceCollection AddNClient<TInterface, TController>(this IServiceCollection serviceCollection,
            string host, IAuthenticator authenticator, IAsyncPolicy asyncPolicy)
            where TInterface : class
            where TController : TInterface
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientControllerBuilder()
                    .Use<TInterface, TController>(host, authenticator)
                    .WithResiliencePolicy(asyncPolicy)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface, TController>(this IServiceCollection serviceCollection,
            string host, IAuthenticator authenticator)
            where TInterface : class
            where TController : TInterface
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientControllerBuilder()
                    .Use<TInterface, TController>(host, authenticator)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface, TController>(this IServiceCollection serviceCollection,
            string host, IAsyncPolicy asyncPolicy)
            where TInterface : class
            where TController : TInterface
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientControllerBuilder()
                    .Use<TInterface, TController>(host)
                    .WithResiliencePolicy(asyncPolicy)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface, TController>(this IServiceCollection serviceCollection,
            string host)
            where TInterface : class
            where TController : TInterface
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientControllerBuilder()
                    .Use<TInterface, TController>(host)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface, TController>(this IServiceCollection serviceCollection,
            Func<INClientControllerBuilder, INClientControllerBuilder<TInterface, TController>> configure)
            where TInterface : class
            where TController : TInterface
        {
            return serviceCollection.AddSingleton(_ => configure(new NClientControllerBuilder()).Build());
        }

        public static IServiceCollection AddNClient<TInterface, TController>(this IServiceCollection serviceCollection,
            string host, IHttpClientProvider httpClientProvider, IResiliencePolicyProvider resiliencePolicyProvider)
            where TInterface : class
            where TController : TInterface
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientControllerBuilder()
                    .Use<TInterface, TController>(host, httpClientProvider)
                    .WithResiliencePolicy(resiliencePolicyProvider)
                    .WithLogging(logger)
                    .Build();
            });
        }

        public static IServiceCollection AddNClient<TInterface, TController>(this IServiceCollection serviceCollection,
            string host, IHttpClientProvider httpClientProvider)
            where TInterface : class
            where TController : TInterface
        {
            return serviceCollection.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TInterface>>();
                return new NClientControllerBuilder()
                    .Use<TInterface, TController>(host, httpClientProvider)
                    .WithLogging(logger)
                    .Build();
            });
        }
    }
}
