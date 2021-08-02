
using System.Collections.Generic;

namespace Stock.Analysis._0607.Models
{
    public class StockTransaction
    {
        public double TransTime { get; set; }
        public string TransTimeString { get; set; }
        public double TransPrice { get; set; }
        public TransactionType TransType { get; set; }
        public int TransVolume { get; set; }
        public int Fees { get; set; } = 0;
        public int Tax { get; set; } = 0;
        public double Balance { get; set; } = 0;
    }

    public class StockTransList
    {
        public string Name { get; set; }
        public TestCase TestCase { get; set; }
        public List<StockTransaction> Transactions { get; set; }
    }

    public enum TransactionType
    {
        AddFunds,
        Buy,
        Sell
    }
}