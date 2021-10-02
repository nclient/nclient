﻿namespace NClient.Abstractions
{
    /// <summary>
    /// A factory abstraction used to create the client with custom providers.
    /// </summary>
    public interface INClientFactory
    {
        /// <summary>
        /// Sets the main client settings.
        /// </summary>
        /// <param name="host">The base address of URI used when sending requests.</param>
        /// <typeparam name="TInterface">The type of interface of controller used to create the client.</typeparam>
        TInterface Create<TInterface>(string host)
            where TInterface : class;
    }
}
