using System;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace Stock.Analysis.Tests.MockData
{
    public class HistoryRepository : IRepository
    {
        private readonly IMovingAvarageService _movingAvarageService = new MovingAvarageService();

        protected ChartData ascHistoryData;
        protected List<StockModelDTO> ascStockListDto = new List<StockModelDTO>();
        protected List<StockModel> ascStockList = new List<StockModel>();
        protected ChartData concussiveHistoryData;
        protected List<StockModelDTO> concussiveStockListDto = new List<StockModelDTO>();
        protected List<StockModel> concussiveStockList = new List<StockModel>();
        protected List<StockModel> realData1yOf2603 = new List<StockModel>();
        protected List<StockModel> realData120dOf2603 = new List<StockModel>();
        public HistoryRepository()
        {
        }

        public ChartData GetAscHistoryData()
        {
            if (ascHistoryData != null) return ascHistoryData;
            DateTime _dateTime;
            DateTime.TryParse("2020-05-08", out _dateTime);
            ascHistoryData = new ChartData
            {
                Name = "AAPL",
                Timestamp = new List<double>(),
                Price = new List<double?>()
            };
            for (var i = 0; i < 180; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(i).Ticks);
                ascStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });

                ascHistoryData.Price.Add(i + 10);
            }
            ascHistoryData.Timestamp = ascStockList.Select(s => s.Date).ToList();
            ascHistoryData.MaList.Add(5, _movingAvarageService.CalculateMovingAvarage(ascStockList, 5).Select(s => s.Price).ToList());
            ascHistoryData.MaList.Add(20, _movingAvarageService.CalculateMovingAvarage(ascStockList, 20).Select(s => s.Price).ToList());

            return ascHistoryData;
        }

        public List<StockModel> GetAscStockList()
        {
            if (ascStockList.Any()) return ascStockList;
            DateTime _dateTime;
            DateTime.TryParse("2020-05-08", out _dateTime);
            ascHistoryData = new ChartData
            {
                Name = "AAPL",
                Timestamp = new List<double>(),
                Price = new List<double?>()
            };
            for (var i = 0; i < 180; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(i).Ticks);
                ascStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });

                ascHistoryData.Price.Add(i + 10);
            }
            return ascStockList;
        }

        public ChartData GetConcussiveHistoryData()
        {
            if (concussiveHistoryData != null) return concussiveHistoryData;
            DateTime _dateTime;
            DateTime.TryParse("2020-05-08", out _dateTime);
            concussiveHistoryData = new ChartData
            {
                Name = "AAPL",
                Price = new List<double?>()
            };
            concussiveStockList = new List<StockModel>();
            var dayIndex = 0;
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 0; i < 90; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }
            concussiveHistoryData.Timestamp = concussiveStockList.Select(s => s.Date).ToList();
            concussiveHistoryData.MaList.Add(5, _movingAvarageService.CalculateMovingAvarage(concussiveStockList, 5).Select(s => s.Price).ToList());
            concussiveHistoryData.MaList.Add(20, _movingAvarageService.CalculateMovingAvarage(concussiveStockList, 20).Select(s => s.Price).ToList());

            return concussiveHistoryData;
        }

        public List<StockModelDTO> GetConcussiveStockListDTO()
        {
            if (concussiveStockListDto.Any()) return concussiveStockListDto;
            DateTime _dateTime;
            DateTime.TryParse("2020-05-08", out _dateTime);
            concussiveHistoryData = new ChartData
            {
                Name = "AAPL",
                Price = new List<double?>()
            };
            concussiveStockListDto = new List<StockModelDTO>();
            var dayIndex = 0;
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockListDto.Add(new StockModelDTO
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 0; i < 90; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockListDto.Add(new StockModelDTO
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockListDto.Add(new StockModelDTO
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }

            return concussiveStockListDto;
        }

        public List<StockModel> GetConcussiveStockList()
        {
            if (concussiveStockList.Any()) return concussiveStockList;
            DateTime _dateTime;
            DateTime.TryParse("2020-05-08", out _dateTime);
            concussiveHistoryData = new ChartData
            {
                Name = "AAPL",
                Price = new List<double?>()
            };
            concussiveStockList = new List<StockModel>();
            var dayIndex = 0;
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 0; i < 90; i++)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }
            for (var i = 90; i > 0; i--)
            {
                var elapsedSpan = new TimeSpan(_dateTime.AddDays(dayIndex).Ticks);
                concussiveStockList.Add(new StockModel
                {
                    Date = elapsedSpan.TotalSeconds,
                    Price = i + 10
                });
                concussiveHistoryData.Price.Add(i + 10);
                dayIndex++;
            }

            return concussiveStockList;
        }

        public List<StockModel> GetRealData1yOf2603()
        {
            if (realData1yOf2603.Any()) return realData1yOf2603;
            var path = Path.Combine(Environment.CurrentDirectory, @"MockData/dataList.json");
            var file = File.ReadAllText(path);
            realData1yOf2603 = JsonConvert.DeserializeObject<List<StockModel>>(file);

            return realData1yOf2603;
        }

        public List<StockModel> GetRealData120dOf2603()
        {
            if (realData120dOf2603.Any()) return realData120dOf2603;
            var path = Path.Combine(Environment.CurrentDirectory, @"MockData/dataList30d.json");
            var file = File.ReadAllText(path);
            realData120dOf2603 = JsonConvert.DeserializeObject<List<StockModel>>(file);

            return realData120dOf2603;
        }
    }

    public interface IRepository
    {
        ChartData GetAscHistoryData();
        List<StockModel> GetAscStockList();
        ChartData GetConcussiveHistoryData();
        List<StockModelDTO> GetConcussiveStockListDTO();
        List<StockModel> GetConcussiveStockList();
        List<StockModel> GetRealData1yOf2603();
        List<StockModel> GetRealData120dOf2603();
    }
}
