﻿using System.Collections.Generic;

namespace Stock.Analysis._0607.Models
{
    public class Particle
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
}
