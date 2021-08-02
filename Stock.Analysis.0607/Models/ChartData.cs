using System.Collections.Generic;

namespace Stock.Analysis._0607.Models
{
    public class ChartData
    {
        public string Name { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public List<string> Day { get; set; } = new List<string>();
        public List<double> Timestamp { get; set; } = new List<double>();
        public List<double?> Price { get; set; } = new List<double?>();
        public List<double?> PriceAvg5Days { get; set; } = new List<double?>();
        public List<double?> PriceAvg10Days { get; set; } = new List<double?>();
        public List<double?> PriceAvg20Days { get; set; } = new List<double?>();
        public List<double?> PriceAvg60Days { get; set; } = new List<double?>();

        public ChartData()
        {
        }

        public List<double?> GetMaValue(int avgDay)
        {
            switch (avgDay)
            {
                case 5:
                    return PriceAvg5Days;
                case 10:
                    return PriceAvg10Days;
                case 20:
                    return PriceAvg20Days;
                case 60:
                    return PriceAvg60Days;
                default:
                    return new List<double?>();
            }
        }
    }
}