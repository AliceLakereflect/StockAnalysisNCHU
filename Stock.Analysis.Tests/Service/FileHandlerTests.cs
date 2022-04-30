using System;
using Xunit;
using Stock.Analysis._0607.Service;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Stock.Analysis._0607.Models;
using Moq;

namespace Stock.Analysis.Tests.Service
{
    public class FileHandlerTests
    {
        private IFileHandler _fileHandler;
        public FileHandlerTests()
        {
            _fileHandler = new FileHandler();
        }

        [Fact]
        public void ReadDataTest()
        {
            var path = Path.Combine(Environment.CurrentDirectory, @"MockData/20100101-20210531.csv");
            var result = _fileHandler.ReadDataFromFile(path);
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void OutputResultTest()
        {
            var chartDataList = new List<ChartData> {
                new ChartData{ Name = "0050.TW", Price = new List<double?> { 130.01, 130.25 }}
            };
            _fileHandler.OutputResult(chartDataList, "chartData");

            var path = Path.Combine(Environment.CurrentDirectory, @"Output/chartData.json");
            var file = File.ReadAllText(path);
            Assert.True(File.Exists(path));
            Assert.NotNull(file);
            Assert.NotEmpty(file);
            var content = JsonConvert.DeserializeObject<List<ChartData>>(file);
            Assert.Single(content);
            
        }

        [Fact]
        public void OutputCsvTest()
        {
            var chartDataList = new List<ChartData> {
                new ChartData{ Name = "0050.TW",
                    Day = new List<string> { "day1", "day2" },
                    Price = new List<double?> { 130.01, 130.25 },
                    MaList = new Dictionary<int, List<double?>>
                    {
                        {5, new List<double?> { 130.01, 130.25 }},
                        {10, new List<double?> { 130.01, 130.25 }},
                        {20, new List<double?> { 130.01, 130.25 }},
                        {60, new List<double?> { 130.01, 130.25 }}
                    },
                    Timestamp = new List<double>{ 1, 2 } }
            };
            _fileHandler.OutputCsv(chartDataList, "chartData");

            var path = Path.Combine(Environment.CurrentDirectory, @"Output/chartData.csv");
            var file = File.ReadAllText(path);
            Assert.True(File.Exists(path));
            Assert.NotNull(file);
            Assert.NotEmpty(file);
        }

        [Fact]
        public void ReadRandom400wTest()
        {
            //var result = _fileHandler.Readcsv("Data/srand343");
            //Assert.NotNull(result);
            //Assert.Equal(160000000, result.Count);
            //Assert.Equal(1158,result.Dequeue());
            //Assert.Equal(24406,result.Dequeue());
        }

    }
}
