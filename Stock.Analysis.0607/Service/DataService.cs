using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class DataService: IDataService
    {
        public DataService()
        {
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        private static void ParseResult(List<StockModel> result, YahooFinanceChartDataModel data)
        {
            var timestamps = data?.chart?.result?.First()?.timestamp;
            var closeStockValue = data?.chart?.result?.First()?.indicators?.quote?.First()?["close"];
            var index = 0;
            timestamps?.ForEach(timestamp =>
            {
                if (closeStockValue.Any())
                {
                    result.Add(new StockModel
                    {
                        Date = timestamp,
                        Price = closeStockValue.ElementAt(index)
                    });
                }
                index++;
            });
        }

        public List<StockModel> GetPeriodDataFromYahooApi(string stockSymbol, DateTime period1, DateTime period2)
        {
            var result = new List<StockModel>();
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timestamp1 = ConvertToUnixTimestamp(period1);
            var timestamp2 = ConvertToUnixTimestamp(period2);
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{stockSymbol}?period1={timestamp1}&period2={timestamp2}&interval=1d";
            var data = JsonConvert.DeserializeObject<YahooFinanceChartDataModel>(SendGetRequest(url));
            ParseResult(result, data);

            return result;
        }

        public List<StockModel> Get1YDataFromYahooApi(string stockSymbol)
        {
            var result = new List<StockModel>();
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{stockSymbol}?range=1y&interval=1d";
            var data = JsonConvert.DeserializeObject<YahooFinanceChartDataModel>(SendGetRequest(url));
            ParseResult(result, data);

            return result;
        }

        

        public List<StockModel> Get1dDataFromYahooApi(string stockSymbol)
        {
            var result = new List<StockModel>();
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{stockSymbol}";
            var data = JsonConvert.DeserializeObject<YahooFinanceChartDataModel>(SendGetRequest(url));
            ParseResult(result, data);

            return result;
        }

        public string SendGetRequest(string url)
        {
            var responseFromServer = string.Empty;
            // Create a request for the URL.
            WebRequest request = WebRequest.Create(url);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";


            // Get the response.
            WebResponse response = request.GetResponse();

            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
            }

            // Close the response.
            response.Close();

            return responseFromServer;
        }
    }

    public interface IDataService
    {
        List<StockModel> Get1YDataFromYahooApi(string stockSymbol);
        List<StockModel> Get1dDataFromYahooApi(string stockSymbol);
        List<StockModel> GetPeriodDataFromYahooApi(string stockSymbol, DateTime period1, DateTime period2);
        string SendGetRequest(string url);
    }
}
