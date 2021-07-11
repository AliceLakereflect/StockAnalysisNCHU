using System;
using Xunit;
using Stock.Analysis._0607.Service;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Stock.Analysis.Tests.Service
{
    public class FileHandlerTests
    {
        private IFileHandler _fileHandler = new FileHandler();
        public FileHandlerTests()
        {

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

    }
}
