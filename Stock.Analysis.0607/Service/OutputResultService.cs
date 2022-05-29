using System;
using System.Collections.Generic;
using Stock.Analysis._0607.Interface;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class OutputResultService : IOutputResultService
    {
        private readonly IResearchOperationService _researchOperationService;
        private readonly IGNQTSAlgorithmService _qtsAlgorithmService;
        private readonly IDataProvider<TrainResult> _trainResultProvider;
        private readonly IDataProvider<TrainBestTransaction> _trainBestTransactionProvider;
        public OutputResultService(
            IResearchOperationService researchOperationService,
            IGNQTSAlgorithmService qtsAlgorithmService,
            IDataProvider<TrainResult> trainResultProvider,
            IDataProvider<TrainBestTransaction> trainBestTransactionProvider
            )
        {
            _researchOperationService = researchOperationService ?? throw new ArgumentNullException(nameof(researchOperationService));
            _qtsAlgorithmService = qtsAlgorithmService ?? throw new ArgumentNullException(nameof(qtsAlgorithmService));
            _trainResultProvider = trainResultProvider ?? throw new ArgumentNullException(nameof(trainResultProvider));
            _trainBestTransactionProvider = trainBestTransactionProvider ?? throw new ArgumentNullException(nameof(trainBestTransactionProvider));
        }

        public void OutputQTSResult(double funds, int experimentNumber, string filename, string stockSymbol, double periodStartTimeStamp, List<StockModelDTO> stockList, StatusValue bestGbest, int gBestCount = 0)
        {
            var algorithmConst = _qtsAlgorithmService.GetConst();

            var trainResult = new TrainResult {
                Id = new Guid(),
                FileName = filename,
                StockName = stockSymbol,
                AlgorithmName = algorithmConst.Name,
                Delta = algorithmConst.DELTA,
                ExperimentNumber = experimentNumber,
                Generations = algorithmConst.GENERATIONS,
                SearchNodeNumber = algorithmConst.SEARCH_NODE_NUMBER,
                InitialCapital = funds,
                FinalCapital = bestGbest.Fitness,
                FinalEarn = bestGbest.Fitness - funds,
                Buy1 = Utils.GetMaNumber(bestGbest.BuyMa1),
                Buy2 = Utils.GetMaNumber(bestGbest.BuyMa2),
                Sell1 = Utils.GetMaNumber(bestGbest.SellMa1),
                Sell2 = Utils.GetMaNumber(bestGbest.SellMa2),
                ReturnRates = (bestGbest.Fitness - funds) / funds * 100,
                ExperimentNumberOfBest = bestGbest.Experiment,
                GenerationOfBest = bestGbest.Generation,
                BestCount = gBestCount
            };

            _trainResultProvider.Add(trainResult);

            if (bestGbest.Fitness == funds) return;

            var trainBestTrainsactions = new List<TrainBestTransaction>();
            var testCase = new TestCase
            {
                Funds = funds,
                BuyShortTermMa = Utils.GetMaNumber(bestGbest.BuyMa1),
                BuyLongTermMa = Utils.GetMaNumber(bestGbest.BuyMa2),
                SellShortTermMa = Utils.GetMaNumber(bestGbest.SellMa1),
                SellLongTermMa = Utils.GetMaNumber(bestGbest.SellMa2)
            };
            var transactions = _researchOperationService.GetMyTransactions(stockList, testCase, periodStartTimeStamp);
            foreach (var transaction in transactions)
            {
                
                if (transaction.TransType == TransactionType.AddFunds) continue;
                var trainBestTrainsaction = new TrainBestTransaction
                {
                    TrainResultId = trainResult.Id,
                    StockName = stockSymbol,
                    TransType = transaction.TransType,
                    Date = transaction.TransTime,
                    Price = transaction.TransPrice,
                };

                if (transaction.TransType == TransactionType.Buy)
                {
                    trainBestTrainsaction.ShortMaPrice1DayBefore = transaction.BuyShortMaPrice1DayBefore;
                    trainBestTrainsaction.LongMaPrice1DayBefore = transaction.BuyLongMaPrice1DayBefore;
                    trainBestTrainsaction.ShortMaPrice = transaction.BuyShortMaPrice;
                    trainBestTrainsaction.LongMaPrice = transaction.BuyLongMaPrice;
                    trainBestTrainsaction.SharesHelds = transaction.TransVolume;
                    trainBestTrainsaction.RemainingCapital = transaction.Balance;
                    trainBestTrainsaction.TotalAssets = transaction.TransVolume * transaction.TransPrice + transaction.Balance;
                }
                else if (transaction.TransType == TransactionType.Sell)
                {
                    trainBestTrainsaction.ShortMaPrice1DayBefore = transaction.SellShortMaPrice1DayBefore;
                    trainBestTrainsaction.LongMaPrice1DayBefore = transaction.SellLongMaPrice1DayBefore;
                    trainBestTrainsaction.ShortMaPrice = transaction.SellShortMaPrice;
                    trainBestTrainsaction.LongMaPrice = transaction.SellLongMaPrice;
                    trainBestTrainsaction.SharesHelds = 0;
                    trainBestTrainsaction.RemainingCapital = transaction.Balance;
                    trainBestTrainsaction.TotalAssets = transaction.Balance;
                }

                trainBestTrainsactions.Add(trainBestTrainsaction);
            }

            _trainBestTransactionProvider.AddBatch(trainBestTrainsactions);
        }

        public void PrintTransactions(
            string symbol,
            TestCase currentTestCase,
            List<StockTransaction> myTrans,
            TestCase ma,
            double? currentStock)
        {
            Console.WriteLine($"{symbol}\t Current Stock: {currentStock}");
            var earn = _researchOperationService.GetEarningsResults(myTrans);
            var resultString = $"When ma = Buy: {ma.BuyShortTermMa} vs {ma.BuyLongTermMa}; Sell: {ma.SellShortTermMa} vs {ma.SellLongTermMa};,\tEarned: {earn}\t";
            if (!resultString.Contains("Earned: 0"))
            {
                Console.WriteLine(resultString);
            }
            var transString = "=== My Transaction ======================================================================================\n";
            transString += $"|\tName: {symbol}\t Test Case: {currentTestCase.BuyShortTermMa}MA vs {currentTestCase.BuyLongTermMa}MA \t Current Stock:{currentStock}\n";
            myTrans.ForEach(t =>
            {
                transString += $"|\t TransType: {t.TransType}\n|\t TransTime: {t.TransTimeString}\t TransVolume:{t.TransVolume}\t Fees:{t.Fees}\t " +
                $"Tax: {t.Tax}\t Balance: {t.Balance}\t TransPrice: {t.TransPrice}\n";
            });
            transString += "=========================================================================================================\n";
            Console.WriteLine(transString);
        }
    }

    public interface IOutputResultService
    {
        void OutputQTSResult(double funds, int experimentNumber, string filename, string stockSymbol, double periodStartTimeStamp, List<StockModelDTO> stockList, StatusValue bestGbest, int gBestCount = 0);
        void PrintTransactions(string symbol, TestCase currentTestCase, List<StockTransaction> myTrans, TestCase ma, double? currentStock);
    }
}
