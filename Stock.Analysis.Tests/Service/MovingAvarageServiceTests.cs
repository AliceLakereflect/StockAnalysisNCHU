using System;
using Xunit;
using Stock.Analysis._0607.Service;
using System.Collections.Generic;
using System.Linq;

namespace Stock.Analysis.Tests.Service
{
    public class MovingAvarageServiceTests
    {
        private IMovingAvarageService _movingAvgService = new MovingAvarageService();
        public MovingAvarageServiceTests()
        {

        }
        [Fact]
        public void CalculateMovingAvarage()
        {
            DateTime.TryParse("2020-05-08", out var dateTime);
            var stockList = new List<StockModel> {
                new StockModel{ Date = dateTime.AddDays(0).Ticks, Price = 10 },
                new StockModel{ Date = dateTime.AddDays(1).Ticks, Price = 11 },
                new StockModel{ Date = dateTime.AddDays(2).Ticks, Price = 12 },
                new StockModel{ Date = dateTime.AddDays(5).Ticks, Price = 13 },
                new StockModel{ Date = dateTime.AddDays(6).Ticks, Price = 14 },
                new StockModel{ Date = dateTime.AddDays(7).Ticks, Price = 15 },
                new StockModel{ Date = dateTime.AddDays(8).Ticks, Price = 16 },
                new StockModel{ Date = dateTime.AddDays(9).Ticks, Price = 17 },
                new StockModel{ Date = dateTime.AddDays(12).Ticks, Price = 18 },
                new StockModel{ Date = dateTime.AddDays(13).Ticks, Price = 19 },
                new StockModel{ Date = dateTime.AddDays(14).Ticks, Price = 20 },
                new StockModel{ Date = dateTime.AddDays(15).Ticks, Price = 21 },
                new StockModel{ Date = dateTime.AddDays(16).Ticks, Price = 22 },
                new StockModel{ Date = dateTime.AddDays(19).Ticks, Price = 23 },
                new StockModel{ Date = dateTime.AddDays(20).Ticks, Price = 24 },
                new StockModel{ Date = dateTime.AddDays(21).Ticks, Price = 25 },
                new StockModel{ Date = dateTime.AddDays(22).Ticks, Price = 26 },
                new StockModel{ Date = dateTime.AddDays(23).Ticks, Price = 27 },
                new StockModel{ Date = dateTime.AddDays(26).Ticks, Price = 28 },
                new StockModel{ Date = dateTime.AddDays(27).Ticks, Price = 29 }
            };
            var result = _movingAvgService.CalculateMovingAvarage(stockList, 5).OrderBy(s => s.Date).Select(s => s.Price).ToList(); ;
            Assert.Null(result[0]);
            Assert.Null(result[1]);
            Assert.Null(result[2]);
            Assert.Null(result[3]);
            Assert.Equal(12, result[4]);
            Assert.Equal(13, result[5]);
            Assert.Equal(14, result[6]);
            Assert.Equal(15, result[7]);
            Assert.Equal(16, result[8]);
            Assert.Equal(17, result[9]);
            Assert.Equal(18, result[10]);
            Assert.Equal(19, result[11]);
            Assert.Equal(20, result[12]);
            Assert.Equal(21, result[13]);
            Assert.Equal(22, result[14]);
            Assert.Equal(23, result[15]);
            Assert.Equal(24, result[16]);
            Assert.Equal(25, result[17]);
            Assert.Equal(26, result[18]);
            Assert.Equal(27, result[19]);
        }
        [Fact]
        public void CalculateMovingAvarage60()
        {
            DateTime.TryParse("2020-05-08", out var dateTime);
            var stockList = new List<StockModel>();
            for (var i = 0; i < 180; i++)
            {
                stockList.Add(new StockModel
                {
                    Date = dateTime.AddDays(i).Ticks,
                    Price = i+10
                });
            }
            
            var result = _movingAvgService.CalculateMovingAvarage(stockList, 60).OrderBy(s => s.Date).Select(s => s.Price).ToList();
            var index = 0;
            var limit = 59;
            var firstExpected = 39.5;
            result.ForEach(stock=>
            {
                if(index < limit)
                {
                    Assert.Null(result[index]);
                }
                else
                {
                    var expected = firstExpected - limit + Convert.ToDouble(index);
                    Assert.Equal(expected, result[index]);
                }

                index++;
            });
        }
    }
}
