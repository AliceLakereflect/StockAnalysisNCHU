using System;
using System.Collections.Generic;
using CsvHelper;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface IAlgorithmService
    {
        StatusValue Fit(Queue<int> random, double funds, List<StockModel> stockList, ChartData data, int experiment, CsvWriter csv, DateTime periodStart);
        void UpdateGBestAndGWorst(Particle p, ref StatusValue gBest, ref StatusValue gWorst, int experiment, int iteration);
        void GetLocalBestAndWorst(List<Particle> particles, ref StatusValue localBest, ref StatusValue localWorst);
        void UpdateProbability(Particle p, StatusValue localBest, StatusValue localWorst);
        int GetMaNumber(List<int> metrix);
        void MetureX(Queue<int> random, List<Particle> particles, double funds);
        double GetFitness(TestCase currentTestCase, List<StockModel> stockList, ChartData data, DateTime periodStart);
        AlgorithmConst GetConst();
    }
}
