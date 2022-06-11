using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Hosting;
using Stock.Analysis._0607.Interface;
using Stock.Analysis._0607.Models;
using Stock.Analysis._0607.Service;

namespace Stock.Analysis._0607
{
    public class Worker : BackgroundService
    {
        private static IResearchOperationService _researchOperationService;
        private static IGNQTSAlgorithmService _qtsAlgorithmService;
        private static IFileHandler _fileHandler;
        private static IDataService _dataService;
        private static ISlidingWindowService _slidingWindowService;
        private static IOutputResultService _outputResultService;
        private static IMapper _mapper;
        private static IDataProvider<StockModel> _stockModelDataProvider;

        const string SYMBOL = "AAPL";
        const double FUNDS = 10000000;
        const int EXPERIMENT_NUMBER = 50;
        public Worker(
            IResearchOperationService researchOperationService,
            IGNQTSAlgorithmService qtsAlgorithmService,
            IFileHandler fileHandler,
            IDataService dataService,
            ISlidingWindowService slidingWindowService,
            IOutputResultService outputResultService,
            IDataProvider<StockModel> stockModelDataProvider,
            IMapper mapper
            )
        {
            _researchOperationService = researchOperationService ?? throw new ArgumentNullException(nameof(researchOperationService));
            _qtsAlgorithmService = qtsAlgorithmService ?? throw new ArgumentNullException(nameof(qtsAlgorithmService));
            _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _slidingWindowService = slidingWindowService ?? throw new ArgumentNullException(nameof(slidingWindowService));
            _stockModelDataProvider = stockModelDataProvider ?? throw new ArgumentNullException(nameof(stockModelDataProvider));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _outputResultService = outputResultService ?? throw new ArgumentNullException(nameof(outputResultService));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            PrepareSource(SYMBOL);
            MainJob();
            return Task.CompletedTask;
        }
        private void PrepareSource(string symbol) {
            var stockList = _dataService.GetStockDataFromDb(SYMBOL, new DateTime(2021, 12, 20, 0, 0, 0), new DateTime(2021, 12, 31, 0, 0, 0));
            if (stockList.Any()) return;

            var periodEnd = new DateTime(2022, 5, 31, 0, 0, 0);
            List<StockModel> maStockList;
            
            maStockList = _dataService.GetPeriodDataFromYahooApi(symbol, new DateTime(2010, 1, 1, 0, 0, 0), periodEnd);
            _researchOperationService.CalculateAllMa(ref maStockList);
            for (var year = 2010; year <= 2022; year++)
            {
                var end = new DateTime(year, 12, 31, 0, 0, 0);
                if (year == 2022) end = periodEnd.AddDays(1);
                maStockList.FindAll(s => s.Date > Utils.ConvertToUnixTimestamp(new DateTime(year, 1, 1, 0, 0, 0)) && s.Date < Utils.ConvertToUnixTimestamp(end));
                _stockModelDataProvider.AddBatch(maStockList);
            }
        }

        private void MainJob()
        {
            var chartDataList = new List<ChartData>();
            var myTransList = new List<StockTransList>();

            // stock parameters
            var allStart = new DateTime(2013, 1, 1, 0, 0, 0);
            var allEnd = new DateTime(2021, 12, 31, 0, 0, 0);
            var period = new Period { Start = allStart, End = allEnd };
            _qtsAlgorithmService.SetDelta(0.00016);
            var random = new Random(343);
            var cRandom = new Queue<int>();
            Console.WriteLine("Reading C Random.");
            cRandom = _fileHandler.Readcsv("Data/srand343");
            var slidingWindows = _slidingWindowService.GetSlidingWindows(period, PeriodEnum.M, PeriodEnum.M);
            Console.WriteLine("Start running sliding windows");
            List<TrainResult> trainResults = new List<TrainResult>();
            List<TrainBestTransaction> trainBestTransactions = new List<TrainBestTransaction>();
            slidingWindows.ForEach((window) =>
            {
                var periodStart = window.TrainPeriod.Start;
                var periodEnd = window.TrainPeriod.End;
                var copyCRandom = new Queue<int>(cRandom);
                var periodStartTimeStamp = Utils.ConvertToUnixTimestamp(periodStart);
                StatusValue bestGbest = Train(copyCRandom, random, window, periodStartTimeStamp, trainResults, trainBestTransactions);
                Test(bestGbest, window,periodStartTimeStamp, trainResults, trainBestTransactions);
            });
            _outputResultService.UpdateResultsInDb(trainResults, trainBestTransactions);
            cRandom = null;
        }

        #region private method

