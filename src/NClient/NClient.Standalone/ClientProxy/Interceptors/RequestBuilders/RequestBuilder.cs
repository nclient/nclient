﻿using System;
using System.Collections.Generic;
using System.Linq;
using NClient.Annotations.Parameters;
using NClient.Core.Helpers;
using NClient.Core.Helpers.ObjectMemberManagers.MemberNameSelectors;
using NClient.Providers.Transport;
using NClient.Standalone.ClientProxy.Interceptors.MethodBuilders.Models;
using NClient.Standalone.ClientProxy.Interceptors.RequestBuilders.Models;
using NClient.Standalone.Exceptions.Factories;
using NClient.Standalone.Helpers.ObjectToKeyValueConverters;

namespace NClient.Standalone.ClientProxy.Interceptors.RequestBuilders
{
    internal interface IRequestBuilder
    {
        IRequest Build(Guid requestId, Uri host, Method method, IEnumerable<object> arguments);
    }

    internal class RequestBuilder : IRequestBuilder
    {
        private readonly IRouteTemplateProvider _routeTemplateProvider;
        private readonly IRouteProvider _routeProvider;
        private readonly ITransportMethodProvider _transportMethodProvider;
        private readonly IObjectToKeyValueConverter _objectToKeyValueConverter;
        private readonly IClientValidationExceptionFactory _clientValidationExceptionFactory;

        public RequestBuilder(
            IRouteTemplateProvider routeTemplateProvider,
            IRouteProvider routeProvider,
            ITransportMethodProvider transportMethodProvider,
            IObjectToKeyValueConverter objectToKeyValueConverter,
            IClientValidationExceptionFactory clientValidationExceptionFactory)
        {
            _routeTemplateProvider = routeTemplateProvider;
            _routeProvider = routeProvider;
            _transportMethodProvider = transportMethodProvider;
            _objectToKeyValueConverter = objectToKeyValueConverter;
            _clientValidationExceptionFactory = clientValidationExceptionFactory;
        }

        public IRequest Build(Guid requestId, Uri host, Method method, IEnumerable<object> arguments)
        {
            var httpMethod = _transportMethodProvider.Get(method.Operation);
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

            var uri = UriHelper.Combine(host, route);
            var request = new Request(requestId, uri, httpMethod);

            var headerAttributes = method.HeaderAttributes;
            foreach (var headerAttribute in headerAttributes)
            {
                request.AddHeader(headerAttribute.Name, headerAttribute.Value);
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
                request.AddHeader(headerParam.Name, headerParam.Value!.ToString());
            }

            var bodyParams = methodParameters
                .Where(x => x.Attribute is BodyParamAttribute && x.Value != null)
                .ToArray();
            if (bodyParams.Length > 1)
                throw _clientValidationExceptionFactory.MultipleBodyParametersNotSupported();
            request.Data = bodyParams.SingleOrDefault()?.Value;

            return request;
        }
    }
}
