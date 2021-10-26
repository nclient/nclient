﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NClient.Annotations.Parameters;
using NClient.Core.Helpers;
using NClient.Core.Helpers.ObjectMemberManagers.MemberNameSelectors;
using NClient.Providers.Serialization;
using NClient.Providers.Transport;
using NClient.Standalone.ClientProxy.Interceptors.MethodBuilders.Models;
using NClient.Standalone.ClientProxy.Interceptors.RequestBuilders.Models;
using NClient.Standalone.Exceptions.Factories;
using NClient.Standalone.Helpers.ObjectToKeyValueConverters;

namespace NClient.Standalone.ClientProxy.Interceptors.RequestBuilders
{
    internal interface IRequestBuilder
    {
        IRequest Build(Guid requestId, string resourceRoot, Method method, IEnumerable<object> arguments);
    }

    internal class RequestBuilder : IRequestBuilder
    {
        private readonly ISerializer _serializer;
        private readonly IRouteTemplateProvider _routeTemplateProvider;
        private readonly IRouteProvider _routeProvider;
        private readonly ITransportMethodProvider _transportMethodProvider;
        private readonly IObjectToKeyValueConverter _objectToKeyValueConverter;
        private readonly IClientValidationExceptionFactory _clientValidationExceptionFactory;

        public RequestBuilder(
            ISerializer serializer,
            IRouteTemplateProvider routeTemplateProvider,
            IRouteProvider routeProvider,
            ITransportMethodProvider transportMethodProvider,
            IObjectToKeyValueConverter objectToKeyValueConverter,
            IClientValidationExceptionFactory clientValidationExceptionFactory)
        {
            _serializer = serializer;
            _routeTemplateProvider = routeTemplateProvider;
            _routeProvider = routeProvider;
            _transportMethodProvider = transportMethodProvider;
            _objectToKeyValueConverter = objectToKeyValueConverter;
            _clientValidationExceptionFactory = clientValidationExceptionFactory;
        }

        public IRequest Build(Guid requestId, string resourceRoot, Method method, IEnumerable<object> arguments)
        {
            var requestType = _transportMethodProvider.Get(method.Operation);
            var routeTemplate = _routeTemplateProvider.Get(method);
            var methodParameters = method.Params
                .Select((methodParam, index) => new MethodParameter(
                    methodParam.Name,
                    methodParam.Type,
                    arguments.ElementAtOrDefault(index),
                    methodParam.Attribute))
                .ToArray();
            var route = _routeProvider
                .Build(routeTemplate, method.ClientName, method.Name, methodParameters, method.UseVersionAttribute);

            var resource = PathHelper.Combine(resourceRoot, route);
            var request = new Request(requestId, resource, requestType);

            var headerAttributes = method.HeaderAttributes;
            foreach (var headerAttribute in headerAttributes)
            {
                request.AddMetadata(headerAttribute.Name, headerAttribute.Value);
            }

            var urlParams = methodParameters
                .Where(x => x.Attribute is QueryParamAttribute && x.Value != null);
            foreach (var uriParam in urlParams)
            {
                var keyValuePairs = _objectToKeyValueConverter
                    .Convert(uriParam.Value, uriParam.Name, new QueryMemberNameSelector());
                foreach (var propertyKeyValue in keyValuePairs)
                {
                    request.AddParameter(propertyKeyValue.Key, propertyKeyValue.Value ?? "");
                }
            }

            var headerParams = methodParameters
                .Where(x => x.Attribute is HeaderParamAttribute && x.Value != null);
            foreach (var headerParam in headerParams)
            {
                if (!headerParam.Type.IsPrimitive())
                    throw _clientValidationExceptionFactory.ComplexTypeInHeaderNotSupported(headerParam.Name);
                request.AddMetadata(headerParam.Name, headerParam.Value!.ToString());
            }
            request.AddMetadata("Accept", _serializer.ContentType);

            var bodyParams = methodParameters
                .Where(x => x.Attribute is BodyParamAttribute && x.Value != null)
                .ToArray();
            if (bodyParams.Length > 1)
                throw _clientValidationExceptionFactory.MultipleBodyParametersNotSupported();
            if (bodyParams.Length == 1)
            {
                var bodyJson = _serializer.Serialize(bodyParams.SingleOrDefault()?.Value);
                var bodyBytes = Encoding.UTF8.GetBytes(bodyJson);
                request.Content = new Content(bodyBytes, Encoding.UTF8.WebName, new MetadataContainer
                {
                    new Metadata("Content-Encoding", Encoding.UTF8.WebName),
                    new Metadata("Content-Type", _serializer.ContentType),
                    new Metadata("Content-Length", bodyBytes.Length.ToString())
                });
            }

            return request;
        }
    }
}
