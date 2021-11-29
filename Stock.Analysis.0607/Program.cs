using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607
{
    class Program
    {

        private static IResearchOperationService _researchOperationService = new ResearchOperationService();
        private static IFileHandler _fileHandler = new FileHandler();
        private static IDataService _dataService = new DataService();
        const string SYMBOL = "2603.TW";
        const double FUNDS = 10000000;
        // QTS paremeters
        const double DELTA = 0.003;
        const int EXPERIMENT_NUMBER = 50;
        const int GENERATIONS = 500;
        const int SEARCH_NODE_NUMBER = 3;

        static void Main(string[] args)
        {
            var stockSymbol = new List<string> { "2603.TW"
                //, "2609.TW", "2615.TW", "0050.TW", "MU"
            };
            var chartDataList = new List<ChartData>();
            var myTransList = new List<StockTransList>();
            List<TestCase> testCase = new List<TestCase>  {
                new TestCase { Funds = FUNDS, BuyShortTermMa = 5, BuyLongTermMa = 20, SellShortTermMa = 5, SellLongTermMa = 60  },
            };

            // stock parameters
            var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
            var stockList = _dataService.GetPeriodDataFromYahooApi(SYMBOL, new DateTime(2020, 1, 1, 0, 0, 0), periodEnd.AddDays(1));
            ChartData data = _researchOperationService.GetMaFromYahoo(SYMBOL, stockList);
            chartDataList.Add(data);

            var iteration = GENERATIONS;
            var ramdom = new Random(114);
            var gBest = new StatusValue();
            var gWorst = new StatusValue();

            // initialize nodes
            List<Particle> particles = new List<Particle>();
            particles.Add(new Particle());
            particles.Add(new Particle());
            particles.Add(new Particle());

            MetureX(ramdom, particles);
            var first = true;
            particles.ForEach((p) =>
            {
                p.CurrentFitness.Fitness = GetFitness(SYMBOL, p.TestCase, myTransList, stockList, data);
                if (first) gBest = p.CurrentFitness;
                else if (gBest.Fitness < p.CurrentFitness.Fitness) gBest = p.CurrentFitness;

                if (first) gWorst = p.CurrentFitness;
                else if (gWorst.Fitness > p.CurrentFitness.Fitness) gWorst = p.CurrentFitness;
                first = false;
            });

            // update probability
            particles.ForEach((p) =>
            {
                for (var index = 0; index < 8; index++)
                {
                    UpdateProbability(p, gBest, gWorst, index);
                    index++;
                };
            });

            do
            {
                MetureX(ramdom, particles);
                particles.ForEach((p) =>
                {
                    p.CurrentFitness.Fitness = GetFitness(SYMBOL, p.TestCase, myTransList, stockList, data);
                    UpdateGBestAndGWorst(p, ref gBest, ref gWorst);
                });

                // update probability
                particles.ForEach((p) =>
                {
                    for (var index = 0; index < 8; index++)
                    {
                        UpdateProbability(p, gBest, gWorst, index);
                        index++;
                    };
                });

                iteration--;
            } while (iteration != 0);

            _fileHandler.OutputTransaction(myTransList, $"Transactions of {SYMBOL}");
        }

        private static void UpdateGBestAndGWorst(Particle p, ref StatusValue gBest, ref StatusValue gWorst)
        {
            if (gBest.Fitness < p.CurrentFitness.Fitness) gBest = p.CurrentFitness;

            if (gWorst.Fitness > p.CurrentFitness.Fitness) gWorst = p.CurrentFitness;
        }

        private static void UpdateProbability(Particle p, StatusValue gBest, StatusValue gWorst, int index)
        {
            // BuyMa1
            if (gBest.BuyMa2[index] > gWorst.BuyMa1[index])
            {
                p.BuyMa1Beta[index] += DELTA;
            }
            else if (gBest.BuyMa1[index] < gWorst.BuyMa1[index])
            {
                p.BuyMa1Beta[index] -= DELTA;
            }
            // BuyMa2
            if (gBest.BuyMa2[index] > gWorst.BuyMa2[index])
            {
                p.BuyMa2Beta[index] += DELTA;
            }
            else if (gBest.BuyMa2[index] < gWorst.BuyMa2[index])
            {
                p.BuyMa2Beta[index] -= DELTA;
            }
            // SellMa1
            if (gBest.SellMa1[index] > gWorst.SellMa1[index])
            {
                p.SellMa1Beta[index] += DELTA;
            }
            else if (gBest.SellMa1[index] < gWorst.SellMa1[index])
            {
                p.SellMa1Beta[index] -= DELTA;
            }
            // SellMa2
            if (gBest.SellMa2[index] > gWorst.SellMa2[index])
            {
                p.SellMa2Beta[index] += DELTA;
            }
            else if (gBest.SellMa2[index] < gWorst.SellMa2[index])
            {
                p.SellMa2Beta[index] -= DELTA;
            }
        }

        private static int GetMaNumber(List<int> metrix)
        {
            return metrix[0] * 1 + metrix[1] * 2 + metrix[2] * 4 + metrix[3] * 8 + metrix[4] * 16 + metrix[5] * 32 + metrix[6] * 64 + metrix[7] * 127;
        }

        private static void MetureX(Random ramdom, List<Particle> particles)
        {
            particles.ForEach((p) =>
            {
                p.CurrentFitness.BuyMa1 = new List<int>();
                p.BuyMa1Beta.ForEach((x) =>
                {
                    p.CurrentFitness.BuyMa1.Add(x >= ramdom.NextDouble() ? 1 : 0);
                });
                p.CurrentFitness.BuyMa2 = new List<int>();
                p.BuyMa2Beta.ForEach((x) =>
                {
                    p.CurrentFitness.BuyMa2.Add(x >= ramdom.NextDouble() ? 1 : 0);
                });
                p.CurrentFitness.SellMa1 = new List<int>();
                p.SellMa1Beta.ForEach((x) =>
                {
                    p.CurrentFitness.SellMa1.Add(x >= ramdom.NextDouble() ? 1 : 0);
                });
                p.CurrentFitness.SellMa2 = new List<int>();
                p.SellMa2Beta.ForEach((x) =>
                {
                    p.CurrentFitness.SellMa2.Add(x >= ramdom.NextDouble() ? 1 : 0);
                });

                var buyMa1 = GetMaNumber(p.CurrentFitness.BuyMa1);
                var buyMa2 = GetMaNumber(p.CurrentFitness.BuyMa2);
                var sellMa1 = GetMaNumber(p.CurrentFitness.SellMa1);
                var sellMa2 = GetMaNumber(p.CurrentFitness.SellMa2);
                p.TestCase = new TestCase
                {
                    Funds = FUNDS,
                    BuyShortTermMa = buyMa1 < buyMa2 ? buyMa1 : buyMa2,
                    BuyLongTermMa = buyMa1 < buyMa2 ? buyMa2 : buyMa1,
                    SellShortTermMa = sellMa1 < sellMa2 ? sellMa1 : sellMa2,
                    SellLongTermMa = sellMa1 < sellMa2 ? sellMa2 : sellMa1,
                };
            });
        }

        #region private method

        private static double GetFitness(
            string symbol,
            TestCase currentTestCase,
            List<StockTransList> myTransList,
            List<StockModel> stockList,
            ChartData data)
        {
            var transactions = _researchOperationService.GetMyTransactions(data, stockList, currentTestCase);
            var earns = _researchOperationService.GetEarningsResults(transactions);
            myTransList.Add(new StockTransList { Name = symbol, TestCase = currentTestCase, Transactions = transactions });
            return earns;
        }

        private static void PrintTransactions(
            string symbol,
            TestCase currentTestCase,
            List<StockModel> stockList,
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
            transString += $"|\tName: {symbol}\t Test Case: {currentTestCase.BuyShortTermMa}MA vs {currentTestCase.BuyLongTermMa}MA \t Current Stock:{stockList.Last().Price}\n";
            myTrans.ForEach(t =>
            {
                transString += $"|\t TransType: {t.TransType}\n|\t TransTime: {t.TransTimeString}\t TransVolume:{t.TransVolume}\t Fees:{t.Fees}\t " +
                $"Tax: {t.Tax}\t Balance: {t.Balance}\t TransPrice: {t.TransPrice}\n";
            });
            transString += "=========================================================================================================\n";
            Console.WriteLine(transString);
        }

        #endregion
    }

    class Particle
    {
        public StatusValue CurrentFitness { get; set; } = new StatusValue();
        public List<double> BuyMa1Beta { get; set; } = new List<double> { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };
        public List<double> BuyMa2Beta { get; set; } = new List<double> { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };
        public List<double> SellMa1Beta { get; set; } = new List<double> { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };
        public List<double> SellMa2Beta { get; set; } = new List<double> { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };

        public TestCase TestCase { get; set; }

        //public XValue BestFitness { get; set; } = new XValue();
        //public XValue WorstFitness { get; set; } = new XValue();
    }

    class StatusValue
    {
        public List<int> BuyMa1 { get; set; } = new List<int>();
        public List<int> BuyMa2 { get; set; } = new List<int>();
        public List<int> SellMa1 { get; set; } = new List<int>();
        public List<int> SellMa2 { get; set; } = new List<int>();
        public double Fitness { get; set; } = 0;
    }
}
