using System;
using System.Collections.Generic;
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

        public ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList)
        {
            var chartData = new ChartData();
            chartData.Name = $"{symbol}";
            chartData.Min = stockList.Where(stock => stock.Price != null).Min(stock => (double)stock.Price);
            chartData.Max = stockList.Where(stock => stock.Price != null).Max(stock => (double)stock.Price);
            chartData.Day = stockList.Select(s => {
                var currentDateTime = UnixTimeStampToDateTime(s.Date);
                return $"{currentDateTime.Year}/{currentDateTime.Month}/{currentDateTime.Day}";
                }).ToList();
            chartData.Timestamp = stockList.Select(s => s.Date).ToList();
            chartData.Price = stockList.Select(s => s.Price).ToList();
            chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList();
            chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10).Select(s => s.Price).ToList();
            chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20).Select(s => s.Price).ToList();
            chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60).Select(s => s.Price).ToList();

            return chartData;
        }

        public List<ChartData> GetMaFromCsv(string path)
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
                chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList();
                chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10).Select(s => s.Price).ToList();
                chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20).Select(s => s.Price).ToList();
                chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60).Select(s => s.Price).ToList();

                chartDataList.Add(chartData);
                index++;
            });
            return chartDataList;
        }

        public string Settlement(double currentStock,List<StockTransaction> myTrans, TestCase ma, double periodEnd)
        {
            var hasQty = myTrans.Last().TransType == TransactionType.Buy;
            if (hasQty)
            {
                var timeString = Utils.UnixTimeStampToDateTime(periodEnd);
                var price = Math.Round(currentStock, 2, MidpointRounding.AwayFromZero);
                myTrans.Add(new StockTransaction
                {
                    TransTime = periodEnd,
                    TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                    TransPrice = price,
                    TransType = TransactionType.Sell,
                    TransVolume = myTrans.Last().TransVolume,
                    Balance = myTrans.Last().Balance + Math.Round(currentStock * myTrans.Last().TransVolume, MidpointRounding.ToZero)
                });
            }
            var buy = myTrans.Where(trans => trans.TransType == TransactionType.Buy)
                .Sum(trans => Math.Round(trans.TransPrice * trans.TransVolume, MidpointRounding.ToZero));
            var sell = myTrans.Where(trans => trans.TransType == TransactionType.Sell)
                .Sum(trans => Math.Round(trans.TransPrice * trans.TransVolume, MidpointRounding.ToZero));
            var earn = sell - buy;
            var resultString = $"When ma = Buy: {ma.BuyShortTermMa} vs {ma.BuyLongTermMa}; Sell: {ma.SellShortTermMa} vs {ma.SellLongTermMa};,\tEarned: {earn}\t";
            return resultString;
        }

        public List<StockTransaction> GetMyTransactionsOddShares(ChartData data, List<StockModel> stockList, TestCase testCase)
        {
            var myTransactions = new List<StockTransaction>();
            myTransactions.Add(new StockTransaction
            {
                TransTime = 0,
                TransTimeString = string.Empty,
                TransPrice = 0,
                TransVolume = 0,
                TransType = TransactionType.AddFunds,
                Balance = testCase.Funds
            });
            var symbol = data.Name;
            var index = 0;
            List<double?> buyShortMaValList;
            List<double?> buyLongMaValList;
            buyShortMaValList = !data.GetMaValue(testCase.BuyShortTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, testCase.BuyShortTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(testCase.BuyShortTermMa);

            buyLongMaValList = !data.GetMaValue(testCase.BuyLongTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, testCase.BuyLongTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(testCase.BuyLongTermMa);
            List<double?> sellShortMaValList;
            List<double?> sellLongMaValList;
            sellShortMaValList = !data.GetMaValue(testCase.SellShortTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, testCase.SellShortTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(testCase.SellShortTermMa);

            sellLongMaValList = !data.GetMaValue(testCase.SellLongTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, testCase.SellLongTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(testCase.SellLongTermMa);

            bool hasQty = false;
            var check = false;
            double maxPrice = 0;
            data.Timestamp.ForEach(timestamp =>
            {
                var timeString = Utils.UnixTimeStampToDateTime(timestamp);
                if (buyShortMaValList[index] != null && buyLongMaValList[index] != null && sellShortMaValList[index] != null && sellLongMaValList[index] != null)
                {
                    var price = data.Price[index] ?? 0;
                    price = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                    var lastTrans = myTransactions.Any() ? myTransactions.LastOrDefault() : new StockTransaction();
                    if (_transTimingService.TimeToBuy(buyShortMaValList, buyLongMaValList, index, hasQty))
                    {
                        var volume = _calculateVolumeService.CalculateBuyingVolumeOddShares(lastTrans.Balance, price);
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = timestamp,
                            TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                            TransPrice = price,
                            TransType = TransactionType.Buy,
                            TransVolume = volume,
                            Balance = lastTrans.Balance - Math.Round(price * volume, MidpointRounding.ToZero),
                            BuyShortMaPrice = buyShortMaValList[index],
                            BuyLongMaPrice = buyLongMaValList[index],
                            BuyShortMaPrice1DayBefore = buyShortMaValList[index-1],
                            BuyLongMaPrice1DayBefore = buyLongMaValList[index-1],
                        });
                        hasQty = !hasQty;
                    }
                    // todo: 停損比例改為參數，從testcase丟進來
                    // todo: 注意現在是用哪一種時機點
                    else if (_transTimingService.TimeToSell(sellShortMaValList, sellLongMaValList, index, hasQty))
                    {
                        var volume = _calculateVolumeService.CalculateSellingVolume(myTransactions.LastOrDefault().TransVolume);
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = timestamp,
                            TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                            TransPrice = price,
                            TransType = TransactionType.Sell,
                            TransVolume = volume,
                            Balance = lastTrans.Balance + Math.Round(price * volume, MidpointRounding.ToZero),
                            SellShortMaPrice = sellShortMaValList[index],
                            SellLongMaPrice = sellLongMaValList[index],
                            SellShortMaPrice1DayBefore = sellShortMaValList[index - 1],
                            SellLongMaPrice1DayBefore = sellLongMaValList[index - 1],
                        });
                        hasQty = !hasQty;
                        maxPrice = 0;
                    }
                }

                index++;
            });

            return myTransactions;
        }
    }

    public interface IResearchOperationService
    {
        List<ChartData> GetMaFromCsv(string path);
        ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList);
        string Settlement(double currentStock, List<StockTransaction> myTrans, TestCase ma, double periodEnd);
        List<StockTransaction> GetMyTransactionsOddShares(ChartData data, List<StockModel> stockList, TestCase testCase);
    }
}
