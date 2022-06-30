﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AQS.OrderProject.API.Configuration
{
    internal class CorrelationMiddleware
    {
        internal const string CorrelationHeaderKey = "CorrelationId";

        private readonly RequestDelegate _next;

        public CorrelationMiddleware(
            RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = Guid.NewGuid();

            context.Request.Headers.Add(CorrelationHeaderKey, correlationId.ToString());

            await this._next.Invoke(context);
        }
    }
}