        private static void Test(StatusValue bestGbest, SlidingWindow window, double periodStartTimeStamp, List<TrainResult> trainResults, List<TrainBestTransaction> trainBestTransactions)
        {
            var stockList = _dataService.GetStockDataFromDb(SYMBOL, window.TestPeriod.Start, window.TestPeriod.End.AddDays(1));
            var stockListDto = _mapper.Map<List<StockModel>, List<StockModelDTO>>(stockList);
            var fileName = $"M2M/GNQTS Test - Crandom - {window.TestPeriod.Start.Year}-{window.TestPeriod.Start.Month}-{window.TestPeriod.Start.Day}";
            var trainResult = _outputResultService.OutputQTSResult(FUNDS, EXPERIMENT_NUMBER, fileName, SYMBOL, periodStartTimeStamp, stockListDto, bestGbest);
            var trainBestTransactionList = _outputResultService.OutputQTSBestTransaction(FUNDS, SYMBOL, periodStartTimeStamp, stockListDto, bestGbest, trainResult.Id);
            trainResults.Add(trainResult);
            trainBestTransactions.AddRange(trainBestTransactionList);
        }

        private static StatusValue Train(
            Queue<int> copyCRandom,
            Random random,
            SlidingWindow window,
            double periodStartTimeStamp,
            List<TrainResult> trainResults,
            List<TrainBestTransaction> trainBestTransactions
            )
        {
            // 用這邊在控制取fitness/transaction的日期區間
            // -7 是為了取得假日之前的前一日股票，後面再把period start丟進去確認起始時間正確
            var stockList = _dataService.GetStockDataFromDb(SYMBOL, window.TrainPeriod.Start.AddDays(-7), window.TrainPeriod.End.AddDays(1));
            var stockListDto = _mapper.Map<List<StockModel>, List<StockModelDTO>>(stockList);
            StatusValue bestGbest = new StatusValue();
            int gBestCount = 0;
            var periodStart = Utils.UnixTimeStampToDateTime(periodStartTimeStamp);

            for (var e = 0; e < EXPERIMENT_NUMBER; e++)
            {
                Console.WriteLine($"\nBegin:\t{periodStart.Year}/{periodStart.Month}\n");
                Stopwatch swFit = new Stopwatch();
                //var path = Path.Combine(Environment.CurrentDirectory, $"Output/50 Experements/debug G best transaction exp: {e} - C random.csv");
                StatusValue gBest;
                //using (var writer = new StreamWriter(path))
                //using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                //{
                    swFit.Start();
                    gBest = _qtsAlgorithmService.Fit(copyCRandom, random, FUNDS, stockListDto, e, periodStartTimeStamp, null);
                    swFit.Stop();
                //}
                Console.WriteLine($"{e}: {gBest.Fitness} => {swFit.Elapsed.Minutes}:{swFit.Elapsed.Seconds}:{swFit.Elapsed.Milliseconds}");
                swFit.Reset();
                CompareGBestByBits(ref bestGbest, ref gBestCount, gBest);

                //var fileName = $"50 Experements/G best transaction {e} - {periodStart.Year}-{periodStart.Month}-{periodStart.Day}";
                //_outputResultService.OutputQTSResult(FUNDS, EXPERIMENT_NUMBER, fileName, SYMBOL, periodStartTimeStamp, stockListDto, bestGbest, gBestCount);
            }

            if (bestGbest.BuyMa1.Count > 0)
            {
                var fileName = $"M2M/GNQTS Train - Crandom - {periodStart.Year}-{periodStart.Month}-{periodStart.Day}";
                var trainResult = _outputResultService.OutputQTSResult(FUNDS, EXPERIMENT_NUMBER, fileName, SYMBOL, periodStartTimeStamp, stockListDto, bestGbest, gBestCount);
                var trainBestTransactionList = _outputResultService.OutputQTSBestTransaction(FUNDS, SYMBOL, periodStartTimeStamp, stockListDto, bestGbest, trainResult.Id);
                trainResults.Add(trainResult);
                trainBestTransactions.AddRange(trainBestTransactionList);
            }

            return bestGbest;
        }

        private static void CompareGBestByBits(ref StatusValue bestGbest, ref int gBestCount, StatusValue gBest)
        {
            if (bestGbest.Fitness < gBest.Fitness)
            {
                bestGbest = gBest.DeepClone();
                gBestCount = 0;
            }

            if (
                Utils.GetMaNumber(bestGbest.BuyMa1) == Utils.GetMaNumber(gBest.BuyMa1) &&
                Utils.GetMaNumber(bestGbest.BuyMa2) == Utils.GetMaNumber(gBest.BuyMa2) &&
                Utils.GetMaNumber(bestGbest.SellMa1) == Utils.GetMaNumber(gBest.SellMa1) &&
                Utils.GetMaNumber(bestGbest.SellMa2) == Utils.GetMaNumber(gBest.SellMa2) &&
                bestGbest.Fitness == gBest.Fitness
                ) gBestCount++;
        }

        private static void CompareGBestByFitness(ref StatusValue bestGbest, ref int gBestCount, StatusValue gBest)
        {
            if (bestGbest.Fitness < gBest.Fitness)
            {
                bestGbest = gBest.DeepClone();
                gBestCount = 0;
            }

            if (bestGbest.Fitness == gBest.Fitness) gBestCount++;
        }

        #endregion

    }
}
