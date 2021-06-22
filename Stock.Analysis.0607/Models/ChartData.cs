using System.Collections.Generic;

public class ChartData
{
    public string Name { get; set; }
    public List<string> Day { get; set; }
    public List<double> Price { get; set; }
    public List<double> PriceAvg5Days { get; set; }
    public List<double> PriceAvg10Days { get; set; }
    public List<double> PriceAvg20Days { get; set; }
    public List<double> PriceAvg60Days { get; set; }

    public ChartData()
    {
    }
}