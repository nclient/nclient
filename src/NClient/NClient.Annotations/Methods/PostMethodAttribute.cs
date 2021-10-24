﻿using NClient.Annotations.Operations;

namespace NClient.Annotations.Methods
{
    /// <summary>
    /// Identifies an action that supports the HTTP POST method.
    /// </summary>
    public class PostMethodAttribute : CreateOperationAttribute, IHttpMethodAttribute
    {
        public string? Name { get; set; }
        public int Order { get; set; }
        public string? Template { get; set; }
        
        /// <summary>
        /// Creates a new <see cref="PostMethodAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template.</param>
        public PostMethodAttribute(string? template = null)
        {
            Template = template;
        }
    }
}
