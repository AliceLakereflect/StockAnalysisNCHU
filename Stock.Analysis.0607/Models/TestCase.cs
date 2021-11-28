namespace Stock.Analysis._0607.Models
{
    public class TestCase
    {
        public double Funds { get; set; }
        public int BuyShortTermMa { get; set; }
        public int BuyLongTermMa { get; set; }
        public int SellShortTermMa { get; set; }
        public int SellLongTermMa { get; set; }

        public TestCase DeepClone()
        {
            return new TestCase {
                Funds = Funds,
                BuyShortTermMa = BuyShortTermMa,
                BuyLongTermMa = BuyLongTermMa,
                SellShortTermMa = SellShortTermMa,
                SellLongTermMa = SellLongTermMa };
        }
    }
}