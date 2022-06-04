using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Stock.Analysis._0607.Interface;
using Stock.Analysis._0607.Models;
using Stock.Analysis._0607.Repository;
using Stock.Analysis._0607.Service;
using Xunit;

namespace Stock.Analysis.Tests.Service
{
    public class DataServiceTests
    {
        private IDataProvider<StockModel> _stockModeldataProvider = new Mock<IDataProvider<StockModel>>().Object;
        private IDataService _dataService;
        public DataServiceTests()
        {
            var connectString = "Host=localhost;Database=StockResearch;Username=postgres;Password=13";
            var options = new DbContextOptionsBuilder<StockModelDbContext>();
            options.UseNpgsql(connectString);
            _stockModeldataProvider = new StockModelDataProvider(new StockModelDbContext(options.Options));
            _dataService = new DataService(_stockModeldataProvider);
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
        public void Get1YDataFromYahooApiTest()
        {
            var symbol = "AAPL";
            var result = _dataService.GetStockDataFromDb(symbol, new DateTime(2021, 1, 1), new DateTime(2021, 12, 31));
            Assert.NotEmpty(result);
            Assert.Equal(251, result.Count);
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
