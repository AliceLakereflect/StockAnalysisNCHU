using System;
using Xunit;
using Stock.Analysis._0607.Service;
using System.Collections.Generic;

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
                new StockModel{ Date = dateTime.AddDays(0), Price = 10 },
                new StockModel{ Date = dateTime.AddDays(1), Price = 11 },
                new StockModel{ Date = dateTime.AddDays(2), Price = 12 },
                new StockModel{ Date = dateTime.AddDays(5), Price = 13 },
                new StockModel{ Date = dateTime.AddDays(6), Price = 14 },
                new StockModel{ Date = dateTime.AddDays(7), Price = 15 },
                new StockModel{ Date = dateTime.AddDays(8), Price = 16 },
                new StockModel{ Date = dateTime.AddDays(9), Price = 17 },
                new StockModel{ Date = dateTime.AddDays(12), Price = 18 },
                new StockModel{ Date = dateTime.AddDays(13), Price = 19 },
                new StockModel{ Date = dateTime.AddDays(14), Price = 20 },
                new StockModel{ Date = dateTime.AddDays(15), Price = 21 },
                new StockModel{ Date = dateTime.AddDays(16), Price = 22 },
                new StockModel{ Date = dateTime.AddDays(19), Price = 23 },
                new StockModel{ Date = dateTime.AddDays(20), Price = 24 },
                new StockModel{ Date = dateTime.AddDays(21), Price = 25 },
                new StockModel{ Date = dateTime.AddDays(22), Price = 26 },
                new StockModel{ Date = dateTime.AddDays(23), Price = 27 },
                new StockModel{ Date = dateTime.AddDays(26), Price = 28 },
                new StockModel{ Date = dateTime.AddDays(27), Price = 29 }
            };
            var result = _movingAvgService.CalculateMovingAvarage(stockList, 5);
            Assert.Equal(0, result[0]);
            Assert.Equal(0, result[1]);
            Assert.Equal(0, result[2]);
            Assert.Equal(0, result[3]);
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
                    Date = dateTime.AddDays(i),
                    Price = i+10
                });
            }
            
            var result = _movingAvgService.CalculateMovingAvarage(stockList, 60);
            var index = 0;
            var limit = 59;
            var firstExpected = 39.5;
            result.ForEach(stock=>
            {
                if(index < limit)
                {
                    Assert.Equal(0, result[index]);
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
