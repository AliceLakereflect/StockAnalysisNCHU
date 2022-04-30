using System;
using System.Linq;
using Newtonsoft.Json;
using Stock.Analysis._0607.Models;
using Stock.Analysis._0607.Service;
using Xunit;

namespace Stock.Analysis.Tests.Service
{
    public class DataServiceTests
    {
        private IDataService _dataService = new DataService();
        public DataServiceTests()
        {
        }

        [Fact]
        public void SendGetRequestTest()
        {
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/AAPL";
            var responseContent = _dataService.SendGetRequest(url);
            var data = JsonConvert.DeserializeObject<YahooFinanceChartDataModel>(responseContent);
            Assert.NotEmpty(responseContent);
            Assert.NotNull(data?.chart?.result?.First()?.meta);
            Assert.Equal("AAPL", data.chart.result.First().meta.symbol);
            Assert.Equal("USD", data.chart.result.First().meta.currency);
            Assert.NotNull(data?.chart?.result?.First()?.indicators?.quote?.First());
            Assert.True(data.chart.result.First().indicators.quote.First().ContainsKey("close"));
        }

        [Fact]
        public void Get1dDataFromYahooApiTest()
        {
            var symbol = "AAPL";
            var result = _dataService.Get1dDataFromYahooApi(symbol);
            Assert.NotEmpty(result);
            Assert.Equal(391, result.Count);
        }

        [Fact]
        public void Get1YDataFromYahooApiTest()
        {
            var symbol = "AAPL";
            var result = _dataService.Get1YDataFromYahooApi(symbol);
            Assert.NotEmpty(result);
            Assert.Equal(253, result.Count);
        }

        [Fact]
        public void GetPeriodDataFromYahooApiTest()
        {
            var symbol = "AAPL";
            var period1 = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var period2 = new DateTime(2021, 6, 30, 0, 0, 0, DateTimeKind.Utc);
            var result = _dataService.GetPeriodDataFromYahooApi(symbol, period1, period2);
            Assert.NotEmpty(result);
            Assert.Equal(376, result.Count);
        }
    }
}
