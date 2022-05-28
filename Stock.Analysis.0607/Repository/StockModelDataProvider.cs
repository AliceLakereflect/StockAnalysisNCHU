using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Interface;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Repository
{
    public class StockModelDataProvider : IDataProvider<StockModel>
    {
        private readonly StockModelDbContext _context;
        public StockModelDataProvider(StockModelDbContext context)
        {
            _context = context;
        }

        public void Add(StockModel stockModel)
        {
            _context.StockModel.Add(stockModel);
            _context.SaveChanges();
        }

        public void AddBatch(List<StockModel> entities)
        {
            _context.StockModel.AddRange(entities);
            _context.SaveChanges();
        }

        public void Delete(StockModel stockModel)
        {
            var entity = _context.StockModel.Find(stockModel);
            _context.StockModel.RemoveRange(entity);
            _context.SaveChanges();
        }

        public List<StockModel> GetAll(string stockName)
        {
            return _context.StockModel.ToList().FindAll(t => t.StockName == stockName);
        }

        public void Update(StockModel stockModel)
        {
            _context.StockModel.Update(stockModel);
            _context.SaveChanges();
        }
    }
}
