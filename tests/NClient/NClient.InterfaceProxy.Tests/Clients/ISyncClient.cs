﻿using NClient.InterfaceProxy.Attributes;
using NClient.InterfaceProxy.Attributes.Methods;
using NClient.Testing.Common.Clients;
using NClient.Testing.Common.Entities;

namespace NClient.InterfaceProxy.Tests.Clients
{
    [Api("api/sync")]
    public interface ISyncClientWithMetadata : ISyncClient
    {
        [AsHttpGet]
        new int Get(int id);

        [AsHttpPost]
        new void Post(BasicEntity entity);

        [AsHttpPut]
        new void Put(BasicEntity entity);

        [AsHttpDelete]
        new void Delete(int id);
    }
}
