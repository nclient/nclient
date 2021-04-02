﻿using NClient.Core.Exceptions;

namespace NClient.AspNetCore.Exceptions.Factories
{
    internal static class OuterAspNetExceptionFactory
    {
        public static NClientException ControllerCanHaveOnlyOneInterface(string controllerName) =>
            new($"Controller '{controllerName}' can have only one NClient interface.");

        public static NClientException ControllerInterfaceNotFound(string controllerName) =>
            new($"NClient interface for controller '{controllerName}' not found.");

        public static NClientException ControllersNotFound() =>
            new($"NClient controllers not found.");
    }
}
