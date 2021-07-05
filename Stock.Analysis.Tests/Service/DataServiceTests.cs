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
            Assert.Equal(390, result.Count);
        }

        [Fact]
        public void Get1YDataFromYahooApiTest()
        {
            var symbol = "AAPL";
            var result = _dataService.Get1YDataFromYahooApi(symbol);
            Assert.NotEmpty(result);
            Assert.Equal(252, result.Count);
        }
    }
}
