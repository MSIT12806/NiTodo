using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainInfra.DefaultImplement
{
    public class DefaultEntityStore<TEntity> : IEntityStore<TEntity> where TEntity : IEntity
    {
        private static readonly Dictionary<string, TEntity> _entities = new Dictionary<string, TEntity>();
        public void Add(TEntity entity)
        {
            _entities.Add(entity.Id, entity);
        }
        public void Remove(string id)
        {
            _entities.Remove(id);
        }
        public void Save(TEntity entity)
        {
            _entities[entity.Id] = entity;
        }
        public TEntity Get(string id)
        {
            if (!_entities.ContainsKey(id))
            {
                return default;
            }

            return _entities[id];
        }
        public IEnumerable<TEntity> Get(Func<TEntity, bool> predicate)
        {
            //TODO: 應該是要取出物件的複製品，而不是原物件，但是Json序列化會有問題
            return _entities.Values.Where(predicate);
        }
    }
}
