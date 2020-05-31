using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tgibbsstoragecrud
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> Get();
        Task<IEnumerable<T>> Get(Predicate<T> func);
        Task<T> GetById(string id);
        Task<T> InsertOrReplace(T entity);
        Task InsertOrReplace(IEnumerable<T> entities);
        Task Delete(string id);
        Task Delete(Predicate<T> func);
        IRepository<T> OverrideTableName(string tableName);

    }
}