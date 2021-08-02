using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Newtonsoft.Json;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class FileHandler: IFileHandler
    {
        public FileHandler()
        {
        }

        public void OutputResult<T>(List<T> chartDataList, string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Output/{fileName}.json");
            var jsonOutput = JsonConvert.SerializeObject(chartDataList);

            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.WriteAllText(file.FullName, jsonOutput);
        }

        public void OutputCsv(List<ChartData> chartDataList, string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Output/{fileName}.csv");
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var chartData in chartDataList)
                {
                    csv.WriteField("Stock Name");
                    csv.NextRecord();
                    csv.WriteField(chartData.Name);
                    csv.NextRecord();

                    var index = 0;
                    csv.WriteField("Date");
                    csv.WriteField("Price");
                    csv.WriteField("5MA");
                    csv.WriteField("10MA");
                    csv.WriteField("20MA");
                    csv.WriteField("60MA");
                    csv.NextRecord();
                    foreach (var data in chartData.Timestamp) {
                        csv.WriteField(chartData.Day[index]);
                        csv.WriteField(chartData.Price.ElementAt(index));
                        csv.WriteField(chartData.PriceAvg5Days.ElementAt(index));
                        csv.WriteField(chartData.PriceAvg10Days.ElementAt(index));
                        csv.WriteField(chartData.PriceAvg20Days.ElementAt(index));
                        csv.WriteField(chartData.PriceAvg60Days.ElementAt(index));
                        csv.NextRecord();
                        index++;
                    }
                    csv.NextRecord();
                }
            }
        }

        public void OutputTransaction(List<StockTransList> MyTransList, string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Output/{fileName}.csv");
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var myTrans in MyTransList)
                {
                    csv.WriteHeader<StockTransList>();
                    csv.NextRecord();
                    csv.WriteRecord(myTrans);
                    csv.NextRecord();
                    csv.WriteHeader<StockTransaction>();
                    csv.NextRecord();
                    foreach (var transaction in myTrans.Transactions)
                    {
                        csv.WriteRecord(transaction);
                        csv.NextRecord();
                    }
                    csv.NextRecord();
                }
            }
        }
        public List<List<StockModel>> ReadDataFromFile(string path)
        {
            Console.WriteLine($"Getting data from {path}");
            var stockList1 = new List<StockModel>();
            var stockList2 = new List<StockModel>();
            var stockList3 = new List<StockModel>();
            var stockList4 = new List<StockModel>();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                while (csv.Read())
                {
                    if (string.IsNullOrEmpty(csv.GetField(0)) || csv.GetField(0) == "???")
                    {
                        continue;
                    }
                    DateTime.TryParse(csv.GetField(0), out var datetime);
                    double.TryParse(csv.GetField(1), out var stock1);
                    stockList1.Add(new StockModel
                    {
                        Date = datetime.Ticks,
                        Price = stock1
                    });

                    double.TryParse(csv.GetField(2), out var stock2);
                    stockList2.Add(new StockModel
                    {
                        Date = datetime.Ticks,
                        Price = stock2
                    });

                    double.TryParse(csv.GetField(3), out var stock3);
                    stockList3.Add(new StockModel
                    {
                        Date = datetime.Ticks,
                        Price = stock3
                    });

                    double.TryParse(csv.GetField(4), out var stock4);
                    stockList4.Add(new StockModel
                    {
                        Date = datetime.Ticks,
                        Price = stock4
                    });
                }
            }
            var result = new List<List<StockModel>>();
            result.Add(stockList1);
            result.Add(stockList2);
            result.Add(stockList3);
            result.Add(stockList4);

            return result;
        }

        private static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

    }

    public interface IFileHandler
    {
        List<List<StockModel>> ReadDataFromFile(string path);
        void OutputResult<T>(List<T> chartDataList, string fileName);
        void OutputCsv(List<ChartData> chartDataList, string fileName);
        void OutputTransaction(List<StockTransList> MyTransList, string fileName);
    }
}
