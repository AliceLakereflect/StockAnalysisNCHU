using System.Collections.Generic;
using System.Linq;

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
        public Dictionary<int, List<double?>> MaList = new Dictionary<int, List<double?>>();

        public ChartData()
        {
        }
    }
}