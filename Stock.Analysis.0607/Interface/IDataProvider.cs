using System;
using System.Collections.Generic;

namespace Stock.Analysis._0607.Interface
{
    public interface IDataProvider<T>
    {
        void Add(T entity);
        void AddBatch(List<T> entities);
        void Update(T entity);
        void Delete(T entity);
        List<T> GetAll(string stockName);
        
    }
}
