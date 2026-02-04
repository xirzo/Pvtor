using System;
using System.Collections.Generic;

namespace Pvtor.Application.Contracts.Notes.Models;

public sealed record NamespaceDtoQuery(long[] NamespaceIds)
{
    public static NamespaceDtoQuery Build(Func<Builder, Builder> action)
    {
        return action(new Builder()).Build();
    }

    public sealed class Builder
    {
        private readonly List<long> _namespaceIds = [];

        public Builder WithNamespaceId(long id)
        {
            _namespaceIds.Add(id);
            return this;
        }

        public Builder WithNamespaceIds(long[] ids)
        {
            _namespaceIds.AddRange(ids);
            return this;
        }

        public NamespaceDtoQuery Build()
        {
            return new NamespaceDtoQuery(_namespaceIds.ToArray());
        }
    }
}