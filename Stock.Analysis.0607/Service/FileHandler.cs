using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Newtonsoft.Json;

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
    }
}
