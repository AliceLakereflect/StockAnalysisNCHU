
using System.Collections.Generic;

class StockTransaction
{
    public long TransTime { get; set; }
    public double TransPrice { get; set; }
    public int TransVolume { get; set; }
    public int Fees { get; set; } = 0;
    public int Tax { get; set; } = 0;
}

class StockTransList
{
    public string Name { get; set; }
    public TestCase TestCase { get; set; }
    public List<StockTransaction> Transactions { get; set; }
}