using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Diagnostics;

namespace Stock.Analysis._0607
{
    class Program
    {

        private static IResearchOperationService _researchOperationService = new ResearchOperationService();
        private static IGNQTSAlgorithmService _qtsAlgorithmService = new GNQTSAlgorithmService();
        private static IFileHandler _fileHandler = new FileHandler();
        private static IDataService _dataService = new DataService();
        private static ISlidingWindowService _slidingWindowService = new SlidingWindowService();
        const string SYMBOL = "AAPL";
        const double FUNDS = 10000000;
        const int EXPERIMENT_NUMBER = 50;

        static void Main(string[] args)
        {
            //var stockSymbol = new List<string> { "2603.TW"
            //    //, "2609.TW", "2615.TW", "0050.TW", "MU"
            //};
            var chartDataList = new List<ChartData>();
            var myTransList = new List<StockTransList>();

            // stock parameters
            var allStart = new DateTime(2014, 12, 1, 0, 0, 0);
            var allEnd = new DateTime(2014, 12, 31, 0, 0, 0);
            var period = new Period { Start = allStart, End = allEnd };
            _qtsAlgorithmService.SetDelta(0.00016);
            var random = new Random(343);
            //var cRandom = new Queue<int>();
            var cRandom = _fileHandler.Readcsv("Data/srand343");
            var maStockList = _dataService.GetPeriodDataFromYahooApi(SYMBOL, new DateTime(2000, 1, 1, 0, 0, 0), allEnd.AddDays(1));

            var slidingWindows = _slidingWindowService.GetSlidingWindows(period, PeriodEnum.M, PeriodEnum.M);


            slidingWindows.ForEach((window)=>
            {

                var periodStart = window.TrainPeriod.Start;
                var periodEnd = window.TrainPeriod.End;
                var copyCRandom = new Queue<int>(cRandom);
                StatusValue bestGbest = Train(chartDataList, myTransList, copyCRandom, random, maStockList, periodStart, periodEnd);
                Test(bestGbest, window.TestPeriod, maStockList);
            });
        }

        #region private method
        private static void Test(StatusValue bestGbest, Period testPeriod, List<StockModel> maStockList)
        {
            var stockList = _dataService.GetPeriodDataFromYahooApi(SYMBOL, testPeriod.Start.AddDays(-7), testPeriod.End.AddDays(1));
            ChartData data = _researchOperationService.GetMaFromYahoo(SYMBOL, maStockList, Utils.ConvertToUnixTimestamp(testPeriod.Start.AddDays(-7)), Utils.ConvertToUnixTimestamp(testPeriod.End.AddDays(1)));

            GetTransactionsAndOutput(testPeriod.Start, stockList, data, bestGbest, "Test");
        }

        private static StatusValue Train(
            List<ChartData> chartDataList,
            List<StockTransList> myTransList,
            Queue<int> copyCRandom,
            Random random,
            List<StockModel> maStockList,
            DateTime periodStart,
            DateTime periodEnd)
        {
            var stockList = _dataService.GetPeriodDataFromYahooApi(SYMBOL, periodStart.AddDays(-7), periodEnd.AddDays(1));
            ChartData data = _researchOperationService.GetMaFromYahoo(SYMBOL, maStockList, Utils.ConvertToUnixTimestamp(periodStart.AddDays(-7)), Utils.ConvertToUnixTimestamp(periodEnd.AddDays(1)));
            chartDataList.Add(data);

            StatusValue bestGbest = new StatusValue();
            int gBestCount = 0;

            //var random = new Queue<int>(cRandom);
            for (var e = 0; e < EXPERIMENT_NUMBER; e++)
            {
                Console.WriteLine("Begin:\n");
                Stopwatch swFit = new Stopwatch();
                var path = Path.Combine(Environment.CurrentDirectory, $"Output/50 Experements/debug G best transaction exp: {e} - C random.csv");
                StatusValue gBest = new StatusValue();
                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    swFit.Start();
                    gBest = _qtsAlgorithmService.Fit(copyCRandom, random, FUNDS, stockList, data, e, csv, periodStart);
                    swFit.Stop();
                }
                Console.WriteLine($"{e}: {gBest.Fitness} => {swFit.Elapsed.Minutes}:{swFit.Elapsed.Seconds}:{swFit.Elapsed.Milliseconds}");
                swFit.Reset();
                CompareGBestByBits(ref bestGbest, ref gBestCount, gBest);

                OutputEachExperiment(periodStart, stockList, data, myTransList, e, gBest);
            }

            if (bestGbest.BuyMa1.Count > 0)
            {
                GetTransactionsAndOutput(periodStart, stockList, data, bestGbest, "Train", gBestCount);
            }

            return bestGbest;
        }

