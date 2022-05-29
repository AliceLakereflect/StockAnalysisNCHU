using System;
namespace Stock.Analysis._0607.Models
{
    public class TrainBestTransaction
    {
        public Guid Id { get; }
        public Guid TrainResultId { get; set;}
        public string StockName { get; set; }
        public TransactionType TransType { get; set; }
        public double Date { get; set; }
        public double Price { get; set; }
        public double? ShortMaPrice1DayBefore { get; set; }
        public double? LongMaPrice1DayBefore { get; set; }
        public double? ShortMaPrice { get; set; }
        public double? LongMaPrice { get; set; }
        public int SharesHelds { get; set; }
        public double RemainingCapital { get; set; }
        public double TotalAssets { get; set; }

        public TrainBestTransaction()
        {
        }
    }
}
