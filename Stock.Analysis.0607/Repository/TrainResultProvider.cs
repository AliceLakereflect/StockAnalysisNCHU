using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Interface;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Repository
{
    public class TrainResultProvider : IDataProvider<TrainResult>
    {
        private readonly TrainResultDbContext _context;
        public TrainResultProvider(TrainResultDbContext context)
        {
            _context = context;
        }

        public void Add(TrainResult entitiy)
        {
            _context.TrainResult.Add(entitiy);
            _context.SaveChanges();
        }

        public void AddBatch(List<TrainResult> entities)
        {
            _context.TrainResult.AddRange(entities);
            _context.SaveChanges();
        }

        public void Delete(TrainResult entitiy)
        {
            var entity = _context.TrainResult.Find(entitiy);
            _context.TrainResult.RemoveRange(entity);
            _context.SaveChanges();
        }

        public List<TrainResult> GetAll(string stockName)
        {
            return _context.TrainResult.ToList().FindAll(t => t.StockName == stockName);
        }

        public void Update(TrainResult entitiy)
        {
            _context.TrainResult.Update(entitiy);
            _context.SaveChanges();
        }
    }
}