        private static void GetTransactionsAndOutput(DateTime periodStart, List<StockModel> stockList, ChartData data, StatusValue bestGbest,string mode, int gBestCount = 0)
        {
            var testCase = new TestCase
            {
                Funds = FUNDS,
                BuyShortTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.BuyMa1),
                BuyLongTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.BuyMa2),
                SellShortTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.SellMa1),
                SellLongTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.SellMa2)
            };
            Stopwatch sw = new Stopwatch();
            var t = _researchOperationService.GetMyTransactions(data, stockList, testCase, periodStart, sw, sw);
            var a = _qtsAlgorithmService.GetConst();
            a.EXPERIMENT_NUMBER = EXPERIMENT_NUMBER;
            _fileHandler.OutputQTSResult(a, FUNDS, bestGbest, gBestCount, t,
                $"M2M/GNQTS {mode} - Crandom - {periodStart.Year}_{periodStart.Month}");
        }

        private static void CompareGBestByBits(ref StatusValue bestGbest, ref int gBestCount, StatusValue gBest)
        {
            if (bestGbest.Fitness < gBest.Fitness)
            {
                bestGbest = gBest.DeepClone();
                gBestCount = 0;
            }

            if (
                _qtsAlgorithmService.GetMaNumber(bestGbest.BuyMa1) == _qtsAlgorithmService.GetMaNumber(gBest.BuyMa1) &&
                _qtsAlgorithmService.GetMaNumber(bestGbest.BuyMa2) == _qtsAlgorithmService.GetMaNumber(gBest.BuyMa2) &&
                _qtsAlgorithmService.GetMaNumber(bestGbest.SellMa1) == _qtsAlgorithmService.GetMaNumber(gBest.SellMa1) &&
                _qtsAlgorithmService.GetMaNumber(bestGbest.SellMa2) == _qtsAlgorithmService.GetMaNumber(gBest.SellMa2) &&
                bestGbest.Fitness == gBest.Fitness
                ) gBestCount++;
        }

        private static void CompareGBestByFitness(ref StatusValue bestGbest, ref int gBestCount, StatusValue gBest)
        {
            if (bestGbest.Fitness < gBest.Fitness)
            {
                bestGbest = gBest.DeepClone();
                gBestCount = 0;
            }

            if (bestGbest.Fitness == gBest.Fitness) gBestCount++;
        }

        private static void OutputEachExperiment(DateTime periodStart, List<StockModel> stockList, ChartData data, List<StockTransList> myTransList, int e, StatusValue gBest)
        {
            var gBestTestCase = new TestCase
            {
                Funds = FUNDS,
                BuyShortTermMa = _qtsAlgorithmService.GetMaNumber(gBest.BuyMa1),
                BuyLongTermMa = _qtsAlgorithmService.GetMaNumber(gBest.BuyMa2),
                SellShortTermMa = _qtsAlgorithmService.GetMaNumber(gBest.SellMa1),
                SellLongTermMa = _qtsAlgorithmService.GetMaNumber(gBest.SellMa2)
            };
            Stopwatch sw = new Stopwatch();
            var transactions = _researchOperationService.GetMyTransactions(data, stockList, gBestTestCase, periodStart, sw, sw);
            myTransList.Add(new StockTransList { Name = SYMBOL, TestCase = gBestTestCase, Transactions = transactions });

            // Output csv file
            var algorithmConst = _qtsAlgorithmService.GetConst();
            algorithmConst.EXPERIMENT_NUMBER = e;
            _fileHandler.OutputQTSResult(algorithmConst, FUNDS, gBest, 0, transactions, $"50 Experements/G best transaction {e} - {periodStart.Year} - {periodStart.Month}");
        }

        private static void PrintTransactions(
            string symbol,
            TestCase currentTestCase,
            List<StockModel> stockList,
            List<StockTransaction> myTrans,
            TestCase ma,
            double? currentStock)
        {
            Console.WriteLine($"{symbol}\t Current Stock: {currentStock}");
            var earn = _researchOperationService.GetEarningsResults(myTrans);
            var resultString = $"When ma = Buy: {ma.BuyShortTermMa} vs {ma.BuyLongTermMa}; Sell: {ma.SellShortTermMa} vs {ma.SellLongTermMa};,\tEarned: {earn}\t";
            if (!resultString.Contains("Earned: 0"))
            {
                Console.WriteLine(resultString);
            }
            var transString = "=== My Transaction ======================================================================================\n";
            transString += $"|\tName: {symbol}\t Test Case: {currentTestCase.BuyShortTermMa}MA vs {currentTestCase.BuyLongTermMa}MA \t Current Stock:{stockList.Last().Price}\n";
            myTrans.ForEach(t =>
            {
                transString += $"|\t TransType: {t.TransType}\n|\t TransTime: {t.TransTimeString}\t TransVolume:{t.TransVolume}\t Fees:{t.Fees}\t " +
                $"Tax: {t.Tax}\t Balance: {t.Balance}\t TransPrice: {t.TransPrice}\n";
            });
            transString += "=========================================================================================================\n";
            Console.WriteLine(transString);
        }

        #endregion
    }
}
