using System;
using System.Collections.Generic;

namespace Pvtor.Application.Contracts.Notes.Models;

public sealed record NoteDtoQuery(long[] NoteIds, long[] NamespaceIds, bool OnlyNonHidden)
{
    public static NoteDtoQuery Build(Func<Builder, Builder> action)
    {
        return action(new Builder()).Build();
    }

    public sealed class Builder
    {
        private readonly List<long> _ids = [];
        private readonly List<long> _namespaceIds = [];
        private bool _onlyNonHidden = true;

        public Builder WithId(long id)
        {
            _ids.Add(id);
            return this;
        }

        public Builder WithIds(long[] ids)
        {
            _ids.AddRange(ids);
            return this;
        }

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

        public Builder OnlyNonHidden(bool value)
        {
            _onlyNonHidden = value;
            return this;
        }

        public NoteDtoQuery Build()
        {
            return new NoteDtoQuery(_ids.ToArray(), _namespaceIds.ToArray(), _onlyNonHidden);
        }
    }
}