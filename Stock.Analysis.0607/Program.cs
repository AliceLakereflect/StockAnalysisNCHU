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
            var stockSymbol = new List<string> { "2603.TW"
                //, "2609.TW", "2615.TW", "0050.TW", "MU"
            };
            var chartDataList = new List<ChartData>();
            var myTransList = new List<StockTransList>();
            List<TestCase> testCase = new List<TestCase>  {
                new TestCase { Funds = 100000, BuyShortTermMa = 5, BuyLongTermMa = 20, SellShortTermMa = 5, SellLongTermMa = 20  },
                //new TestCase { Funds = 100000, BuyShortTermMa = 5, BuyLongTermMa = 60, SellShortTermMa = 5, SellLongTermMa = 60 },
                //new TestCase { Funds = 100000, BuyShortTermMa = 10, BuyLongTermMa = 20, SellShortTermMa = 10, SellLongTermMa = 20 },
                //new TestCase { Funds = 100000, BuyShortTermMa = 10, BuyLongTermMa = 60, SellShortTermMa = 10, SellLongTermMa = 60 },
                //new TestCase { Funds = 100000, BuyShortTermMa = 20, BuyLongTermMa = 120, SellShortTermMa = 20, SellLongTermMa = 120 },
                //new TestCase { Funds = 100000, BuyShortTermMa = 20, BuyLongTermMa = 240, SellShortTermMa = 20, SellLongTermMa = 240 },
                //new TestCase { Funds = 100000, BuyShortTermMa = 60, BuyLongTermMa = 120, SellShortTermMa = 60, SellLongTermMa = 120 },
                //new TestCase { Funds = 100000, BuyShortTermMa = 60, BuyLongTermMa = 240, SellShortTermMa = 60, SellLongTermMa = 240 },
            };

            //for (var s = 1; s <= 240; s++)
            //{
            //    for (var l = 1; l <= 240 ; l++)
            //    {
            //        if (s < l)
            //        {
            //            testCase.Add(new TestCase { Funds = 100000, ShortTermMa = s, LongTermMa = l });
            //        }
            //    }
            //}
            stockSymbol.ForEach(symbol =>
            {
                var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
                var stockList = _dataService.GetPeriodDataFromYahooApi(symbol, new DateTime(2020,1,1,0,0,0), periodEnd.AddDays(1));
                ChartData data = _researchOperationService.GetMaFromYahoo(symbol, stockList);
                var currentStock = stockList.Last().Price ?? 0;
                Console.WriteLine($"{symbol}\t Current Stock: {currentStock}");
                chartDataList.Add(data);
                testCase.ForEach(currentTestCase =>
                {
                    var myTrans = _researchOperationService.GetMyTransactionsOddShares(currentTestCase.Funds, data, stockList, currentTestCase);
                    myTransList.Add(new StockTransList { Name = symbol, TestCase = currentTestCase, Transactions = myTrans });
                    string resultString = _researchOperationService.Settlement(currentStock, myTrans, currentTestCase, Utils.ConvertToUnixTimestamp(periodEnd));
                    //if (!resultString.Contains("Earned: 0"))
                    //{
                        Console.WriteLine(resultString);
                    //}
                    var transString = "=== My Transaction ======================================================================================\n";
                    transString += $"|\tName: {symbol}\t Test Case: {currentTestCase.BuyShortTermMa}MA vs {currentTestCase.BuyLongTermMa}MA \t Current Stock:{stockList.Last().Price}\n";
                    myTrans.ForEach(t=>
                    {
                        transString += $"|\t TransType: {t.TransType}\n|\t TransTime: {t.TransTimeString}\t TransVolume:{t.TransVolume}\t Fees:{t.Fees}\t " +
                        $"Tax: {t.Tax}\t Balance: {t.Balance}\t TransPrice: {t.TransPrice}\n";
                    });
                    transString +=    "=========================================================================================================\n";
                    //Console.WriteLine(transString);
                });
                //_fileHandler.OutputEarn(myTransList, $"{symbol} Earn");
                //myTransList = new List<StockTransList>();
            });
            _fileHandler.OutputTransaction(myTransList, "AllTransaction");
            _fileHandler.OutputCsv(chartDataList, "chartDataCsv");
        }
    }
}
