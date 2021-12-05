using System.Collections.Generic;

namespace Stock.Analysis._0607
{
    class StatusValue
    {
        public List<int> BuyMa1 { get; set; } = new List<int>();
        public List<int> BuyMa2 { get; set; } = new List<int>();
        public List<int> SellMa1 { get; set; } = new List<int>();
        public List<int> SellMa2 { get; set; } = new List<int>();
        public double Fitness { get; set; } = 0;
    }
}
