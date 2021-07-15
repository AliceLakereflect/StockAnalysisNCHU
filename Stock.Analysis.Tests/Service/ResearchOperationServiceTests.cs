using System;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Stock.Analysis.Tests.Service
{
    public class ResearchOperationServiceTests
    {
        private readonly IResearchOperationService _researchOperationService = new ResearchOperationService();
        private readonly IMovingAvarageService _movingAvarageService = new MovingAvarageService();
        DateTime _dateTime;
        ChartData _historicalData;
        List<StockModel> _stockList = new List<StockModel>();

        public ResearchOperationServiceTests()
        {
            DateTime.TryParse("2020-05-08", out _dateTime);
            _historicalData = new ChartData
            {
                Name = "AAPL",
                Timestamp = new List<double>(),
                Price = new List<double?>()
            };
            for (var i = 0; i < 180; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(i).Ticks);
                _stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                
                _historicalData.Price.Add(i + 10);
            }
            _historicalData.Timestamp = _stockList.Select(s => s.Date).ToList();
            _historicalData.PriceAvg5Days = _movingAvarageService.CalculateMovingAvarage(_stockList, 5).Select(s => s.Price).ToList();
            _historicalData.PriceAvg20Days = _movingAvarageService.CalculateMovingAvarage(_stockList, 20).Select(s => s.Price).ToList();
        }

        [Fact]
        public void GetMyTransactionsTest0()
        {
            var transactionsList = _researchOperationService.GetMyTransactions(100, _historicalData, _stockList, 5, 20);
            Assert.Empty(transactionsList);
        }

        [Fact]
        public void GetMyTransactionsTest1()
        {
            var historicalData = new ChartData {
                Name = "AAPL",
                Price = new List<double?>()
            };
            var stockList = new List<StockModel>();
            var dayIndex = 0;
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                historicalData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 0; i < 90; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                historicalData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                historicalData.Price.Add(i + 10);
                dayIndex++;
            }
            historicalData.Timestamp = stockList.Select(s => s.Date).ToList();
            historicalData.PriceAvg5Days = _movingAvarageService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList();
            historicalData.PriceAvg20Days = _movingAvarageService.CalculateMovingAvarage(stockList, 20).Select(s => s.Price).ToList();

            var transactionsList = _researchOperationService.GetMyTransactions(18, historicalData, stockList, 5, 20);
            Assert.Equal(2,transactionsList.Count);
            Assert.Equal(18, transactionsList.FirstOrDefault().TransPrice);
            Assert.Equal(1, transactionsList.FirstOrDefault().TransVolume);
            Assert.Equal(92, transactionsList.LastOrDefault().TransPrice);
            Assert.Equal(1, transactionsList.LastOrDefault().TransVolume);
        }

        [Fact]
        public void GetMaFromYahooTest()
        {
            var symbol = "AAPL";
            var chartdata = _researchOperationService.GetMaFromYahoo(symbol, _stockList);
            Assert.Equal(symbol, chartdata.Name);

            var index = 0;
            var limit = 59;
            var firstExpected = 39.5;
            chartdata.PriceAvg60Days.ForEach(stock =>
            {
                if (index < limit)
                {
                    Assert.Null(chartdata.PriceAvg60Days[index]);
                }
                else
                {
                    var expected = firstExpected - limit + Convert.ToDouble(index);
                    Assert.Equal(expected, chartdata.PriceAvg60Days[index]);
                }

                index++;
            });
        }

        [Fact]
        public void SettlementNoQtyTest()
        {
            var myTrans = new List<StockTransaction> {
                new StockTransaction
                {
                    TransPrice = 100,
                    TransType = TransactionType.Buy,
                    TransVolume = 1
                },
                new StockTransaction
                {
                    TransPrice = 120,
                    TransType = TransactionType.Sell,
                    TransVolume = 1
                }
            };
            var testCase = new TestCase { ShortTermMa = 5, LongTermMa = 20};
            var resultMsg = _researchOperationService.Settlement(140, myTrans, testCase);
            Assert.Contains("When ma = 5 vs 20,\tEarned: 20", resultMsg);
        }

        [Fact]
        public void SettlementHasQtyTest()
        {
            var myTrans = new List<StockTransaction> {
                new StockTransaction
                {
                    TransPrice = 100,
                    TransType = TransactionType.Buy,
                    TransVolume = 1
                },
                new StockTransaction
                {
                    TransPrice = 120,
                    TransType = TransactionType.Sell,
                    TransVolume = 1
                },
                new StockTransaction
                {
                    TransPrice = 140,
                    TransType = TransactionType.Buy,
                    TransVolume = 1
                }
            };
            var testCase = new TestCase { Funds = 140, ShortTermMa = 5, LongTermMa = 20 };
            var resultMsg = _researchOperationService.Settlement(160, myTrans, testCase);
            Assert.Contains("When ma = 5 vs 20,\tEarned: 40", resultMsg);
        }
    }
}
