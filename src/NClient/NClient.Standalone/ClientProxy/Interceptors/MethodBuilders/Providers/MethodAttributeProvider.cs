﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NClient.Annotations;
using NClient.Annotations.Operations;
using NClient.Core.Mappers;
using NClient.Standalone.Exceptions.Factories;

namespace NClient.Standalone.ClientProxy.Interceptors.MethodBuilders.Providers
{
    internal interface IMethodAttributeProvider
    {
        OperationAttribute Get(MethodInfo method, IEnumerable<MethodInfo> overridingMethods);
    }

    internal class MethodAttributeProvider : IMethodAttributeProvider
    {
        private readonly IAttributeMapper _attributeMapper;
        private readonly IClientValidationExceptionFactory _clientValidationExceptionFactory;

        public MethodAttributeProvider(
            IAttributeMapper attributeMapper,
            IClientValidationExceptionFactory clientValidationExceptionFactory)
        {
            _attributeMapper = attributeMapper;
            _clientValidationExceptionFactory = clientValidationExceptionFactory;
        }

        public OperationAttribute Get(MethodInfo method, IEnumerable<MethodInfo> overridingMethods)
        {
            return Find(method) ?? overridingMethods.Select(Find).FirstOrDefault()
                ?? throw _clientValidationExceptionFactory.MethodAttributeNotFound(nameof(OperationAttribute));
        }

        private OperationAttribute? Find(MethodInfo method)
        {
            var attributes = method
                .GetCustomAttributes()
                .Select(x => _attributeMapper.TryMap(x))
                .ToArray();
            if (attributes.Any(x => x is PathAttribute))
                throw _clientValidationExceptionFactory.MethodAttributeNotSupported(nameof(PathAttribute));

            var methodAttributes = method
                .GetCustomAttributes()
                .Select(x => _attributeMapper.TryMap(x))
                .Where(x => x is OperationAttribute)
                .Cast<OperationAttribute>()
                .ToArray();
            if (methodAttributes.Length > 1)
                throw _clientValidationExceptionFactory.MultipleMethodAttributeNotSupported();

            return methodAttributes.SingleOrDefault();
        }
    }
}
