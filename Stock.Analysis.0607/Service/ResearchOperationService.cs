using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class ResearchOperationService: IResearchOperationService
    {
        private IMovingAvarageService _movingAvgService = new MovingAvarageService();
        private ITransTimingService _transTimingService = new TransTimingService();
        private ICalculateVolumeService _calculateVolumeService = new CalculateVolumeService();
        private IFileHandler _fileHandler = new FileHandler();
        public ResearchOperationService()
        {
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList, double start, double end)
        {
            var chartData = new ChartData();
            //chartData = _fileHandler.ReadMaByFile($"{symbol} ma");
            if(chartData.Name == null)
            {
                chartData.Name = $"{symbol}";
                chartData.Min = stockList.Where(stock => stock.Price != null).Min(stock => (double)stock.Price);
                chartData.Max = stockList.Where(stock => stock.Price != null).Max(stock => (double)stock.Price);
                stockList.ForEach(s => {
                    if (s.Date >= start && s.Date < end)
                    {
                        chartData.Timestamp.Add(s.Date);
                        var currentDateTime = UnixTimeStampToDateTime(s.Date);
                        var d = $"{currentDateTime.Year}/{currentDateTime.Month}/{currentDateTime.Day}";
                        chartData.Day.Add(d);
                        chartData.Price.Add(s.Price);
                    }
                });
                for (var i = 1; i <= 256; i++)
                {
                    var maList = new List<double?>();
                    _movingAvgService.CalculateMovingAvarage(stockList, i).ForEach(ma =>
                    {
                        if (ma.Date >= start && ma.Date < end)
                        {
                            maList.Add(ma.Price);
                        }
                    });
                    chartData.MaList.Add(i, maList);
                }

                _fileHandler.OutputCsv(new List<ChartData> { chartData }, $"{symbol} ma");
            }

            return chartData;
        }

        public List<ChartData> CalculateMaFromCsv(string path)
        {
            var stockData = _fileHandler.ReadDataFromFile(path);
            var index = 0;
            var chartDataList = new List<ChartData>();
            stockData.ForEach(stockList =>
            {
                Console.WriteLine($"Stock {index}");
                var chartData = new ChartData();
                chartData.Name = $"Stock {index}";
                chartData.Day = stockList.Select(s => s.Date.ToString()).ToList();
                chartData.Price = stockList.Select(s => s.Price).ToList();

                chartDataList.Add(chartData);
                index++;
            });
            return chartDataList;
        }

        public List<StockTransaction> ProfitSettlement(double currentStock, List<StockModel> stockList, TestCase testCase, List<StockTransaction> myTrans, double periodEnd)
        {
            var hasQty = myTrans.Last().TransType == TransactionType.Buy;
            if (hasQty && myTrans.Last().TransTime == stockList.Last().Date) {
                myTrans.RemoveAt(myTrans.Count - 1);
            }
            else if (hasQty)
            {
                //var timeString = Utils.UnixTimeStampToDateTime(periodEnd);
                var price = Math.Round(currentStock, 6, MidpointRounding.AwayFromZero);
                var sellShortMaValList = _movingAvgService.CalculateMovingAvarage(stockList, testCase.SellShortTermMa).Select(stock => stock.Price).TakeLast(2).ToList();
                var sellLongMaValList = _movingAvgService.CalculateMovingAvarage(stockList, testCase.SellLongTermMa).Select(stock => stock.Price).TakeLast(2).ToList();
                myTrans.Add(new StockTransaction
                {
                    TransTime = periodEnd,
                    //TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                    TransPrice = price,
                    TransType = TransactionType.Sell,
                    TransVolume = myTrans.Last().TransVolume,
                    Balance = myTrans.Last().Balance + Math.Round(currentStock * myTrans.Last().TransVolume, 10, MidpointRounding.ToZero),
                    SellShortMaPrice = sellShortMaValList.LastOrDefault() ?? 0,
                    SellLongMaPrice = sellLongMaValList.LastOrDefault() ?? 0,
                    SellShortMaPrice1DayBefore = sellShortMaValList.FirstOrDefault() ?? 0,
                    SellLongMaPrice1DayBefore = sellLongMaValList.FirstOrDefault() ?? 0,
                });
            }

            return myTrans;
        }

        public double GetEarningsResults(List<StockTransaction> myTrans)
        {
            var buy = myTrans.Where(trans => trans.TransType == TransactionType.Buy)
                .Sum(trans => Math.Round(trans.TransPrice * trans.TransVolume, 10, MidpointRounding.ToZero));
            var sell = myTrans.Where(trans => trans.TransType == TransactionType.Sell)
                .Sum(trans => Math.Round(trans.TransPrice * trans.TransVolume, 10, MidpointRounding.ToZero));
            var earn = sell - buy + myTrans.FirstOrDefault(t=>t.TransType == TransactionType.AddFunds).Balance;
            return earn;
        }


        public List<StockTransaction> GetMyTransactions(
            ChartData data,
            List<StockModel> stockList,
            TestCase testCase,
            DateTime periodStart,
            Stopwatch sw1,
            Stopwatch sw2)
        {
            var myTransactions = new List<StockTransaction>();
            var lastTrans = new StockTransaction
            {
                TransTime = 0,
                TransTimeString = string.Empty,
                TransPrice = 0,
                TransVolume = 0,
                TransType = TransactionType.AddFunds,
                Balance = testCase.Funds
            };
            myTransactions.Add(lastTrans);
            var symbol = data.Name;
            var index = 0;
            List<double?> buyShortMaValList;
            List<double?> buyLongMaValList;
            List<double?> sellShortMaValList;
            List<double?> sellLongMaValList;

            buyShortMaValList = GetMaValue(testCase.BuyShortTermMa, data, stockList);
            buyLongMaValList = GetMaValue(testCase.BuyLongTermMa, data, stockList);
            sellShortMaValList = GetMaValue(testCase.SellShortTermMa, data, stockList);
            sellLongMaValList = GetMaValue(testCase.SellLongTermMa, data, stockList);

            bool hasQty = false;
            double maxPrice = 0;
            var periodStartTimeStamp = Utils.ConvertToUnixTimestamp(periodStart);
            bool hasBuyMaValue = buyShortMaValList.Any() && buyLongMaValList.Any();
            bool hasSellMaValue = sellShortMaValList.Any() && sellLongMaValList.Any();
            data.Timestamp.ForEach(timestamp =>
            {
                if (timestamp > periodStartTimeStamp)
                {
                    var price = data.Price[index] ?? 0;
                    price = Math.Round(price, 6, MidpointRounding.AwayFromZero);
                    sw1.Start();
                    bool test1 = _transTimingService.TimeToBuy(buyShortMaValList, buyLongMaValList, index, hasQty);
                    sw1.Stop();

                    sw2.Start();
                    bool test2 = _transTimingService.TimeToSell(sellShortMaValList, sellLongMaValList, index, hasQty);
                    sw2.Stop();
                    if (hasBuyMaValue && buyShortMaValList[index] != null && buyLongMaValList[index] != null && test1)
                    {
                        var volume = _calculateVolumeService.CalculateBuyingVolumeOddShares(lastTrans.Balance, price);
                        lastTrans = new StockTransaction
                        {
                            TransTime = timestamp,
                            TransPrice = price,
                            TransType = TransactionType.Buy,
                            TransVolume = volume,
                            Balance = lastTrans.Balance - Math.Round(price * volume, 10, MidpointRounding.ToZero),
                            BuyShortMaPrice = buyShortMaValList[index],
                            BuyLongMaPrice = buyLongMaValList[index],
                            BuyShortMaPrice1DayBefore = buyShortMaValList[index - 1],
                            BuyLongMaPrice1DayBefore = buyLongMaValList[index - 1],
                        };
                        myTransactions.Add(lastTrans);
                        hasQty = !hasQty;
                    }
                    // todo: 停損比例改為參數，從testcase丟進來
                    // todo: 注意現在是用哪一種時機點
                    else if (hasSellMaValue && sellShortMaValList[index] != null && sellLongMaValList[index] != null && test2)
                    {
                        var volume = _calculateVolumeService.CalculateSellingVolume(myTransactions.LastOrDefault().TransVolume);
                        lastTrans = new StockTransaction
                        {
                            TransTime = timestamp,
                            TransPrice = price,
                            TransType = TransactionType.Sell,
                            TransVolume = volume,
                            Balance = lastTrans.Balance + Math.Round(price * volume, 10, MidpointRounding.ToZero),
                            SellShortMaPrice = sellShortMaValList[index],
                            SellLongMaPrice = sellLongMaValList[index],
                            SellShortMaPrice1DayBefore = sellShortMaValList[index - 1],
                            SellLongMaPrice1DayBefore = sellLongMaValList[index - 1],
                        };
                        myTransactions.Add(lastTrans);
                        hasQty = !hasQty;
                        maxPrice = 0;
                    }
                }

                index++;
            });
            sw1.Stop();

            var currentStock = stockList.Last().Price ?? 0;
            var periodEnd = data.Timestamp.Last();
            ProfitSettlement(currentStock, stockList, testCase, myTransactions, periodEnd);

            return myTransactions;
        }

        public List<double?> GetMaValue(int avgDay, ChartData data, List<StockModel> stockList)
        {
            return data.MaList.ContainsKey(avgDay)
                ? data.MaList[avgDay]
                : _movingAvgService.CalculateMovingAvarage(stockList, avgDay).Select(stock => stock.Price).ToList();
        }
    }
}
