using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Interface;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Repository
{
    public class TrainBestTransactionProvider : IDataProvider<TrainBestTransaction>
    {
        private readonly TrainBestTransactionDbContext _context;
        public TrainBestTransactionProvider(TrainBestTransactionDbContext context)
        {
            _context = context;
        }

        public void Add(TrainBestTransaction entity)
        {
            _context.TrainBestTransaction.Add(entity);
            _context.SaveChanges();
        }

        public void AddBatch(List<TrainBestTransaction> entities)
        {
            _context.TrainBestTransaction.AddRange(entities);
            _context.SaveChanges();
        }

        public void Delete(TrainBestTransaction stockModel)
        {
            var entity = _context.TrainBestTransaction.Find(stockModel);
            _context.TrainBestTransaction.RemoveRange(entity);
            _context.SaveChanges();
        }

        public List<TrainBestTransaction> GetAll(string stockName)
        {
            return _context.TrainBestTransaction.ToList().FindAll(t => t.StockName == stockName);
        }

        public void Update(TrainBestTransaction stockModel)
        {
            _context.TrainBestTransaction.Update(stockModel);
            _context.SaveChanges();
        }
    }
}
