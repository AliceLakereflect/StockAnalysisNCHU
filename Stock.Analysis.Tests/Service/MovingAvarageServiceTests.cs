using System;
using Xunit;
using Stock.Analysis._0607.Service;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis.Tests.Service
{
    public class MovingAvarageServiceTests
    {
        private IMovingAvarageService _movingAvgService = new MovingAvarageService();
        private IDataService _dataService = new DataService();
        private List<double?> ma5FromYahoo = new List<double?> { 95.26, 100.24, 106.5, 110.52, 112.9, 114.4, 115.5, 114.4, 115.9, 121, 124.6,
            128.7, 134, 139.6, 143.1, 144.1, 147.2, 151.2, 156, 161.2, 172.9
        };
        private List<double?> ma10FromYahoo = new List<double?> { 88.31, 91.73, 95.93, 99.11, 101.52, 104.83, 107.87, 110.45, 113.21, 116.95, 119.5,
            122.1, 124.2, 127.75, 132.05, 134.35, 137.95, 142.6, 147.8, 152.15, 158.5
        };
        private List<double?> ma20FromYahoo = new List<double?> { 84.22, 85.75, 87.64, 89.22, 90.07, 91.06, 92.56, 94.36, 97.05, 100.7, 103.9,
            106.91, 110.06, 113.43, 116.78, 119.59, 122.91, 126.53, 130.5, 134.55, 139
        };
        private List<double?> ma60FromYahoo = new List<double?> { 63.8, 65.02, 66.38, 67.7, 68.91, 70.2, 71.49, 72.74, 74.14, 75.75, 77.3,
            78.88, 80.52, 82.29, 84.14, 85.72, 87.55, 89.5, 91.71, 93.97, 96.49
        };
        ChartData _historicalData;
        List<StockModel> _stockList = new List<StockModel>();

        public MovingAvarageServiceTests()
        {
            DateTime.TryParse("2020-05-08", out var _dateTime);
            _historicalData = new ChartData
            {
                Name = "AAPL",
                Price = new List<double?>()
            };
            _stockList = new List<StockModel>();
            var dayIndex = 0;
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                _stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                _historicalData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 0; i < 90; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                _stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                _historicalData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                _stockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                _historicalData.Price.Add(i + 10);
                dayIndex++;
            }
            _historicalData.Timestamp = _stockList.Select(s => s.Date).ToList();

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
            var result = _movingAvgService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList(); ;
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
        public void CalculateMovingAvarage5()
        {
            var result = _movingAvgService.CalculateMovingAvarage(_stockList, 5).Select(s => s.Price).ToList();
            var index = 0;
            var limit = 4;
            var descExpected = 98;
            var ascExpected = 12;
            result.ForEach(stock =>
            {
                if (index < limit)
                {
                    Assert.Null(result[index]);
                }
                else if (index < 90)
                {
                    var expected = descExpected + limit - Convert.ToDouble(index);
                    Assert.Equal(expected, result[index]);
                }
                else if (index == 91)
                {
                    Assert.Equal(11.4, result[index]);
                }
                else if (index == 92)
                {
                    Assert.Equal(11.2, result[index]);
                }
                else if (index == 93)
                {
                    Assert.Equal(11.4, result[index]);
                }
                else if (index > 94 && index < 180)
                {
                    var expected = ascExpected - 94 + Convert.ToDouble(index);
                    Assert.Equal(expected, result[index]);
                }
                

                index++;
            });

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
                    Price = i + 10
                });
            }

            var result = _movingAvgService.CalculateMovingAvarage(stockList, 60).Select(s => s.Price).ToList();
            var index = 0;
            var limit = 59;
            var firstExpected = 39.5;
            result.ForEach(stock =>
            {
                if (index < limit)
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

        [Fact]
        public void TestWithRealData()
        {
            const string symbol = "2603.TW";
            // todo: change to static data
            var dataList = _dataService.GetPeriodDataFromYahooApi(symbol, new DateTime(2021,3,1,0,0,0), new DateTime(2021,7,1,0,0,0));
            var ma5 = _movingAvgService.CalculateMovingAvarage(dataList, 5).Select(s => s.Price).ToList().GetRange(61,21);
            var ma10 = _movingAvgService.CalculateMovingAvarage(dataList, 10).Select(s => s.Price).ToList().GetRange(61, 21);
            var ma20 = _movingAvgService.CalculateMovingAvarage(dataList, 20).Select(s => s.Price).ToList().GetRange(61, 21);
            var ma60 = _movingAvgService.CalculateMovingAvarage(dataList, 60).Select(s => s.Price).ToList().GetRange(61, 21);

            Assert.Equal<double?>(ma5FromYahoo, ma5);
            Assert.Equal<double?>(ma10FromYahoo, ma10);
            Assert.Equal<double?>(ma20FromYahoo, ma20);
            Assert.Equal<double?>(ma60FromYahoo, ma60);
        }
    }
}
