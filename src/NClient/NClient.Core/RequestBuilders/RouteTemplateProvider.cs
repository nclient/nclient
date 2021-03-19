﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing.Template;
using NClient.Core.Attributes;
using NClient.Core.Attributes.Clients;
using NClient.Core.Attributes.Clients.Methods;
using NClient.Core.Exceptions.Factories;

namespace NClient.Core.RequestBuilders
{
    public interface IRouteTemplateProvider
    {
        RouteTemplate Get(Type clientType, MethodInfo method);
    }

    public class RouteTemplateProvider : IRouteTemplateProvider
    {
        private readonly IAttributeMapper _attributeMapper;

        public RouteTemplateProvider(IAttributeMapper attributeMapper)
        {
            _attributeMapper = attributeMapper;
        }

        public RouteTemplate Get(Type clientType, MethodInfo method)
        {
            var apiAttributes = clientType
                .GetCustomAttributes()
                .Select(x => _attributeMapper.TryMap(x))
                .Where(x => x is ApiAttribute)
                .ToArray();
            if (apiAttributes.Length > 1)
                throw OuterExceptionFactory.MultipleAttributeForClientNotSupported(clientType.Name, nameof(ApiAttribute));
            var apiAttribute = apiAttributes.SingleOrDefault()
                ?? throw OuterExceptionFactory.ClientAttributeNotFound(typeof(ApiAttribute), clientType);

            //TODO: Duplication here and in HttpMethodProvider
            var methodAttributes = method
                .GetCustomAttributes()
                .Select(x => _attributeMapper.TryMap(x))
                .Where(x => x is AsHttpMethodAttribute)
                .ToArray();
            if (methodAttributes.Length > 1)
                throw OuterExceptionFactory.MultipleMethodAttributeNotSupported(method);
            var methodAttribute = methodAttributes.SingleOrDefault()
                ?? throw OuterExceptionFactory.MethodAttributeNotFound(typeof(AsHttpMethodAttribute), method);

            var apiTemplate = apiAttribute.GetType().GetProperty("Template")!.GetValue(apiAttribute, null) as string ?? "";
            var methodTemplate = methodAttribute.GetType().GetProperty("Template")!.GetValue(methodAttribute, null) as string ?? "";
            var routeTemplateStr = UriCombine(apiTemplate, methodTemplate);

            return Parse(routeTemplateStr);
        }

        private static RouteTemplate Parse(string routeTemplateStr)
        {
            try
            {
                return TemplateParser.Parse(routeTemplateStr);
            }
            catch (ArgumentException e)
            {
                throw OuterExceptionFactory.TemplateParsingError(e);
            }
        }

        public static string UriCombine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }
    }
}
