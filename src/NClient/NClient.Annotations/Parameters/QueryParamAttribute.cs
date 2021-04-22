﻿using System;
using NClient.Annotations.Abstractions;

namespace NClient.Annotations.Parameters
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class QueryParamAttribute : ParamAttribute, INameProviderAttribute
    {
        public string? Name { get; set; }
    }
}
