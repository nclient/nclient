﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NClient.Providers.Handling;
using NClient.Providers.Mapping;
using NClient.Providers.Resilience;
using NClient.Providers.Serialization;
using NClient.Providers.Transport;
using NClient.Providers.Validation;

namespace NClient.Standalone.Client
{
    internal interface ITransportNClient<TRequest, TResponse>
    {
        Task<TResult> GetResultAsync<TResult>(IRequest request, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<TResponse> GetOriginalResponseAsync(IRequest request, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<IResponse> GetHttpResponseAsync(IRequest request, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<IResponse<TData>> GetHttpResponseAsync<TData>(IRequest request, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<IResponseWithError<TError>> GetHttpResponseWithErrorAsync<TError>(IRequest request, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<IResponseWithError<TData, TError>> GetHttpResponseWithDataAndErrorAsync<TData, TError>(IRequest request, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task GetResultAsync(IRequest request, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<object?> GetResultAsync(IRequest request, Type dataType, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<IResponse> GetHttpResponseAsync(IRequest request, Type dataType, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<IResponse> GetHttpResponseWithErrorAsync(IRequest request, Type errorType, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
        Task<IResponse> GetHttpResponseWithDataAndErrorAsync(IRequest request, Type dataType, Type errorType, IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default);
    }

    internal class TransportNClient<TRequest, TResponse> : ITransportNClient<TRequest, TResponse>
    {
        private readonly ISerializer _serializer;
        private readonly ITransport<TRequest, TResponse> _transport;
        private readonly ITransportRequestBuilder<TRequest, TResponse> _transportRequestBuilder;
        private readonly IResponseBuilder<TRequest, TResponse> _responseBuilder;
        private readonly IClientHandler<TRequest, TResponse> _clientHandler;
        private readonly IResiliencePolicy<TRequest, TResponse> _resiliencePolicy;
        private readonly IEnumerable<IResponseMapper<TRequest, TResponse>> _typedResultBuilders;
        private readonly IReadOnlyCollection<IResponseMapper<IRequest, IResponse>> _resultBuilders;
        private readonly IResponseValidator<TRequest, TResponse> _responseValidator;
        private readonly ILogger? _logger;

        public TransportNClient(
            ISerializer serializer,
            ITransport<TRequest, TResponse> transport,
            ITransportRequestBuilder<TRequest, TResponse> transportRequestBuilder,
            IResponseBuilder<TRequest, TResponse> responseBuilder,
            IClientHandler<TRequest, TResponse> clientHandler,
            IResiliencePolicy<TRequest, TResponse> resiliencePolicy,
            IEnumerable<IResponseMapper<IRequest, IResponse>> resultBuilders,
            IEnumerable<IResponseMapper<TRequest, TResponse>> typedResultBuilders,
            IResponseValidator<TRequest, TResponse> responseValidator,
            ILogger? logger)
        {
            _serializer = serializer;
            _transport = transport;
            _transportRequestBuilder = transportRequestBuilder;
            _responseBuilder = responseBuilder;
            _clientHandler = clientHandler;
            _resiliencePolicy = resiliencePolicy;
            _typedResultBuilders = typedResultBuilders;
            _resultBuilders = resultBuilders.ToArray();
            _responseValidator = responseValidator;
            _logger = logger;
        }

        public async Task<TResult> GetResultAsync<TResult>(IRequest request, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            #pragma warning disable 8600, 8603
            return (TResult) await GetResultAsync(request, typeof(TResult), resiliencePolicy, cancellationToken).ConfigureAwait(false);
            #pragma warning restore 8600, 8603
        }

        public async Task<TResponse> GetOriginalResponseAsync(IRequest request, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            return (await ExecuteAsync(request, resiliencePolicy, cancellationToken).ConfigureAwait(false)).Response;
        }
        
        public async Task<IResponse> GetHttpResponseAsync(IRequest request, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            var transportResponseContext = await (resiliencePolicy ?? _resiliencePolicy)
                .ExecuteAsync(token => ExecuteAttemptAsync(request, token), cancellationToken)
                .ConfigureAwait(false);
            
            return await _responseBuilder
                .BuildAsync(request, transportResponseContext, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IResponse<TData>> GetHttpResponseAsync<TData>(IRequest request, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            return (IResponse<TData>) await GetHttpResponseAsync(
                    request, dataType: typeof(TData), resiliencePolicy, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IResponseWithError<TError>> GetHttpResponseWithErrorAsync<TError>(IRequest request, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            return (IResponseWithError<TError>) await GetHttpResponseWithErrorAsync(
                    request, errorType: typeof(TError), resiliencePolicy, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IResponseWithError<TData, TError>> GetHttpResponseWithDataAndErrorAsync<TData, TError>(IRequest request, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            return (IResponseWithError<TData, TError>) await GetHttpResponseWithDataAndErrorAsync(
                    request, dataType: typeof(TData), errorType: typeof(TData), resiliencePolicy, cancellationToken)
                .ConfigureAwait(false);
        }
        
        public async Task GetResultAsync(IRequest request, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            var transportResponseContext = await (resiliencePolicy ?? _resiliencePolicy)
                .ExecuteAsync(token => ExecuteAttemptAsync(request, token), cancellationToken)
                .ConfigureAwait(false);
            
            if (!_responseValidator.IsSuccess(transportResponseContext))
                await _responseValidator.OnFailureAsync(transportResponseContext).ConfigureAwait(false);
        }

        public async Task<object?> GetResultAsync(IRequest request, Type dataType, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            var transportResponseContext = await (resiliencePolicy ?? _resiliencePolicy)
                .ExecuteAsync(token => ExecuteAttemptAsync(request, token), cancellationToken)
                .ConfigureAwait(false);

            if (_typedResultBuilders.FirstOrDefault(x => x.CanMap(dataType, transportResponseContext)) is { } typedResultBuilder)
                return await typedResultBuilder
                    .MapAsync(dataType, transportResponseContext, _serializer, cancellationToken)
                    .ConfigureAwait(false);
            
            var response = await _responseBuilder
                .BuildAsync(request, transportResponseContext, cancellationToken)
                .ConfigureAwait(false);
            var responseContext = new ResponseContext<IRequest, IResponse>(request, response);

            if (_resultBuilders.FirstOrDefault(x => x.CanMap(dataType, responseContext)) is { } resultBuilder)
                return await resultBuilder
                    .MapAsync(dataType, responseContext, _serializer, cancellationToken)
                    .ConfigureAwait(false);
            
            if (!_responseValidator.IsSuccess(transportResponseContext))
                await _responseValidator.OnFailureAsync(transportResponseContext).ConfigureAwait(false);
            
            return _serializer.Deserialize(await new StreamReader(response.Content.StreamContent).ReadToEndAsync(), dataType);
        }
        
        public async Task<IResponse> GetHttpResponseAsync(IRequest request, Type dataType, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            var transportResponseContext = await (resiliencePolicy ?? _resiliencePolicy)
                .ExecuteAsync(token => ExecuteAttemptAsync(request, token), cancellationToken)
                .ConfigureAwait(false);
            
            var response = await _responseBuilder
                .BuildAsync(request, transportResponseContext, cancellationToken)
                .ConfigureAwait(false);
            
            var dataObject = TryGetDataObject(dataType, response.Content.StreamContent, transportResponseContext);
            var s = await new StreamReader(response.Content.StreamContent).ReadToEndAsync();
            return BuildResponseWithData(dataObject, dataType, response);
        }
        
        public async Task<IResponse> GetHttpResponseWithErrorAsync(IRequest request, Type errorType, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            var transportResponseContext = await (resiliencePolicy ?? _resiliencePolicy)
                .ExecuteAsync(token => ExecuteAttemptAsync(request, token), cancellationToken)
                .ConfigureAwait(false);
            
            var response = await _responseBuilder
                .BuildAsync(request, transportResponseContext, cancellationToken)
                .ConfigureAwait(false);
            
            var errorObject = TryGetErrorObject(errorType, response.Content.StreamContent, transportResponseContext);
            return BuildResponseWithError(errorObject, errorType, response);
        }
        
        public async Task<IResponse> GetHttpResponseWithDataAndErrorAsync(IRequest request, Type dataType, Type errorType, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy = null, CancellationToken cancellationToken = default)
        {
            var transportResponseContext = await (resiliencePolicy ?? _resiliencePolicy)
                .ExecuteAsync(token => ExecuteAttemptAsync(request, token), cancellationToken)
                .ConfigureAwait(false);
            
            var response = await _responseBuilder
                .BuildAsync(request, transportResponseContext, cancellationToken)
                .ConfigureAwait(false);
            
            var dataObject = TryGetDataObject(dataType, response.Content.StreamContent, transportResponseContext);
            var errorObject = TryGetErrorObject(errorType, response.Content.StreamContent, transportResponseContext);
            return BuildResponseWithDataAndError(dataObject, dataType, errorObject, errorType, response);
        }

        private async Task<IResponseContext<TRequest, TResponse>> ExecuteAsync(IRequest transportRequest, 
            IResiliencePolicy<TRequest, TResponse>? resiliencePolicy, CancellationToken cancellationToken = default)
        {
            return await (resiliencePolicy ?? _resiliencePolicy)
                .ExecuteAsync(token => ExecuteAttemptAsync(transportRequest, token), cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<IResponseContext<TRequest, TResponse>> ExecuteAttemptAsync(IRequest request, CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Start sending '{requestMethod}' request to '{requestUri}'. Request id: '{requestId}'.", request.Type, request.Endpoint, request.Id);

            TRequest? transportRequest;
            TResponse? transportResponse;
            try
            {
                _logger?.LogDebug("Start sending request attempt. Request id: '{requestId}'.", request.Id);
                transportRequest = await _transportRequestBuilder
                    .BuildAsync(request, cancellationToken)
                    .ConfigureAwait(false);
                
                await _clientHandler
                    .HandleRequestAsync(transportRequest, cancellationToken)
                    .ConfigureAwait(false);
                
                transportResponse = await _transport.ExecuteAsync(transportRequest, cancellationToken).ConfigureAwait(false);

                transportResponse = await _clientHandler
                    .HandleResponseAsync(transportResponse, cancellationToken)
                    .ConfigureAwait(false);
                
                _logger?.LogDebug("Request attempt finished. Request id: '{requestId}'.", request.Id);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "Request attempt failed with exception. Request id: '{requestId}'.", request.Id);
                throw;
            }
            
            _logger?.LogDebug("Response received. Request id: '{requestId}'.", request.Id);
            return new ResponseContext<TRequest, TResponse>(transportRequest, transportResponse);
        }
        
        private object? TryGetDataObject(Type dataType, Stream data, IResponseContext<TRequest, TResponse> transportResponseContext)
        {
            return _responseValidator.IsSuccess(transportResponseContext)
                ? _serializer.Deserialize(new StreamReader(data).ReadToEnd(), dataType)
                : null;
        }

        private object? TryGetErrorObject(Type errorType, Stream data, IResponseContext<TRequest, TResponse> transportResponseContext)
        {
            return !_responseValidator.IsSuccess(transportResponseContext)
                ? _serializer.Deserialize(new StreamReader(data).ReadToEnd(), errorType)
                : null;
        }
        
        private static IResponse BuildResponseWithData(object? data, Type dataType, IResponse response)
        {
            var genericResponseType = typeof(Response<>).MakeGenericType(dataType);
            return (IResponse) Activator.CreateInstance(genericResponseType, response, response.Request, data);
        }
        
        private static IResponse BuildResponseWithError(object? error, Type errorType, IResponse response)
        {
            var genericResponseType = typeof(ResponseWithError<>).MakeGenericType(errorType);
            return (IResponse) Activator.CreateInstance(genericResponseType, response, response.Request, error);
        }
        
        private static IResponse BuildResponseWithDataAndError(object? data, Type dataType, object? error, Type errorType, IResponse response)
        {
            var genericResponseType = typeof(ResponseWithError<,>).MakeGenericType(dataType, errorType);
            return (IResponse) Activator.CreateInstance(genericResponseType, response, response.Request, data, error);
        }
    }
}
