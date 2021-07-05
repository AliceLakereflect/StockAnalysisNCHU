using System;
using System.Collections.Generic;
using System.Linq;

namespace Stock.Analysis._0607.Service
{
    public class MovingAvarageService: IMovingAvarageService
    {
        public MovingAvarageService()
        {

        }

        public List<StockModel> CalculateMovingAvarage(List<StockModel> stockList, int avgDay)
        {
            Console.WriteLine($"Calculating {avgDay}-day moving avarage...");
            var avgList = new List<StockModel>();
            var sortedStock = stockList.OrderByDescending(s => s.Date).ToList();
            double? firstSum = 0;
            for (var i = 0; i < avgDay; i++)
            {
                firstSum += sortedStock[i].Price;
            }

            var index = 0;
            double? prePrice = 0;
            sortedStock.ForEach(stock =>
            {
                double? sumPrice = 0;
                var stopCaculate = (index + avgDay - 1) >= sortedStock.Count;
                if (index == 0)
                {
                    sumPrice = firstSum;
                }
                else if (!stopCaculate)
                {
                    sumPrice = sortedStock[index - 1].Price != null && sortedStock[index + avgDay - 1].Price  != null ?
                        prePrice - sortedStock[index - 1].Price + sortedStock[index + avgDay - 1].Price
                        : prePrice;
                }

                avgList.Add(new StockModel{
                    Date = stock.Date,
                    Price = sumPrice == 0 ? null : (double)Math.Round(((decimal)sumPrice) / avgDay, 4)
                });
                prePrice = sumPrice;
                index++;
            });
            return avgList;
        }

        public decimal CalculateSingleMovingAvarage(List<StockModel> stocks, int avgDay)
        {
            return 0;
        }
    }

    public interface IMovingAvarageService
    {
        List<StockModel?> CalculateMovingAvarage(List<StockModel> stockList, int avgDay);
        decimal CalculateSingleMovingAvarage(List<StockModel> stocks, int avgDay);
    }
}
