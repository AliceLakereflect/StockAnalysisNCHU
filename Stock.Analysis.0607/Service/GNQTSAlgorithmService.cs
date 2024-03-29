﻿using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class GNQTSAlgorithmService: IGNQTSAlgorithmService
    {
        private static IResearchOperationService _researchOperationService = new ResearchOperationService();
        // GNQTS paremeters
        const double DELTA = 0.003;
        const int GENERATIONS = 10000;
        const int SEARCH_NODE_NUMBER = 10;
        const int DIGIT_NUMBER = 8;
        const double RANDOM_MAX = 32767.0;

        public GNQTSAlgorithmService()
        {
            
        }

        public AlgorithmConst GetConst()
        {
            return new AlgorithmConst
            {
                Name = "GNQTS",
                DELTA = DELTA,
                GENERATIONS = GENERATIONS,
                SEARCH_NODE_NUMBER = SEARCH_NODE_NUMBER
            };
        }

        public StatusValue Fit(Queue<int> random, double funds, List<StockModel> stockList, ChartData data, int experiment, CsvWriter csv, DateTime periodStart)
        {
            var iteration = 0;
            
            var gBest = new StatusValue();
            var gWorst = new StatusValue();
            var localBest = new StatusValue();
            var localWorst = new StatusValue();
            #region debug

            csv.WriteField($"exp:{experiment}");
            csv.NextRecord();
            csv.WriteField($"gen:{iteration}");
            csv.NextRecord();

            #endregion
            // initialize nodes
            List<Particle> particles = new List<Particle>();
            for (var i = 0; i < SEARCH_NODE_NUMBER; i++)
            {
                particles.Add(new Particle());
            }

            MetureX(random, particles, funds);

            var first = true;
            var index = 0;
            particles.ForEach((p) =>
            {
                
                p.CurrentFitness.Fitness = GetFitness(p.TestCase, stockList, data, periodStart);
                #region debug

                csv.WriteField($"P{index}");
                DebugPrintParticle(csv, p.CurrentFitness);
                csv.WriteField($"{p.CurrentFitness.Fitness / funds * 100}% ({p.CurrentFitness.Fitness}/{funds})");
                csv.NextRecord();
                index++;

                #endregion
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
            #region debug

            csv.WriteField("global best");
            DebugPrintParticle(csv, gBest);
            csv.NextRecord();
            csv.WriteField("local worst");
            DebugPrintParticle(csv, localWorst);
            csv.NextRecord();

            #endregion
            particles.ForEach((p) =>
            {
                UpdateProByGN(p, gBest, localWorst);
            });
            particles.ForEach((p) =>
            {
                UpdateProbability(p, gBest, localWorst);
            });
            
            #region debug

            csv.WriteField("beta matrix");
            DebugPrintBetaMatrix(csv, particles.FirstOrDefault());
            csv.NextRecord();

            #endregion

            while (iteration < GENERATIONS - 1)
            {
                iteration++;
                #region debug

                csv.WriteField($"gen:{iteration}");
                csv.NextRecord();

                #endregion
                MetureX(random, particles, funds);
                index = 0;
                particles.ForEach((p) =>
                {
                    
                    p.CurrentFitness.Fitness = GetFitness(p.TestCase, stockList, data, periodStart);
                    UpdateGBestAndGWorst(p, ref gBest, ref gWorst, experiment, iteration);
                    #region debug

                    csv.WriteField($"P{index}");
                    DebugPrintParticle(csv, p.CurrentFitness);
                    csv.WriteField($"{p.CurrentFitness.Fitness/funds*100}% ({p.CurrentFitness.Fitness}/{funds})");
                    csv.NextRecord();
                    index++;

                    #endregion
                });

                GetLocalBestAndWorst(particles, ref localBest, ref localWorst);
                #region debug

                csv.WriteField("global best");
                DebugPrintParticle(csv, gBest);
                csv.NextRecord();
                csv.WriteField("local worst");
                DebugPrintParticle(csv, localWorst);
                csv.NextRecord();

                #endregion
                // update probability

                particles.ForEach((p) =>
                {
                    UpdateProByGN(p, gBest, localWorst);
                });
                particles.ForEach((p) =>
                {
                    UpdateProbability(p, gBest, localWorst);
                    
                });

                
                #region debug

                csv.WriteField("beta matrix");
                DebugPrintBetaMatrix(csv, particles.FirstOrDefault());
                csv.NextRecord();

                #endregion

                //Console.WriteLine($"{iteration} - gbest:{gBest.Fitness} " +
                //    $"buyMa1: {GetMaNumber(gBest.BuyMa1)} " +
                //    $"buyMa2: {GetMaNumber(gBest.BuyMa2)} " +
                //    $"sellMa1: {GetMaNumber(gBest.SellMa1)} " +
                //    $"sellMa2:{GetMaNumber(gBest.SellMa2)}");


                
            }
            
            return gBest;
        }

        private static void DebugPrintBetaMatrix(CsvWriter csv, Particle p)
        {
            var str = string.Empty;
            p.BuyMa1Beta.ForEach(digit =>
            {
                str += $"{digit},";
            });
            csv.WriteField(str);
            csv.WriteField("");

            str = string.Empty;
            p.BuyMa2Beta.ForEach(digit =>
            {
                str += $"{digit},";
            });
            csv.WriteField(str);
            csv.WriteField("");

            str = string.Empty;
            p.SellMa1Beta.ForEach(digit =>
            {
                str += $"{digit},";
            });
            csv.WriteField(str);
            csv.WriteField("");

            str = string.Empty;
            p.SellMa2Beta.ForEach(digit =>
            {
                str += $"{digit},";
            });
            csv.WriteField(str);
            csv.WriteField("");
        }

        private void DebugPrintParticle(CsvWriter csv, StatusValue current)
        {
            var str = string.Empty;
            current.BuyMa1.ForEach(digit =>
            {
                str += $"{digit}";
            });
            csv.WriteField($"{GetMaNumber(current.BuyMa1)} ({str})");
            csv.WriteField("");

            str = string.Empty;
            current.BuyMa2.ForEach(digit =>
            {
                str += $"{digit}";
            });
            csv.WriteField($"{GetMaNumber(current.BuyMa2)} ({str})");
            csv.WriteField("");

            str = string.Empty;
            current.SellMa1.ForEach(digit =>
            {
                str += $"{digit}";
            });
            csv.WriteField($"{GetMaNumber(current.SellMa1)} ({str})");
            csv.WriteField("");

            str = string.Empty;
            current.SellMa2.ForEach(digit =>
            {
                str += $"{digit}";
            });
            csv.WriteField($"{GetMaNumber(current.SellMa2)} ({str})");
            csv.WriteField("");
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

        public void UpdateProbability(Particle p, StatusValue gBest, StatusValue localWorst)
        {
            if (gBest.Fitness == 0) return;
            for (var index = 0; index < DIGIT_NUMBER; index++)
            {
                // BuyMa1
                if (gBest.BuyMa1[index] > localWorst.BuyMa1[index])
                {
                    p.BuyMa1Beta[index] = Math.Round(p.BuyMa1Beta[index] + DELTA, 3);
                }
                else if (gBest.BuyMa1[index] < localWorst.BuyMa1[index])
                {
                    p.BuyMa1Beta[index] = Math.Round(p.BuyMa1Beta[index] - DELTA, 3);
                }
                // BuyMa2
                if (gBest.BuyMa2[index] > localWorst.BuyMa2[index])
                {
                    p.BuyMa2Beta[index] = Math.Round(p.BuyMa2Beta[index] + DELTA, 3);
                }
                else if (gBest.BuyMa2[index] < localWorst.BuyMa2[index])
                {
                    p.BuyMa2Beta[index] = Math.Round(p.BuyMa2Beta[index] - DELTA, 3);
                }
                // SellMa1
                if (gBest.SellMa1[index] > localWorst.SellMa1[index])
                {
                    p.SellMa1Beta[index] = Math.Round(p.SellMa1Beta[index] + DELTA, 3);
                }
                else if (gBest.SellMa1[index] < localWorst.SellMa1[index])
                {
                    p.SellMa1Beta[index] = Math.Round(p.SellMa1Beta[index] - DELTA, 3);
                }
                // SellMa2
                if (gBest.SellMa2[index] > localWorst.SellMa2[index])
                {
                    p.SellMa2Beta[index] = Math.Round(p.SellMa2Beta[index] + DELTA, 3);
                }
                else if (gBest.SellMa2[index] < localWorst.SellMa2[index])
                {
                    p.SellMa2Beta[index] = Math.Round(p.SellMa2Beta[index] - DELTA, 3);
                }
            }
        }

        public void UpdateProByGN(Particle p, StatusValue gbest, StatusValue localWorst)
        {
            for (var index = 0; index < DIGIT_NUMBER; index++)
            {
                // BuyMa1
                if ((gbest.BuyMa1[index] > localWorst.BuyMa1[index] && p.BuyMa1Beta[index] < 0.5)
                    || (gbest.BuyMa1[index] < localWorst.BuyMa1[index] && p.BuyMa1Beta[index] > 0.5))
                {
                    p.BuyMa1Beta[index] = Math.Round(1 - p.BuyMa1Beta[index], 3);
                }

                // BuyMa2
                if ((gbest.BuyMa2[index] > localWorst.BuyMa2[index] && p.BuyMa2Beta[index] < 0.5)
                    || (gbest.BuyMa2[index] < localWorst.BuyMa2[index] && p.BuyMa2Beta[index] > 0.5))
                {
                    p.BuyMa2Beta[index] = Math.Round(1 - p.BuyMa2Beta[index], 3);
                }

                // SellMa1
                if ((gbest.SellMa1[index] > localWorst.SellMa1[index] && p.SellMa1Beta[index] < 0.5)
                    || (gbest.SellMa1[index] < localWorst.SellMa1[index] && p.SellMa1Beta[index] > 0.5))
                {
                    p.SellMa1Beta[index] = Math.Round(1 - p.SellMa1Beta[index], 3);
                }

                // SellMa2
                if ((gbest.SellMa2[index] > localWorst.SellMa2[index] && p.SellMa2Beta[index] < 0.5)
                    || (gbest.SellMa2[index] < localWorst.SellMa2[index] && p.SellMa2Beta[index] > 0.5))
                {
                    p.SellMa2Beta[index] = Math.Round(1 - p.SellMa2Beta[index], 3);
                }
            }

        }
        public int GetMaNumber(List<int> metrix) 
        {
            return 1 + metrix[7] * 1 + metrix[6] * 2 + metrix[5] * 4 + metrix[4] * 8 + metrix[3] * 16 + metrix[2] * 32 + metrix[1] * 64 + metrix[0] * 128;
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

        public double GetFitness(TestCase currentTestCase, List<StockModel> stockList, ChartData data, DateTime periodStart)
        {
            var transactions = _researchOperationService.GetMyTransactions(data, stockList, currentTestCase, periodStart);
            var earns = _researchOperationService.GetEarningsResults(transactions);
            return earns;
        }
    }

    public interface IGNQTSAlgorithmService : IAlgorithmService
    {
        public void UpdateProByGN(Particle p, StatusValue gbest, StatusValue localWorst);
    }
}
