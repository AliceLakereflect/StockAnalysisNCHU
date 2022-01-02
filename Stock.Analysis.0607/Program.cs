using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace Stock.Analysis._0607
{
    class Program
    {

        private static IResearchOperationService _researchOperationService = new ResearchOperationService();
        private static IAlgorithmService _qtsAlgorithmService = new QTSAlgorithmService();
        private static IFileHandler _fileHandler = new FileHandler();
        private static IDataService _dataService = new DataService();
        const string SYMBOL = "2603.TW";
        const double FUNDS = 10000000;
        const int EXPERIMENT_NUMBER = 50;

        static void Main(string[] args)
        {
            var stockSymbol = new List<string> { "2603.TW"
                //, "2609.TW", "2615.TW", "0050.TW", "MU"
            };
            var chartDataList = new List<ChartData>();

            // stock parameters
            var periodStart = new DateTime(2020, 1, 1, 0, 0, 0);
            var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
            var stockList = _dataService.GetPeriodDataFromYahooApi(SYMBOL, periodStart.AddDays(-2), periodEnd.AddDays(1));
            var maStockList = _dataService.GetPeriodDataFromYahooApi(SYMBOL, new DateTime(2000, 1, 1, 0, 0, 0), periodEnd.AddDays(1));
            ChartData data = _researchOperationService.GetMaFromYahoo(SYMBOL, maStockList, Utils.ConvertToUnixTimestamp(periodStart.AddDays(-2)), Utils.ConvertToUnixTimestamp(periodEnd.AddDays(1)));
            chartDataList.Add(data);

            var myTransList = new List<StockTransList>();
            StatusValue bestGbest = new StatusValue();
            //var random = new Random(343);
            var random = _fileHandler.Readcsv("Data/srand343");
            for (var e = 0; e < EXPERIMENT_NUMBER; e++)
            {
                var path = Path.Combine(Environment.CurrentDirectory, $"Output/50 Experements/debug G best transaction exp: {e} - C random.csv");
                StatusValue gBest = new StatusValue();
                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    gBest = _qtsAlgorithmService.Fit(random, FUNDS, stockList, data, e, csv, periodStart);
                }
                Console.WriteLine($"{e}: {gBest.Fitness}");
                if(bestGbest.Fitness < gBest.Fitness)
                {
                    bestGbest = gBest.DeepClone();
                }

                var gBestTestCase = new TestCase
                {
                    Funds = FUNDS,
                    BuyShortTermMa = _qtsAlgorithmService.GetMaNumber(gBest.BuyMa1),
                    BuyLongTermMa = _qtsAlgorithmService.GetMaNumber(gBest.BuyMa2),
                    SellShortTermMa = _qtsAlgorithmService.GetMaNumber(gBest.SellMa1),
                    SellLongTermMa = _qtsAlgorithmService.GetMaNumber(gBest.SellMa2)
                };
                var transactions = _researchOperationService.GetMyTransactions(data, stockList, gBestTestCase, periodStart);
                myTransList.Add(new StockTransList { Name = SYMBOL, TestCase = gBestTestCase, Transactions = transactions });

                // Output csv file
                var algorithmConst = _qtsAlgorithmService.GetConst();
                algorithmConst.EXPERIMENT_NUMBER = e;
                _fileHandler.OutputQTSResult(algorithmConst, FUNDS, gBest, transactions, $"50 Experements/G best transaction {e} - 2020-1-2 ~ 2021-6-30");
            }

            if(bestGbest.BuyMa1.Count > 0)
            {
                var testCase = new TestCase
                {
                    Funds = FUNDS,
                    BuyShortTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.BuyMa1),
                    BuyLongTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.BuyMa2),
                    SellShortTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.SellMa1),
                    SellLongTermMa = _qtsAlgorithmService.GetMaNumber(bestGbest.SellMa2)
                };
                var t = _researchOperationService.GetMyTransactions(data, stockList, testCase, periodStart);
                var a = _qtsAlgorithmService.GetConst();
                a.EXPERIMENT_NUMBER = EXPERIMENT_NUMBER;
                _fileHandler.OutputQTSResult(a, FUNDS, bestGbest, t, $"G best transaction - C random - 2020-1-2 ~ 2021-6-30");
            }
        }

        

        #region private method

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
