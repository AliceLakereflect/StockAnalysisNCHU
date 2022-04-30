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

        public void OutputCsv(List<StockModel> dataList, string fileName)
        {
            var _movingAvgService = new MovingAvarageService();
            var ma20 = _movingAvgService.CalculateMovingAvarage(dataList, 20).Select(s => s.Price).ToList();
            var path = Path.Combine(Environment.CurrentDirectory, $"Output/{fileName}.csv");
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Date");
                csv.WriteField("Price");
                csv.WriteField("20MA");
                csv.NextRecord();
                var index = 0;
                foreach (var data in dataList)
                {
                    var time = Utils.UnixTimeStampToDateTime(data.Date);
                    csv.WriteField($"{time.Year}-{time.Month}-{time.Day}");
                    csv.WriteField(data.Price);
                    csv.WriteField(ma20.ElementAt(index));
                    csv.NextRecord();
                    index++;
                }
            }
        }

        public void OutputCsv(List<ChartData> chartDataList, string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Data/{fileName}.csv");
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
            {
                foreach (var chartData in chartDataList)
                {
                    csv.WriteRecord(chartData);
                    csv.NextRecord();
                    chartData.Timestamp.ForEach(t => csv.WriteField(t));
                    csv.NextRecord();
                    chartData.Day.ForEach(d => csv.WriteField(d));
                    csv.NextRecord();
                    chartData.Price.ForEach(p => csv.WriteField(p));
                    csv.NextRecord();
                    foreach (var (maNum, maList) in chartData.MaList)
                    {
                        csv.WriteField(maNum);
                        maList.ForEach(ma => csv.WriteField(ma));
                        csv.NextRecord();
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

        public void OutputEarn(List<StockTransList> MyTransList, string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Output/{fileName}.csv");
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<StockTransList>();
                csv.NextRecord();
                foreach (var myTrans in MyTransList)
                {
                    csv.WriteRecord(myTrans);
                    csv.WriteField(myTrans.Transactions.Last().Balance);
                    csv.NextRecord();
                }
            }
        }

        public void OutputQTSResult(AlgorithmConst algorithmConst, double funds, StatusValue gBest, int gBestCount, List<StockTransaction> transactions, string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Output/{fileName}.csv");
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Algorithm Name");
                csv.WriteField(algorithmConst.Name);
                csv.NextRecord();
                csv.WriteField("Delta");
                csv.WriteField(algorithmConst.DELTA);
                csv.NextRecord();
                csv.WriteField("Experiment Number");
                csv.WriteField(algorithmConst.EXPERIMENT_NUMBER);
                csv.NextRecord();
                csv.WriteField("Generations");
                csv.WriteField(algorithmConst.GENERATIONS);
                csv.NextRecord();
                csv.WriteField("P Amount");
                csv.WriteField(algorithmConst.SEARCH_NODE_NUMBER);
                csv.NextRecord();

                csv.NextRecord();
                csv.WriteField("Initial Capital");
                csv.WriteField(funds);
                csv.NextRecord();
                csv.WriteField("Final Capital");
                csv.WriteField(gBest.Fitness);
                csv.NextRecord();
                csv.WriteField("Final Earn");
                csv.WriteField(gBest.Fitness - funds);
                csv.NextRecord();

                csv.NextRecord();
                csv.WriteField("Buy1");
                csv.WriteField(Utils.GetMaNumber(gBest.BuyMa1));
                csv.NextRecord();
                csv.WriteField("Buy2");
                csv.WriteField(Utils.GetMaNumber(gBest.BuyMa2));
                csv.NextRecord();
                csv.WriteField("Sell1");
                csv.WriteField(Utils.GetMaNumber(gBest.SellMa1));
                csv.NextRecord();
                csv.WriteField("Sell2");
                csv.WriteField(Utils.GetMaNumber(gBest.SellMa2));
                csv.NextRecord();
                csv.WriteField("Number of Trades");
                csv.WriteField((transactions.Count-1)/2);
                csv.NextRecord();
                csv.WriteField("Return Rates \n((Final Capital - Initial Capital) / Initial Capital * 100%)");
                csv.WriteField($"{(gBest.Fitness - funds) / funds * 100} %");
                csv.NextRecord();

                csv.NextRecord();
                csv.WriteField("The Experiment Number the Data Obtained in");
                csv.WriteField(gBest.Experiment);
                csv.NextRecord();
                csv.WriteField("The Generation Number the Data Obtained in");
                csv.WriteField(gBest.Generation);
                csv.NextRecord();
                csv.WriteField("Count");
                csv.WriteField(gBestCount);
                csv.NextRecord();

                csv.NextRecord();
                csv.WriteField("");
                csv.WriteField("Date");
                csv.WriteField("Price");
                csv.WriteField("The Ma1 Value 1 Day Ago");
                csv.WriteField("The Ma2 Value 1 Day Ago");
                csv.WriteField("The Current Ma1 Value");
                csv.WriteField("The Current Ma2 Value");
                csv.WriteField("Shares Helds");
                csv.WriteField("Remaining Capital");
                csv.WriteField("Total Assets");
                csv.NextRecord();
                foreach (var transaction in transactions)
                {
                    if (transaction.TransType == TransactionType.AddFunds) continue;
                    csv.WriteField(transaction.TransType);
                    csv.WriteField(transaction.TransTimeString);
                    csv.WriteField(transaction.TransPrice);
                    if(transaction.TransType == TransactionType.Buy)
                    {
                        csv.WriteField(transaction.BuyShortMaPrice1DayBefore);
                        csv.WriteField(transaction.BuyLongMaPrice1DayBefore);
                        csv.WriteField(transaction.BuyShortMaPrice);
                        csv.WriteField(transaction.BuyLongMaPrice);
                        csv.WriteField(transaction.TransVolume);
                        csv.WriteField(transaction.Balance);
                        csv.WriteField(transaction.TransVolume * transaction.TransPrice + transaction.Balance);
                    }
                    else if (transaction.TransType == TransactionType.Sell)
                    {
                        csv.WriteField(transaction.SellShortMaPrice1DayBefore);
                        csv.WriteField(transaction.SellShortMaPrice);
                        csv.WriteField(transaction.SellLongMaPrice1DayBefore);
                        csv.WriteField(transaction.SellLongMaPrice);
                        csv.WriteField(0);
                        csv.WriteField(transaction.Balance);
                        csv.WriteField(transaction.Balance);
                        csv.NextRecord();
                    }
                    
                    csv.NextRecord();
                }
                csv.NextRecord();
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
        public ChartData ReadMaByFile(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Data/{fileName}.csv");
            var chartData = new ChartData();
            try
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    chartData.Name = csv.GetField(0);
                    chartData.Min = double.Parse(csv.GetField(1));
                    chartData.Max = double.Parse(csv.GetField(2));
                    csv.Read();
                    for (int i = 0; csv.TryGetField<double>(i, out var t); i++)
                    {
                        chartData.Timestamp.Add(t);
                    }
                    csv.Read();
                    for (int i = 0; csv.TryGetField<string>(i, out var d); i++)
                    {
                        chartData.Day.Add(d);
                    }
                    csv.Read();

                    for (int i = 0; csv.TryGetField<double?>(i, out var p); i++)
                    {
                        chartData.Price.Add(p);
                    }
                    while (csv.Read())
                    {
                        var maNum = int.Parse(csv.GetField(0));
                        var maList = new List<double?>();
                        var first = true;
                        for (int i = 0; csv.TryGetField<double?>(i, out var ma); i++)
                        {
                            if(!first)
                                maList.Add(ma);

                            first = false;
                        }
                        chartData.MaList.Add(maNum, maList);
                    }
                }

            }
            catch (FileNotFoundException e)
            {
                return chartData;
            }
            return chartData;
        }
        public Queue<int> Readcsv(string fileName)
        {
            var result = new Queue<int>();
            var path = Path.Combine(Environment.CurrentDirectory, $"{fileName}.csv");
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                result.Enqueue(1158);
                while (csv.Read())
                {
                    var random = csv.GetRecord<int>();
                    result.Enqueue(random);
                }
            }
            return result;
        }
    }
}
