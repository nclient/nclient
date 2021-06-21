﻿using System;
using System.Linq;
using System.Reflection;
using NClient.Abstractions.Resilience;
using NClient.Annotations.Methods;
using NClient.Core.Mappers;
using NClient.Mappers;
using Polly;
using Polly.Wrap;

namespace NClient.Resilience
{
    internal abstract class PollyMethodResiliencePolicyProviderBase : IMethodResiliencePolicyProvider
    {
        protected readonly AsyncPolicyWrap<ResponseContext> Policy;

        protected PollyMethodResiliencePolicyProviderBase()
        {
            var basePolicy = Policy<ResponseContext>.HandleResult(x => !x.HttpResponse.IsSuccessful).Or<Exception>();
            var retryPolicy = basePolicy.WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var fallbackPolicy = basePolicy.FallbackAsync(
                fallbackValue: default!,
                onFallbackAsync: x => throw (x.Exception ?? x.Result.HttpResponse.ErrorException!));

            Policy = fallbackPolicy.WrapAsync(retryPolicy);
        }

        protected static MethodAttribute GetMethodAttributeFor(MethodInfo methodInfo)
        {
            // TODO: It is better to pass it through the constructor, but how?
            IAttributeMapper attributeMapper = methodInfo.DeclaringType!.IsClass
                ? new AspNetAttributeMapper()
                : new AttributeMapper();

            return (MethodAttribute)methodInfo.GetCustomAttributes()
                .Select(x => attributeMapper.TryMap(x))
                .Single(x => x is MethodAttribute)!;
        }

        public abstract IResiliencePolicy Create(MethodInfo methodInfo);
    }
}