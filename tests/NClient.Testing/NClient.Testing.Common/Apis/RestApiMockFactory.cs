﻿using System;
using NClient.Testing.Common.Entities;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace NClient.Testing.Common.Apis
{
    public class RestApiMockFactory
    {
        public Uri ApiUri { get; }

        public RestApiMockFactory(int port)
        {
            ApiUri = new UriBuilder("http", "localhost", port).Uri;
        }

        public IWireMockServer MockIntGetMethod(int id)
        {
            var api = WireMockServer.Start(ApiUri.ToString());
            api.Given(Request.Create()
                    .WithPath($"/api/rest/{id}")
                    .WithHeader("Accept", "application/json")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(id));

            return api;
        }
        
        public IWireMockServer MockNotFoundIntGetMethod(int id)
        {
            var api = WireMockServer.Start(ApiUri.ToString());
            api.Given(Request.Create()
                    .WithPath($"/api/rest/{id}")
                    .WithHeader("Accept", "application/json")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(404)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson("Error message"));

            return api;
        }

        public IWireMockServer MockStringGetMethod(string id)
        {
            var api = WireMockServer.Start(ApiUri.ToString());
            api.Given(Request.Create()
                    .WithPath($"/api/rest/{id}")
                    .WithHeader("Accept", "application/json")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(id));

            return api;
        }
        
        public IWireMockServer MockNotFoundStringGetMethod(string id)
        {
            var api = WireMockServer.Start(ApiUri.ToString());
            api.Given(Request.Create()
                    .WithPath($"/api/rest/{id}")
                    .WithHeader("Accept", "application/json")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(404)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson("Error message"));

            return api;
        }

        public IWireMockServer MockPostMethod(BasicEntity entity)
        {
            var api = WireMockServer.Start(ApiUri.ToString());
            api.Given(Request.Create()
                    .WithPath("/api/rest")
                    .WithHeader("Accept", "application/json")
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(new JsonMatcher(entity))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            return api;
        }

        public IWireMockServer MockPutMethod(BasicEntity entity)
        {
            var api = WireMockServer.Start(ApiUri.ToString());
            api.Given(Request.Create()
                    .WithPath("/api/rest")
                    .WithHeader("Accept", "application/json")
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(new JsonMatcher(entity))
                    .UsingPut())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            return api;
        }

        public IWireMockServer MockDeleteMethod(int id)
        {
            var api = WireMockServer.Start(ApiUri.ToString());
            api.Given(Request.Create()
                    .WithPath($"/api/rest/{id}")
                    .WithHeader("Accept", "application/json")
                    .UsingDelete())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            return api;
        }
    }
}
