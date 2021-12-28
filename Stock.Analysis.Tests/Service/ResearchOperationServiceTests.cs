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
        TestCase _testCase = new TestCase { Funds = 100000, BuyShortTermMa = 5, BuyLongTermMa = 20, SellShortTermMa = 5, SellLongTermMa = 20 };
        DateTime _periodStart = new DateTime(2020, 1, 1, 0, 0, 0);

        public ResearchOperationServiceTests()
        {
            _historicalData = _historyRepository.GetAscHistoryData();
            _stockList = _historyRepository.GetAscStockList();
        }

        [Fact]
        public void GetMyTransactionsTest0()
        {
            var testCase = _testCase.DeepClone();
            testCase.Funds = 100;
            
            var transactionsList = _researchOperationService.GetMyTransactions(_historicalData, _stockList, testCase, _periodStart);
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
            var testCase = _testCase.DeepClone();
            testCase.Funds = 18000;
            var transactionsList = _researchOperationService.GetMyTransactions(historicalData, stockList, testCase, _periodStart);
            Assert.Equal(3, transactionsList.Count);
            Assert.Equal(18, transactionsList.Find(t=>t.TransType == TransactionType.Buy).TransPrice);
            Assert.Equal(1000, transactionsList.Find(t => t.TransType == TransactionType.Buy).TransVolume);
            Assert.Equal(92, transactionsList.Find(t => t.TransType == TransactionType.Sell).TransPrice);
            Assert.Equal(1000, transactionsList.Find(t => t.TransType == TransactionType.Sell).TransVolume);
        }

        [Theory]
        [InlineData(18, 1)]
        [InlineData(37, 2)]
        [InlineData(60, 3)]
        public void GetMyTransactionsTestVolumn(double funds, int volumns)
        {
            var historicalData = new ChartData
            {
                Name = "AAPL",
                Price = new List<double?>()
            };
            var stockList = new List<StockModel>();
            var dayIndex = -2;
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_periodStart.AddDays(dayIndex).Ticks);
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
                var elapsedSpan = new TimeSpan(_periodStart.AddDays(dayIndex).Ticks);
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
                var elapsedSpan = new TimeSpan(_periodStart.AddDays(dayIndex).Ticks);
                stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                historicalData.Price.Add(i + 10);
                dayIndex++;
            }
            historicalData.Timestamp = stockList.Select(s => s.Date).ToList();
            historicalData.MaList.Add(5, _movingAvarageService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList());
            historicalData.MaList.Add(20, _movingAvarageService.CalculateMovingAvarage(stockList, 20).Select(s => s.Price).ToList());
            var testCase = _testCase.DeepClone();
            testCase.Funds = funds;
            var transactionsList = _researchOperationService.GetMyTransactions(historicalData, stockList, testCase, _periodStart);
            Assert.Equal(3, transactionsList.Count);
            Assert.Equal(volumns, transactionsList.Find(t => t.TransType == TransactionType.Buy).TransVolume);
            Assert.Equal(volumns, transactionsList.Find(t => t.TransType == TransactionType.Sell).TransVolume);
        }

        [Fact]
        public void GetMaFromYahooTest()
        {
            var symbol = "AAPL";
            var chartdata = _researchOperationService.GetMaFromYahoo(symbol, _stockList, 1.0, 1.0);
            Assert.Equal(symbol, chartdata.Name);

            var index = 0;
            var limit = 59;
            var firstExpected = 39.5;
            var ma60 = chartdata.MaList[60];
            ma60.ForEach(maValue =>
            {
                if (index < limit)
                {
                    Assert.Null(maValue);
                }
                else
                {
                    var expected = firstExpected - limit + Convert.ToDouble(index);
                    Assert.Equal(expected, maValue);
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
            var testCase = new TestCase { BuyShortTermMa = 5, BuyLongTermMa = 20, SellShortTermMa = 5, SellLongTermMa = 20 };
            var periodEnd = new DateTime(2021, 7, 1, 0, 0, 0);
            var dataList = _historyRepository.GetRealData1yOf2603();
            var transactions = _researchOperationService.ProfitSettlement(140, dataList, testCase, myTrans, Utils.ConvertToUnixTimestamp(periodEnd));
            var earn = _researchOperationService.GetEarningsResults(transactions);
            Assert.Equal(20, earn);
        }

        [Theory]
        [InlineData(1, 1, 160, 40)]
        [InlineData(1, 2, 160, 60)]
        public void SettlementHasQtyTest(int firstVolumn, int secondVolumn,double currentStock, double expectedEarned)
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
            var periodEnd = new DateTime(2021, 7, 1, 0, 0, 0);
            var dataList = _historyRepository.GetRealData1yOf2603();
            var transactions = _researchOperationService.ProfitSettlement(currentStock, dataList, _testCase, myTrans, Utils.ConvertToUnixTimestamp(periodEnd));
            var earn = _researchOperationService.GetEarningsResults(transactions);
            Assert.Equal(expectedEarned, earn);
        }

        [Fact]
        public void TestBalance()
        {
            var testCase = new TestCase
            {
                Funds = 100000,
                BuyShortTermMa = 5, BuyLongTermMa = 20, SellShortTermMa = 5, SellLongTermMa = 20
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
            chartData.MaList.Add(5, _movingAvarageService.CalculateMovingAvarage(dataList, 5).Select(s => s.Price).ToList());
            chartData.MaList.Add(20, _movingAvarageService.CalculateMovingAvarage(dataList, 20).Select(s => s.Price).ToList());

            var result = _researchOperationService.GetMyTransactions(chartData, dataList, testCase, _periodStart);
            Assert.Equal(15, result.Count);
            var expectedBuyTime = new List<string>{ "2020-4-10", "2020-7-7", "2020-7-22", "2020-8-3", "2020-10-8", "2021-2-17", "2021-5-25" };
            var index = 0;
            Assert.All(result.FindAll(t=>t.TransType == TransactionType.Buy), trans=> {
                Assert.Equal(expectedBuyTime.ElementAt(index), trans.TransTimeString);
                index++;
            });
            var expectedSellTime = new List<string> { "2020-6-16", "2020-7-14", "2020-7-23", "2020-9-17", "2021-1-18", "2021-5-18", "2021-6-30" };
            index = 0;
            Assert.All(result.FindAll(t => t.TransType == TransactionType.Sell), trans => {
                Assert.Equal(expectedSellTime.ElementAt(index), trans.TransTimeString);
                index++;
            });

            var expectedBalance = new List<double> { 100000, 10, 110293, 10, 104902, 1, 103944, 8, 151444, 13, 309243, 9, 584773, 63, 1397975 };
            var expectedVolume = new List<int> { 0, 9803, 9803, 9803, 9803, 9580, 9580, 9406, 9406, 9095, 9095, 8426, 8426, 7096, 7096 };
            index = 0;
            Assert.All(result, trans => {
                Assert.Equal(expectedBalance.ElementAt(index), trans.Balance);
                Assert.Equal(expectedVolume.ElementAt(index), trans.TransVolume);
                index++;
            });

            var transactions = _researchOperationService.ProfitSettlement(197, dataList, testCase, result, Utils.ConvertToUnixTimestamp(periodEnd));
            var earn = _researchOperationService.GetEarningsResults(transactions);
            Assert.Equal(1297975, earn);
            var lastTrans = result.LastOrDefault();
            Assert.Equal(197, lastTrans.TransPrice);
        }

        [Fact]
        public void DebugTransaction()
        {

            IDataService _dataService = new DataService();
            var testCase = new TestCase
            {
                Funds = 10000000,
                BuyShortTermMa = 187,
                BuyLongTermMa = 100,
                SellShortTermMa = 24,
                SellLongTermMa = 255
            };

            var periodStart = new DateTime(2020, 1, 1, 0, 0, 0);
            var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
            var dataList = _historyRepository.GetRealData1yOf2603();

            var maStockList = _dataService.GetPeriodDataFromYahooApi("2603.TW", new DateTime(2000, 1, 1, 0, 0, 0), periodEnd.AddDays(1));
            ChartData chartData = _researchOperationService.GetMaFromYahoo("2603.TW", maStockList, Utils.ConvertToUnixTimestamp(periodStart), Utils.ConvertToUnixTimestamp(periodEnd.AddDays(1)));
            var result = _researchOperationService.GetMyTransactions(chartData, dataList, testCase, periodStart);
        }
    }
}
