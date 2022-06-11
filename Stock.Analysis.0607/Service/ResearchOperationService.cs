using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class ResearchOperationService: IResearchOperationService
    {
        private IMovingAvarageService _movingAvgService;
        private ITransTimingService _transTimingService;
        private ICalculateVolumeService _calculateVolumeService;
        private IFileHandler _fileHandler;
        public ResearchOperationService(
            IMovingAvarageService movingAvgService,
            ITransTimingService transTimingService,
            ICalculateVolumeService calculateVolumeService,
            IFileHandler fileHandler)
        {
            _movingAvgService = movingAvgService ?? throw new ArgumentNullException(nameof(movingAvgService));
            _transTimingService = transTimingService ?? throw new ArgumentNullException(nameof(transTimingService));
            _calculateVolumeService = calculateVolumeService ?? throw new ArgumentNullException(nameof(calculateVolumeService));
            _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        }

        public void CalculateAllMa(ref List<StockModel> stockList)
        {
            for (var i = 1; i <= 256; i++)
            {
                stockList = _movingAvgService.CalculateMovingAvarage(stockList, i);
            }
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

        public List<StockTransaction> ProfitSettlement(double currentStock, List<StockModelDTO> stockList, TestCase testCase, List<StockTransaction> myTrans, double periodEnd)
        {
            var hasQty = myTrans.Last().TransType == TransactionType.Buy;
            if (hasQty && myTrans.Last().TransTime == stockList.Last().Date) {
                myTrans.RemoveAt(myTrans.Count - 1);
            }
            else if (hasQty)
            {
                //var timeString = Utils.UnixTimeStampToDateTime(periodEnd);
                var price = currentStock;
                var sellMaValList = stockList.TakeLast(2);
                myTrans.Add(new StockTransaction
                {
                    TransTime = periodEnd,
                    //TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                    TransPrice = price,
                    TransType = TransactionType.Sell,
                    TransVolume = myTrans.Last().TransVolume,
                    Balance = myTrans.Last().Balance + Math.Round(currentStock * myTrans.Last().TransVolume, 10, MidpointRounding.ToZero),
                    SellShortMaPrice = sellMaValList.LastOrDefault().MaList[testCase.SellShortTermMa] ?? 0,
                    SellLongMaPrice = sellMaValList.LastOrDefault().MaList[testCase.SellLongTermMa] ?? 0,
                    SellShortMaPrice1DayBefore = sellMaValList.FirstOrDefault().MaList[testCase.SellShortTermMa] ?? 0,
                    SellLongMaPrice1DayBefore = sellMaValList.FirstOrDefault().MaList[testCase.SellLongTermMa] ?? 0,
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
            List<StockModelDTO> stockList,
            TestCase testCase,
            double periodStartTimeStamp)
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
            var symbol = testCase.Symbol;

            bool hasQty = false;
            StockModelDTO prevStock = stockList.FirstOrDefault();
            stockList.ForEach(stock =>
            {
                var buyShortMaVal = stock.MaList[testCase.BuyShortTermMa] ?? null;
                var buyLongMaVal = stock.MaList[testCase.BuyLongTermMa] ?? null;
                var sellShortMa = stock.MaList[testCase.SellShortTermMa] ?? null;
                var sellLongMaVal = stock.MaList[testCase.SellLongTermMa] ?? null;

                var prevBuyShortMa = prevStock.MaList[testCase.BuyShortTermMa] ?? null;
                var prevBuyLongMaVal = prevStock.MaList[testCase.BuyLongTermMa] ?? null;
                var prevSellShortMaVal = prevStock.MaList[testCase.SellShortTermMa] ?? null;
                var prevSellLongMaVal = prevStock.MaList[testCase.SellLongTermMa] ?? null;
                if (stock.Date > periodStartTimeStamp)
                {
                    var price = stock.Price ?? 0;
                    
                    bool testToBuy = _transTimingService.TimeToBuy(buyShortMaVal, buyLongMaVal, prevBuyShortMa, prevBuyLongMaVal, hasQty);
                    bool testToSell = _transTimingService.TimeToSell(sellShortMa, sellLongMaVal, prevSellShortMaVal, prevSellLongMaVal, hasQty);
                    
                    if (buyShortMaVal != null && buyLongMaVal != null && testToBuy)
                    {
                        
                        var volume = _calculateVolumeService.CalculateBuyingVolumeOddShares(lastTrans.Balance, price);
                        lastTrans = new StockTransaction
                        {
                            TransTime = stock.Date,
                            TransPrice = price,
                            TransType = TransactionType.Buy,
                            TransVolume = volume,
                            Balance = lastTrans.Balance - Math.Round(price * volume, 10, MidpointRounding.ToZero),
                            BuyShortMaPrice = buyShortMaVal,
                            BuyLongMaPrice = buyLongMaVal,
                            BuyShortMaPrice1DayBefore = prevBuyShortMa,
                            BuyLongMaPrice1DayBefore = prevBuyLongMaVal,
                        };
                        myTransactions.Add(lastTrans);
                        hasQty = !hasQty;
                        
                    }
                    // todo: 停損比例改為參數，從testcase丟進來
                    // todo: 注意現在是用哪一種時機點
                    else if (sellShortMa != null && sellLongMaVal != null && testToSell)
                    {
                        var volume = _calculateVolumeService.CalculateSellingVolume(myTransactions.LastOrDefault().TransVolume);
                        lastTrans = new StockTransaction
                        {
                            TransTime = stock.Date,
                            TransPrice = price,
                            TransType = TransactionType.Sell,
                            TransVolume = volume,
                            Balance = lastTrans.Balance + Math.Round(price * volume, 10, MidpointRounding.ToZero),
                            SellShortMaPrice = sellShortMa,
                            SellLongMaPrice = sellLongMaVal,
                            SellShortMaPrice1DayBefore = prevSellShortMaVal,
                            SellLongMaPrice1DayBefore = prevSellLongMaVal,
                        };
                        myTransactions.Add(lastTrans);
                        hasQty = !hasQty;
                    }
                }
                prevStock = stock;
            });

            var currentStock = stockList.Last().Price ?? 0;
            var periodEnd = stockList.Last().Date;
            ProfitSettlement(currentStock, stockList, testCase, myTransactions, periodEnd);

            return myTransactions;
        }
    }
}
