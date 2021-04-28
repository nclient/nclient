﻿using System;

namespace NClient.AspNetCore.Controllers.Models
{
    public class VirtualControllerInfo
    {
        public Type Type { get; }
        public Type ControllerType { get; }

        public VirtualControllerInfo(Type type, Type controllerType)
        {
            Type = type;
            ControllerType = controllerType;
        }
    }
}