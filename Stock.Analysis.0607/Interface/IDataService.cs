using System;
using System.Collections.Generic;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface IDataService
    {
        List<StockModel> Get1YDataFromYahooApi(string stockSymbol);
        List<StockModel> Get1dDataFromYahooApi(string stockSymbol);
        List<StockModel> GetPeriodDataFromYahooApi(string stockSymbol, DateTime period1, DateTime period2);
        string SendGetRequest(string url);
    }
}
