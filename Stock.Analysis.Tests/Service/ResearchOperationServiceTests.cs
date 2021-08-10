using System;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis.Tests.MockData;

namespace Stock.Analysis.Tests.Service
{
    public class ResearchOperationServiceTests
    {
        private readonly IResearchOperationService _researchOperationService = new ResearchOperationService();
        private readonly IMovingAvarageService _movingAvarageService = new MovingAvarageService();
        private readonly IRepository _historyRepository = new HistoryRepository();
        DateTime _dateTime;
        ChartData _historicalData;
        List<StockModel> _stockList = new List<StockModel>();

        public ResearchOperationServiceTests()
        {
            _historicalData = _historyRepository.GetAscHistoryData();
            _stockList = _historyRepository.GetAscStockList();
        }

        [Fact]
        public void GetMyTransactionsTest0()
        {
            var transactionsList = _researchOperationService.GetMyTransactions(100, _historicalData, _stockList, 5, 20);
            Assert.Single(transactionsList);
            Assert.All(transactionsList, trans=> {
                Assert.Equal(TransactionType.AddFunds, trans.TransType);
            });
        }

        [Fact]
        public void GetMyTransactionsTest1()
        {
            var historicalData = _historyRepository.GetConcussiveHistoryData();
            var stockList = _historyRepository.GetConcussiveStockList();
            var transactionsList = _researchOperationService.GetMyTransactions(18000, historicalData, stockList, 5, 20);
            Assert.Equal(3,transactionsList.Count);
            Assert.Equal(18, transactionsList.Find(t=>t.TransType == TransactionType.Buy).TransPrice);
            Assert.Equal(1000, transactionsList.Find(t => t.TransType == TransactionType.Buy).TransVolume);
            Assert.Equal(92, transactionsList.Find(t => t.TransType == TransactionType.Sell).TransPrice);
            Assert.Equal(1000, transactionsList.Find(t => t.TransType == TransactionType.Sell).TransVolume);
        }

        [Theory]
        [InlineData(18,0)]
        [InlineData(18000,1000)]
        [InlineData(37000,2000)]
        [InlineData(60000,3000)]
        public void GetMyTransactionsTestVolumn(double funds, int volumns)
        {
            var historicalData = new ChartData
            {
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

            var transactionsList = _researchOperationService.GetMyTransactions(funds, historicalData, stockList, 5, 20);
            Assert.Equal(3, transactionsList.Count);
            Assert.Equal(volumns, transactionsList.Find(t => t.TransType == TransactionType.Buy).TransVolume);
            Assert.Equal(volumns, transactionsList.Find(t => t.TransType == TransactionType.Sell).TransVolume);
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
            var periodEnd = new DateTime(2021, 7, 1, 0, 0, 0);
            var resultMsg = _researchOperationService.Settlement(140, myTrans, testCase, Utils.ConvertToUnixTimestamp(periodEnd));
            Assert.Contains("When ma = 5 vs 20,\tEarned: 20", resultMsg);
        }

        [Theory]
        [InlineData(1, 1, 160, "40")]
        [InlineData(1, 2, 160, "60")]
        public void SettlementHasQtyTest(int firstVolumn, int secondVolumn,double currentStock, string expectedEarned)
        {
            var myTrans = new List<StockTransaction> {
                new StockTransaction
                {
                    TransPrice = 100,
                    TransType = TransactionType.Buy,
                    TransVolume = firstVolumn
                },
                new StockTransaction
                {
                    TransPrice = 120,
                    TransType = TransactionType.Sell,
                    TransVolume = firstVolumn
                },
                new StockTransaction
                {
                    TransPrice = 140,
                    TransType = TransactionType.Buy,
                    TransVolume = secondVolumn
                }
            };
            var testCase = new TestCase { Funds = 140, ShortTermMa = 5, LongTermMa = 20 };
            var periodEnd = new DateTime(2021, 7, 1, 0, 0, 0);
            var resultMsg = _researchOperationService.Settlement(currentStock, myTrans, testCase, Utils.ConvertToUnixTimestamp(periodEnd));
            Assert.Contains($"When ma = 5 vs 20,\tEarned: {expectedEarned}", resultMsg);
        }

        [Fact]
        public void TestBalance()
        {
            var testCase = new TestCase
            {
                Funds = 100000,
                ShortTermMa = 5,
                LongTermMa = 20
            };
            var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
            var dataList = _historyRepository.GetRealData1yOf2603();
            var chartData = new ChartData
            {
                Name = "2603.TW",
                Price = new List<double?>()
            };

            chartData.Price = dataList.Select(s => s.Price).ToList();
            chartData.Timestamp = dataList.Select(s => s.Date).ToList();
            chartData.PriceAvg5Days = _movingAvarageService.CalculateMovingAvarage(dataList, 5).Select(s => s.Price).ToList();
            chartData.PriceAvg20Days = _movingAvarageService.CalculateMovingAvarage(dataList, 20).Select(s => s.Price).ToList();

            var result = _researchOperationService.GetMyTransactions(testCase.Funds, chartData, dataList, testCase.ShortTermMa, testCase.LongTermMa);
            Assert.Equal(14, result.Count);
            var expectedBuyTime = new List<string>{ "2020-4-10", "2020-7-7", "2020-7-21", "2020-8-3", "2020-10-8", "2021-2-17", "2021-5-25" };
            var index = 0;
            Assert.All(result.FindAll(t=>t.TransType == TransactionType.Buy), trans=> {
                Assert.Equal(expectedBuyTime.ElementAt(index), trans.TransTimeString);
                index++;
            });
            var expectedSellTime = new List<string> { "2020-6-15", "2020-7-14", "2020-7-23", "2020-9-17", "2021-1-18", "2021-5-18" };
            index = 0;
            Assert.All(result.FindAll(t => t.TransType == TransactionType.Sell), trans => {
                Assert.Equal(expectedSellTime.ElementAt(index), trans.TransTimeString);
                index++;
            });
            
            var expectedBalance = new List<double> { 100000, 8200, 104950, 3700, 100000, 1450, 99100, 10700, 139500, 6300, 278300, 21400, 507200, 12800  };
            var expectedVolume = new List<int> { 0, 9000, 9000, 9000, 9000, 9000, 9000, 8000, 8000, 8000, 8000, 7000, 7000, 6000 };
            index = 0;
            Assert.All(result, trans => {
                Assert.Equal(expectedBalance.ElementAt(index), trans.Balance);
                Assert.Equal(expectedVolume.ElementAt(index), trans.TransVolume);
                index++;
            });

            var settlement = _researchOperationService.Settlement(197, result, testCase, Utils.ConvertToUnixTimestamp(periodEnd));
            Assert.Contains("1094800", settlement);
            var lastTrans = result.LastOrDefault();
            Assert.Equal("2021-6-30", lastTrans.TransTimeString);
            Assert.Equal(1194800, lastTrans.Balance);
            Assert.Equal(6000, lastTrans.TransVolume);
            Assert.Equal(197, lastTrans.TransPrice);
        }
    }
}
