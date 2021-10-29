﻿using System;
using NClient.Providers.Transport;

namespace NClient.Providers.Api
{
    public interface IRequestBuilder
    {
        IRequest Build(Guid requestId, string resourceRoot, IMethodInvocation methodInvocation);
    }
}
