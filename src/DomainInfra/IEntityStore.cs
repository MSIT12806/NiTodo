using System;
using System.Collections.Generic;

namespace DomainInfra
{
    public interface IEntityStore<TEntity> where TEntity : IEntity
    {
        void Add(TEntity entity);
        void Remove(string id);
        void Save(TEntity entity);
        TEntity Get(string id);
        IEnumerable<TEntity> Get(Func<TEntity, bool> predicate);
    }
}
