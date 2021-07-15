using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class ResearchOperationService: IResearchOperationService
    {
        private IMovingAvarageService _movingAvgService = new MovingAvarageService();
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
            chartData.Day = stockList.Select(s => UnixTimeStampToDateTime(s.Date).ToString()).ToList();
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

        public string Settlement(List<StockTransaction> myTrans, TestCase ma)
        {
            var hasQty = myTrans.Last().TransVolume == 1;
            var buy = myTrans.Where(trans => trans.TransVolume == 1).Sum(trans => trans.TransPrice);
            var sell = myTrans.Where(trans => trans.TransVolume == -1).Sum(trans => trans.TransPrice);
            var earn = hasQty ? sell - buy + myTrans.Last().TransPrice * myTrans.Last().TransVolume : sell - buy;
            var resultString = hasQty
                                ? $"When ma = {ma.ShortTermMa} vs {ma.LongTermMa},\tEarned: {earn}\t(Include holdings: {myTrans.Last().TransPrice})"
                                : $"When ma = {ma.ShortTermMa} vs {ma.LongTermMa},\tEarned: {earn}\t";
            Console.WriteLine(resultString);
            return resultString;
        }

        public List<StockTransaction> GetMyTransactions(ChartData data, List<StockModel> stockList, int shortTermMa, int LongTermMa)
        {
            var myTransactions = new List<StockTransaction>();
            var symbol = data.Name;
            var index = 0;
            List<double?> shortMaValList;
            List<double?> longMaValList;
            shortMaValList = !data.GetMaValue(shortTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, shortTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(shortTermMa);

            longMaValList = !data.GetMaValue(LongTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, LongTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(LongTermMa);

            bool hasQty = false;
            var missedBuying = false;
            var first = true;
            data.Timestamp.ForEach(timestamp =>
            {
                var shortMaVal = shortMaValList[index];
                var longMaVal = longMaValList[index];
                if (shortMaVal != null && longMaVal != null)
                {
                    IfMissedBuying(ref missedBuying, ref first, shortMaVal, longMaVal);

                    if (TimeToBuy(shortMaVal, longMaVal, hasQty, missedBuying))
                    {
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = data.Timestamp[index],
                            TransPrice = data.Price[index] ?? 0,
                            TransVolume = 1
                        });
                        hasQty = !hasQty;
                    }
                    else if (TimeToSell(shortMaVal, longMaVal, hasQty))
                    {
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = data.Timestamp[index],
                            TransPrice = data.Price[index] ?? 0,
                            TransVolume = -1
                        });
                        hasQty = !hasQty;
                    }
                }

                index++;
            });

            return myTransactions;
        }

        private static void IfMissedBuying(ref bool missedBuying, ref bool first, double? shortMaVal, double? longMaVal)
        {
            if (first && shortMaVal >= longMaVal)
            {
                missedBuying = true;
            }
            first = false;
            if (missedBuying && shortMaVal < longMaVal)
            {
                missedBuying = false;
            }
        }

        private bool TimeToBuy(double? shortMaVal, double? longMaVal, bool hasQty, bool missedBuying)
        {
            return shortMaVal >= longMaVal && hasQty == false && !missedBuying;
        }

        private bool TimeToSell(double? shortMaVal, double? longMaVal, bool hasQty)
        {
            return shortMaVal <= longMaVal && hasQty == true;
        }
    }

    public interface IResearchOperationService
    {
        List<ChartData> GetMaFromCsv(string path);
        ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList);
        string Settlement(List<StockTransaction> myTrans, TestCase ma);
        List<StockTransaction> GetMyTransactions(ChartData data, List<StockModel> stockList, int shortTermMa, int LongTermMa);
    }
}
