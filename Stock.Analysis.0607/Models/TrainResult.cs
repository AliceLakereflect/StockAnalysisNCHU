using System;
namespace Stock.Analysis._0607.Models
{
    public class TrainResult
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public double Timestamp { get; set; }
        public string StockName { get; set; }
        public string AlgorithmName { get; set; }
        public double Delta { get; set; }
        public int ExperimentNumber { get; set; }
        public int Generations { get; set; }
        public int SearchNodeNumber { get; set; }
        public double InitialCapital { get; set; }
        public double FinalCapital { get; set; }
        public double FinalEarn { get; set; }
        public int Buy1 { get; set; }
        public int Buy2 { get; set; }
        public int Sell1 { get; set; }
        public int Sell2 { get; set; }
        public double ReturnRates { get; set; }
        public double ExperimentNumberOfBest { get; set; }
        public double GenerationOfBest { get; set; }
        public double BestCount { get; set; }

        public TrainResult()
        {
        }
    }
}
