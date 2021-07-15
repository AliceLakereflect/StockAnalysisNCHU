using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607
{
    class Program
    {

        private static IResearchOperationService _researchOperationService = new ResearchOperationService();
        private static IFileHandler _fileHandler = new FileHandler();
        private static IDataService _dataService = new DataService();
        static void Main(string[] args)
        {
            var stockSymbol = new List<string> { "2603.TW", "2609.TW", "2615.TW" };
            var chartDataList = new List<ChartData>();
            var myTransList = new List<StockTransList>();
            List<TestCase> testCase = new List<TestCase> {
                new TestCase { Funds = 100000, ShortTermMa = 5, LongTermMa = 20 },
                new TestCase { Funds = 100000, ShortTermMa = 5, LongTermMa = 60 },
                new TestCase { Funds = 100000, ShortTermMa = 20, LongTermMa = 60 },
                new TestCase { Funds = 100000, ShortTermMa = 6, LongTermMa = 18 },
                new TestCase { Funds = 100000, ShortTermMa = 6, LongTermMa = 50 },
                new TestCase { Funds = 100000, ShortTermMa = 18, LongTermMa = 50 },
            };
            stockSymbol.ForEach(symbol =>
            {
                var chartDataList = new List<ChartData>();
                var stockList = _dataService.GetPeriodDataFromYahooApi(symbol, new DateTime(2020,1,1,0,0,0), new DateTime(2021, 7, 1, 0, 0, 0));
                ChartData data = _researchOperationService.GetMaFromYahoo(symbol, stockList);
                var currentStock = stockList.Last().Price ?? 0;
                Console.WriteLine($"{symbol}\t Current Stock: {currentStock}");
                chartDataList.Add(data);
                testCase.ForEach(currentTestCase =>
                {
                    var myTrans = _researchOperationService.GetMyTransactions(currentTestCase.Funds, data, stockList, currentTestCase.ShortTermMa, currentTestCase.LongTermMa);
                    myTransList.Add(new StockTransList { Name = symbol, TestCase = currentTestCase, Transactions = myTrans });
                    string resultString = _researchOperationService.Settlement(currentStock, myTrans, currentTestCase);
                    var transString = "=== My Transaction ======================================================================================\n";
                    transString += $"|\tName: {symbol}\t Test Case: {currentTestCase.ShortTermMa}MA vs {currentTestCase.LongTermMa}MA \t Current Stock:{stockList.Last().Price}\n";
                    myTrans.ForEach(t=>
                    {
                        transString += $"|\tTransTime: {t.TransTime}\t TransVolume:{t.TransVolume}\t Fees:{t.Fees}\t Tax: {t.Tax}\t TransPrice: {t.TransPrice}\n";
                    });
                    transString +=    "=========================================================================================================\n";
                    //Console.WriteLine(transString);
                });
            });

            _fileHandler.OutputResult(chartDataList, "chartData");
            _fileHandler.OutputResult(myTransList, "myTransaction");
            
        }

        
    }
}
