using System;
using System.Collections.Generic;
using System.Diagnostics;
using CsvHelper;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface IAlgorithmService
    {
        
        void UpdateGBestAndGWorst(Particle p, ref StatusValue gBest, ref StatusValue gWorst, int experiment, int iteration);
        void GetLocalBestAndWorst(List<Particle> particles, ref StatusValue localBest, ref StatusValue localWorst);
        void UpdateProbability(Particle p, StatusValue localBest, StatusValue localWorst);
        AlgorithmConst GetConst();
    }
}
