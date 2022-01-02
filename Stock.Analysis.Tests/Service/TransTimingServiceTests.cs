using System;
using Xunit;
using Stock.Analysis._0607.Service;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Models;

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
        public void TimeToBuyTest(bool expected, List<double?> shortMaValList, List<double?> longMaValList, bool hasQty)
        {
            var buyOrNot = _transTimingService.TimeToBuy(shortMaValList, longMaValList, 1, hasQty);
            Assert.Equal(expected, buyOrNot);
        }

        [Theory]
        [MemberData(nameof(CalculatorData.TimeToBuyAllUp), MemberType = typeof(CalculatorData))]
        public void TimeToBuyAllUpTest(bool expected, int index, List<double?> shortMaValList, List<double?> longMaValList, bool hasQty,
            bool check)
        {
            var buyOrNot = _transTimingService.TimeToBuy(index, shortMaValList, longMaValList, hasQty, check);
            Assert.Equal(expected, buyOrNot);
        }

        [Theory] // 死亡交叉
        [InlineData(10.2, 10.5, true, true)]
        [InlineData(10.2, 10.5, false, false)]
        [InlineData(10.8, 10.5, true, false)]
        [InlineData(10.8, 10.5, false, false)]
        [InlineData(null, 10.5, true, false)]
        [InlineData(10.8, null, true, false)]
        public void TimeToSellTest(double? shortMaVal, double? longMaVal, bool hasQty, bool expected)
        {
            var result = _transTimingService.TimeToSell(new List<double?> { 1, shortMaVal }, new List<double?> { 0,longMaVal }, 1, hasQty);
            Assert.Equal(expected, result);
        }

        [Theory] //一般停損
        [InlineData(100, 100, 10, false, false)]
        [InlineData(100, 100, 10, true, false)]
        [InlineData(109, 100, 10, true, false)]
        [InlineData(110, 100, 10, true, true)]
        [InlineData(120, 100, 10, true, true)]
        [InlineData(90.2, 100, 10, true, false)]
        [InlineData(90, 100, 10, true, true)]
        [InlineData(88, 100, 10, true, true)]
        [InlineData(88, 100, 12.5, true, false)]
        [InlineData(87.5, 100, 12.5, true, true)]
        [InlineData(87.4, 100, 12.5, true, true)]
        public void TimeToSellStopLoss(double currentPrice, double buyPirce, double sellPct, bool hasQty, bool expect)
        {
            var result = _transTimingService.TimeToSell(currentPrice, buyPirce, sellPct, hasQty);
            Assert.Equal(expect, result);
        }

        [Theory] //移動停損
        [InlineData(1624368600.0, 100.0, 90.0, 1624368599.0, 10, false, false)]
        // 下跌
        [InlineData(1624368600.0, 100.0, 90.0, 1624368601.0, 10, false, false)]
        [InlineData(1624368600.0, 100.0, 92.0, 1624368601.0, 10, true, true)]
        [InlineData(1624368600.0, 100.0, 99.0, 1624368601.0, 10, true, true)]
        [InlineData(1624368600.0, 100.0, 99.1, 1624368601.0, 10, true, false)]
        [InlineData(1624368600.0, 100.0, 90.0, 1624368601.0, 10, true, true)]
        // 上漲
        [InlineData(1624368600.0, 120.0, 110.0, 1624368601.0, 10, false, false)]
        [InlineData(1624368600.0, 120.0, 110.0, 1624368601.0, 10, true, false)]
        [InlineData(1624368600.0, 120.0, 108.0, 1624368601.0, 10, true, true)]
        public void TimeToSellMovingStopTest(double lastTransTime, ref double maxPrice, double price,
            double currentTime, double sellPct,bool hasQty, bool expected)
        {
            var lastTrans = new StockTransaction {
                TransTime = lastTransTime,
                TransPrice = 100
            };
            var result = _transTimingService.TimeToSell(lastTrans, ref maxPrice, price, currentTime, sellPct, hasQty);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(true, 11.2, 10.5, false)]
        [InlineData(true, 10.2, 10.8, true)]
        [InlineData(false, 10.2, 10.8, true)]
        [InlineData(false, 11.1, 10.8, false)]
        public void TrueCheckGoldCrossTest(bool check, double? shortMaVal, double? longMaVal, bool expected)
        {
            check = _transTimingService.TrueCheckGoldCross(check, shortMaVal, longMaVal);
            Assert.Equal(expected, check);
        }
    }
    public class CalculatorData
    {
        public static IEnumerable<object[]> TimeToBuy =>
        new List<object[]>
        {
            // 黃金交叉
            new object[] { true, new List<double?> { 0,10 }, new List<double?> { 1,5 }, false },
            new object[] { false, new List<double?> { 0,5 }, new List<double?> { 1,10 }, false},
            new object[] { false, new List<double?> { 0,10}, new List<double?> { 1,5 }, true },
        };
        public static IEnumerable<object[]> TimeToBuyAllUp =>
        new List<object[]>
        {
            // 黃金交叉
            new object[] { true, 0, new List<double?> { 10 }, new List<double?> { 5 }, false, true },
            new object[] { false, 0, new List<double?> { 5 }, new List<double?> { 10 }, false, true},
            new object[] { false, 0, new List<double?> { 10}, new List<double?> { 5 }, true, true },
            new object[] { false, 0, new List<double?> { 10 }, new List<double?> { 5 }, false, false },
            // 黃金交叉 + 均線向上 
            new object[] { true, 0, new List<double?> { 10, 11 }, new List<double?> { 5, 6 }, false, true },
            new object[] { true, 1, new List<double?> { 10, 11 }, new List<double?> { 5, 6 }, false, true },
            new object[] { false, 1, new List<double?> { 10, 9 }, new List<double?> { 5, 6 }, false, true },
            new object[] { false, 1, new List<double?> { 10, 11 }, new List<double?> { 5, 4 }, false, true },
        };
    }
}
