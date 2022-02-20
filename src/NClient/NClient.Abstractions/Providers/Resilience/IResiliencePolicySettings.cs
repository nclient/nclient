﻿using System;
using NClient.Providers.Transport;

namespace NClient.Providers.Resilience
{
    /// <summary>The settings for resilience policy provider.</summary>
    public interface IResiliencePolicySettings<TRequest, TResponse>
    {
        /// <summary>The max number of retries.</summary>
        int MaxRetries { get; }
        
        /// <summary>The function that provides the duration to wait for for a particular retry attempt.</summary>
        Func<int, TimeSpan> GetDelay { get; }
        
        /// <summary>The predicate to filter the results this policy will handle.</summary>
        Func<IResponseContext<TRequest, TResponse>, bool> ShouldRetry { get; }
    }
}
