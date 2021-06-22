using System;
using System.Collections.Generic;
using System.Linq;

namespace Stock.Analysis._0607
{
    public class MovingAvarageService: IMovingAvarageService
    {
        public MovingAvarageService()
        {

        }

        public List<double> CalculateMovingAvarage(List<StockModel> stockList, int avgDay)
        {
            Console.WriteLine($"Calculating {avgDay}-day moving avarage...");
            var avgList = new List<StockModel>();
            var sortedStock = stockList.OrderByDescending(s => s.Date).ToList();
            double firstSum = 0;
            for (var i = 0; i < avgDay; i++)
            {
                firstSum += sortedStock[i].Price;
            }

            var index = 0;
            double prePrice = 0;
            sortedStock.ForEach(stock =>
            {
                double sumPrice = 0;
                var stopCaculate = (index + avgDay - 1) >= sortedStock.Count;
                if (index == 0)
                {
                    sumPrice = firstSum;
                }
                else if (!stopCaculate)
                {
                    sumPrice = prePrice - sortedStock[index - 1].Price + sortedStock[index + avgDay - 1].Price;
                }

                avgList.Add(new StockModel{
                    Date = stock.Date,
                    Price = Math.Round(sumPrice / avgDay, 4)
                });
                prePrice = sumPrice;
                index++;
            });
            return avgList.OrderBy(s => s.Date).Select(s => s.Price).ToList();
        }
    }

    public interface IMovingAvarageService
    {
        List<double> CalculateMovingAvarage(List<StockModel> stockList, int avgDay);
    }
}
