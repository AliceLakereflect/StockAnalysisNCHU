using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class QTSAlgorithmService: IAlgorithmService
    {
        private static IResearchOperationService _researchOperationService = new ResearchOperationService();
        // QTS paremeters
        const double DELTA = 0.003;
        const int GENERATIONS = 10000;
        const int SEARCH_NODE_NUMBER = 3;
        const int DIGIT_NUMBER = 8;
        const double RANDOM_MAX = 32767.0;

        public QTSAlgorithmService()
        {
            
        }

        public AlgorithmConst GetConst()
        {
            return new AlgorithmConst
            {
                Name = "QTS",
                DELTA = DELTA,
                GENERATIONS = GENERATIONS,
                SEARCH_NODE_NUMBER = SEARCH_NODE_NUMBER
            };
        }

        public StatusValue Fit(Queue<int> random, double funds, List<StockModel> stockList, ChartData data, int experiment)
        {
            var iteration = 0;
            
            var gBest = new StatusValue();
            var gWorst = new StatusValue();
            var localBest = new StatusValue();
            var localWorst = new StatusValue();

            // initialize nodes
            List<Particle> particles = new List<Particle>();
            for (var i = 0; i < SEARCH_NODE_NUMBER; i++)
            {
                particles.Add(new Particle());
            }

            MetureX(random, particles, funds);
            var first = true;
            particles.ForEach((p) =>
            {
                p.CurrentFitness.Fitness = GetFitness(p.TestCase, stockList, data);
                if (first)
                {
                    gBest = p.CurrentFitness;
                    gBest.Experiment = experiment;
                    gBest.Generation = iteration;
                    gWorst = p.CurrentFitness;
                    gBest.Experiment = experiment;
                    gBest.Generation = iteration;
                }
                else
                {
                    UpdateGBestAndGWorst(p, ref gBest, ref gWorst, experiment, iteration);
                }
                first = false;
            });

            // update probability
            GetLocalBestAndWorst(particles, ref localBest, ref localWorst);
            particles.ForEach((p) =>
            {
                UpdateProbability(p, localBest, localWorst);
            });

            while (iteration <= GENERATIONS)
            {
                MetureX(random, particles, funds);
                particles.ForEach((p) =>
                {
                    p.CurrentFitness.Fitness = GetFitness(p.TestCase, stockList, data);
                    UpdateGBestAndGWorst(p, ref gBest, ref gWorst, experiment, iteration);
                });

                GetLocalBestAndWorst(particles, ref localBest, ref localWorst);
                // update probability
                particles.ForEach((p) =>
                {
                    UpdateProbability(p, localBest, localWorst);
                });
                //Console.WriteLine($"Iteration: {iteration} " +
                //    $"gbest: earn - {gBest.Fitness}, buy1 - {GetMaNumber(gBest.BuyMa1)}, buy2 - {GetMaNumber(gBest.BuyMa2)}," +
                //    $" sell1 - {GetMaNumber(gBest.SellMa1)}, sell2 - {GetMaNumber(gBest.SellMa2)}");
                Console.WriteLine($"{iteration} - gbest:{gBest.Fitness} " +
                    $"buyMa1: {GetMaNumber(gBest.BuyMa1)} " +
                    $"buyMa2: {GetMaNumber(gBest.BuyMa2)} " +
                    $"sellMa1: {GetMaNumber(gBest.SellMa1)} " +
                    $"sellMa2:{GetMaNumber(gBest.SellMa2)}");
                iteration++;
            }
            
            return gBest;
        }

        public void UpdateGBestAndGWorst(Particle p, ref StatusValue gBest, ref StatusValue gWorst, int experiment, int iteration)
        {
            if (gBest.Fitness < p.CurrentFitness.Fitness)
            {
                gBest = p.CurrentFitness.DeepClone();
                gBest.Experiment = experiment;
                gBest.Generation = iteration;
            }

            if (gWorst.Fitness > p.CurrentFitness.Fitness)
            {
                gWorst = p.CurrentFitness.DeepClone();
                gWorst.Experiment = experiment;
                gWorst.Generation = iteration;
            }
        }

        public void GetLocalBestAndWorst(List<Particle> particles, ref StatusValue localBest, ref StatusValue localWorst)
        {
            StatusValue max = particles.First().CurrentFitness;
            StatusValue min = particles.First().CurrentFitness;

            particles.ForEach((p) =>
            {
                if (p.CurrentFitness.Fitness > max.Fitness)
                {
                    max = p.CurrentFitness.DeepClone();
                }
                if (p.CurrentFitness.Fitness < min.Fitness)
                {
                    min = p.CurrentFitness.DeepClone();
                }
            });
            localBest = max;
            localWorst = min;
        }

        public void UpdateProbability(Particle p, StatusValue localBest, StatusValue localWorst)
        {
            if (localBest.Fitness == 0) return;
            for (var index = 0; index < DIGIT_NUMBER; index++)
            {
                // BuyMa1
                if (localBest.BuyMa2[index] > localWorst.BuyMa1[index])
                {
                    p.BuyMa1Beta[index] += DELTA;
                }
                else if (localBest.BuyMa1[index] < localWorst.BuyMa1[index])
                {
                    p.BuyMa1Beta[index] -= DELTA;
                }
                // BuyMa2
                if (localBest.BuyMa2[index] > localWorst.BuyMa2[index])
                {
                    p.BuyMa2Beta[index] += DELTA;
                }
                else if (localBest.BuyMa2[index] < localWorst.BuyMa2[index])
                {
                    p.BuyMa2Beta[index] -= DELTA;
                }
                // SellMa1
                if (localBest.SellMa1[index] > localWorst.SellMa1[index])
                {
                    p.SellMa1Beta[index] += DELTA;
                }
                else if (localBest.SellMa1[index] < localWorst.SellMa1[index])
                {
                    p.SellMa1Beta[index] -= DELTA;
                }
                // SellMa2
                if (localBest.SellMa2[index] > localWorst.SellMa2[index])
                {
                    p.SellMa2Beta[index] += DELTA;
                }
                else if (localBest.SellMa2[index] < localWorst.SellMa2[index])
                {
                    p.SellMa2Beta[index] -= DELTA;
                }
            }
        }

        public int GetMaNumber(List<int> metrix)
        {
            return metrix[0] * 1 + metrix[1] * 2 + metrix[2] * 4 + metrix[3] * 8 + metrix[4] * 16 + metrix[5] * 32 + metrix[6] * 64 + metrix[7] * 127;
        }

        public void MetureX(Queue<int> random, List<Particle> particles, double funds)
        {
            particles.ForEach((p) =>
            {
                p.CurrentFitness.BuyMa1 = new List<int>();
                p.BuyMa1Beta.ForEach((x) =>
                {
                    p.CurrentFitness.BuyMa1.Add(x >= random.Dequeue() / RANDOM_MAX ? 1 : 0);
                });
                p.CurrentFitness.BuyMa2 = new List<int>();
                p.BuyMa2Beta.ForEach((x) =>
                {
                    p.CurrentFitness.BuyMa2.Add(x >= random.Dequeue() / RANDOM_MAX ? 1 : 0);
                });
                p.CurrentFitness.SellMa1 = new List<int>();
                p.SellMa1Beta.ForEach((x) =>
                {
                    p.CurrentFitness.SellMa1.Add(x >= random.Dequeue() / RANDOM_MAX ? 1 : 0);
                });
                p.CurrentFitness.SellMa2 = new List<int>();
                p.SellMa2Beta.ForEach((x) =>
                {
                    p.CurrentFitness.SellMa2.Add(x >= random.Dequeue() / RANDOM_MAX ? 1 : 0);
                });

                var buyMa1 = GetMaNumber(p.CurrentFitness.BuyMa1);
                var buyMa2 = GetMaNumber(p.CurrentFitness.BuyMa2);
                var sellMa1 = GetMaNumber(p.CurrentFitness.SellMa1);
                var sellMa2 = GetMaNumber(p.CurrentFitness.SellMa2);
                p.TestCase = new TestCase
                {
                    Funds = funds,
                    BuyShortTermMa = buyMa1,
                    BuyLongTermMa = buyMa2,
                    SellShortTermMa = sellMa1,
                    SellLongTermMa = sellMa2,
                };
            });
        }

        public double GetFitness(TestCase currentTestCase, List<StockModel> stockList, ChartData data)
        {
            var transactions = _researchOperationService.GetMyTransactions(data, stockList, currentTestCase);
            var earns = _researchOperationService.GetEarningsResults(transactions);
            return earns;
        }
    }
    public interface IAlgorithmService
    { 
        StatusValue Fit(Queue<int> random, double funds, List<StockModel> stockList, ChartData data, int experiment);
        void UpdateGBestAndGWorst(Particle p, ref StatusValue gBest, ref StatusValue gWorst, int experiment, int iteration);
        void GetLocalBestAndWorst(List<Particle> particles, ref StatusValue localBest, ref StatusValue localWorst);
        void UpdateProbability(Particle p, StatusValue localBest, StatusValue localWorst);
        int GetMaNumber(List<int> metrix);
        void MetureX(Queue<int> random, List<Particle> particles, double funds);
        double GetFitness(TestCase currentTestCase, List<StockModel> stockList, ChartData data);
        AlgorithmConst GetConst();
    }
}
