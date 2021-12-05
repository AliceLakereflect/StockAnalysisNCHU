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
        const int GENERATIONS = 10000;
        const int SEARCH_NODE_NUMBER = 3;

        static void Main(string[] args)
        {
            var stockSymbol = new List<string> { "2603.TW"
                //, "2609.TW", "2615.TW", "0050.TW", "MU"
            };
            var chartDataList = new List<ChartData>();
            var myTransList = new List<StockTransList>();
            List<TestCase> testCase = new List<TestCase>  {
                new TestCase { Funds = FUNDS, BuyShortTermMa = 5, BuyLongTermMa = 20, SellShortTermMa = 5, SellLongTermMa = 60 },
            };

            // stock parameters
            var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
            var stockList = _dataService.GetPeriodDataFromYahooApi(SYMBOL, new DateTime(2018, 1, 1, 0, 0, 0), periodEnd.AddDays(1));
            ChartData data = _researchOperationService.GetMaFromYahoo(SYMBOL, stockList);
            chartDataList.Add(data);

            var iteration = GENERATIONS;
            var ramdom = new Random(343);
            var gBest = new StatusValue();
            var gWorst = new StatusValue();
            var localBest = new StatusValue();
            var localWorst = new StatusValue();

            // initialize nodes
            List<Particle> particles = new List<Particle>();
            for (var i = 0; i < SEARCH_NODE_NUMBER; i++){
                particles.Add(new Particle());
            }

            MetureX(ramdom, particles);
            var first = true;
            particles.ForEach((p) =>
            {
                p.CurrentFitness.Fitness = GetFitness(p.TestCase, stockList, data);
                if (first) gBest = p.CurrentFitness;
                else if (gBest.Fitness < p.CurrentFitness.Fitness) gBest = p.CurrentFitness;

                if (first) gWorst = p.CurrentFitness;
                else if (gWorst.Fitness > p.CurrentFitness.Fitness) gWorst = p.CurrentFitness;
                first = false;
            });

            // update probability
            GetLocalBestAndWorst(particles, ref localBest, ref localWorst);
            particles.ForEach((p) =>
            {
                for (var index = 0; index < 8; index++)
                {
                    UpdateProbability(p, localBest, localWorst, index);
                    index++;
                };
            });

            do
            {
                MetureX(ramdom, particles);
                particles.ForEach((p) =>
                {
                    p.CurrentFitness.Fitness = GetFitness(p.TestCase, stockList, data);
                    UpdateGBestAndGWorst(p, ref gBest, ref gWorst);
                });

                GetLocalBestAndWorst(particles, ref localBest, ref localWorst);
                // update probability
                particles.ForEach((p) =>
                {
                    for (var index = 0; index < 8; index++)
                    {
                        UpdateProbability(p, localBest, localWorst, index);
                        index++;
                    };
                });

                iteration--;
            } while (iteration != 0);

            Console.WriteLine(gBest.Fitness);
        }

        private static void UpdateGBestAndGWorst(Particle p, ref StatusValue gBest, ref StatusValue gWorst)
        {
            if (gBest.Fitness < p.CurrentFitness.Fitness) gBest = p.CurrentFitness;

            if (gWorst.Fitness > p.CurrentFitness.Fitness) gWorst = p.CurrentFitness;
        }

        private static void GetLocalBestAndWorst(List<Particle> particles, ref StatusValue localBest, ref StatusValue localWorst)
        {
            StatusValue max = particles.First().CurrentFitness;
            StatusValue min = particles.First().CurrentFitness;

            particles.ForEach((p) =>
            {
                if (p.CurrentFitness.Fitness > max.Fitness)
                {
                    max = p.CurrentFitness;
                }
                if (p.CurrentFitness.Fitness < min.Fitness)
                {
                    min = p.CurrentFitness;
                }
            });
            localBest = max;
            localWorst = min;
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
                    BuyShortTermMa = buyMa1,
                    BuyLongTermMa = buyMa2,
                    SellShortTermMa = sellMa1,
                    SellLongTermMa = sellMa2,
                };
            });
        }

        #region private method

        private static double GetFitness(
            TestCase currentTestCase,
            List<StockModel> stockList,
            ChartData data)
        {
            var transactions = _researchOperationService.GetMyTransactions(data, stockList, currentTestCase);
            var earns = _researchOperationService.GetEarningsResults(transactions);
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
