using System;
using Xunit;
using Stock.Analysis._0607.Service;
using System.Collections.Generic;

namespace Stock.Analysis.Tests.Service
{
    public class TransTimingServiceTests
    {
        ITransTimingService _transTimingService = new TransTimingService();
        public TransTimingServiceTests()
        {
        }

        [Theory]
        [MemberData(nameof(CalculatorData.TimeToBuy), MemberType = typeof(CalculatorData))]
        public void TimeToBuyTest(bool expected, int index, List<double?> shortMaValList, List<double?> longMaValList, bool hasQty, bool missedBuying)
        {
            var buyOrNot = _transTimingService.TimeToBuy(index, shortMaValList, longMaValList, hasQty, missedBuying);
            Assert.Equal(expected, buyOrNot);
        }

        [Theory]
        [InlineData(10.2, 10.5, true, true)]
        [InlineData(10.2, 10.5, false, false)]
        [InlineData(10.8, 10.5, true, false)]
        [InlineData(10.8, 10.5, false, false)]
        [InlineData(null, 10.5, true, false)]
        [InlineData(10.8, null, true, false)]
        public void TimeToSellTest(double? shortMaVal, double? longMaVal, bool hasQty, bool expected)
        {
            var result = _transTimingService.TimeToSell(shortMaVal, longMaVal, hasQty);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(false, true, 11.2, 10.5, true)]
        [InlineData(false, true, 10.2, 10.8, false)]
        [InlineData(true, false, 10.2, 10.8, false)]
        [InlineData(true, false, 11.1, 10.8, true)]
        public void IfMissedBuyingTest(bool missedBuying, bool first, double? shortMaVal, double? longMaVal, bool expected)
        {
            _transTimingService.IfMissedBuying(ref missedBuying,ref first, shortMaVal, longMaVal);
            Assert.Equal(expected, missedBuying);
            Assert.False(first);
        }
    }
    public class CalculatorData
    {
        public static IEnumerable<object[]> TimeToBuy =>
        new List<object[]>
        {
            // condition1
            new object[] { true, 0, new List<double?> { 10 }, new List<double?> { 5 }, false, false },
            new object[] { false, 0, new List<double?> { 5 }, new List<double?> { 10 }, false, false},
            new object[] { false, 0, new List<double?> { 10}, new List<double?> { 5 }, true, false },
            new object[] { false, 0, new List<double?> { 10 }, new List<double?> { 5 }, false, true },
            // condition1 + condition2 
            new object[] { true, 0, new List<double?> { 10, 11 }, new List<double?> { 5, 6 }, false, false },
            new object[] { true, 1, new List<double?> { 10, 11 }, new List<double?> { 5, 6 }, false, false },
            new object[] { false, 1, new List<double?> { 10, 9 }, new List<double?> { 5, 6 }, false, false },
            new object[] { false, 1, new List<double?> { 10, 11 }, new List<double?> { 5, 4 }, false, false },
        };
    }
}
