﻿using System;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;
using Xunit;
using System.Collections.Generic;
using Stock.Analysis.Tests.MockData;
using System.Linq;

namespace Stock.Analysis.Tests.Service
{
    public class QTSAlgorithmServiceTests
    {
        private readonly IAlgorithmService _qtsService = new QTSAlgorithmService();
        private readonly IMovingAvarageService _movingAvarageService = new MovingAvarageService();
        private readonly IRepository _historyRepository = new HistoryRepository();
        public QTSAlgorithmServiceTests()
        {
        }

        [Fact]
        public void TestFit120d()
        {
            var stockList = _historyRepository.GetRealData120dOf2603();
            var chartData = new ChartData
            {
                Name = "2603.TW",
                Price = new List<double?>()
            };

            chartData.Price = stockList.Select(s => s.Price).ToList();
            chartData.Timestamp = stockList.Select(s => s.Date).ToList();
            //var gbest = _qtsService.Fit(new Random(343), 100000, stockList, chartData, 0);
            //Assert.Equal(155760, gbest.Fitness);
            //Assert.Equal(27, _qtsService.GetMaNumber(gbest.BuyMa1));
            //Assert.Equal(7, _qtsService.GetMaNumber(gbest.BuyMa2));
            //Assert.Equal(154, _qtsService.GetMaNumber(gbest.SellMa1));
            //Assert.Equal(254, _qtsService.GetMaNumber(gbest.SellMa2));
        }

        [Fact]
        public void TestFit1y()
        {
            var stockList = _historyRepository.GetRealData1yOf2603();
            var chartData = new ChartData
            {
                Name = "2603.TW",
                Price = new List<double?>()
            };

            chartData.Price = stockList.Select(s => s.Price).ToList();
            chartData.Timestamp = stockList.Select(s => s.Date).ToList();
            //var gbest = _qtsService.Fit(new Random(343),100000, stockList, chartData, 0);
            //Assert.Equal(2336908, gbest.Fitness);
            //Assert.Equal(29, _qtsService.GetMaNumber(gbest.BuyMa1));
            //Assert.Equal(13, _qtsService.GetMaNumber(gbest.BuyMa2));
            //Assert.Equal(244, _qtsService.GetMaNumber(gbest.SellMa1));
            //Assert.Equal(243, _qtsService.GetMaNumber(gbest.SellMa2));
        }

        [Theory]
        [InlineData(23.0, 23.0, 0)]
        [InlineData(2.0, 20.0, 0)]
        public void TestUpdateGBestAndGWorst(double currentFitness, double expectedBestFitness, double expectedWorstFitness)
        {
            var best = new StatusValue { Fitness = 20};
            var worst = new StatusValue { Fitness = 0};
            var particale = new Particle { CurrentFitness = new StatusValue { Fitness = currentFitness } };
            _qtsService.UpdateGBestAndGWorst(particale, ref best, ref worst, 0, 0);

            Assert.Equal(expectedBestFitness, best.Fitness);
            Assert.Equal(expectedWorstFitness, worst.Fitness);
        }

        [Fact]
        public void TestGetLocalBestAndWorst()
        {
            var best = new StatusValue();
            var worst = new StatusValue();
            var particles = new List<Particle>();
            for (var i = 0; i < 5; i++)
            {
                particles.Add(new Particle { CurrentFitness = new StatusValue { Fitness = i } });
            }
            _qtsService.GetLocalBestAndWorst(particles, ref best, ref worst);

            Assert.Equal(4, best.Fitness);
            Assert.Equal(0, worst.Fitness);
        }

        [Theory]
        [MemberData(nameof(TestData.ProbabilityParameters), MemberType = typeof(TestData))]
        public void TestUpdateProbability(StatusValue best, StatusValue worst, List<double> expectBuyBeta1, List<double> expectBuyBeta2, List<double> expectSellBeta1, List<double> expectSellBeta2)
        {
            var particle = new Particle();
            _qtsService.UpdateProbability(particle, best, worst);
            var index = 0;
            Assert.All(expectBuyBeta1, d =>
            {
                Assert.Equal(d, particle.BuyMa1Beta[index]);
                index++;
            });
            index = 0;
            Assert.All(expectBuyBeta2, d => {
                Assert.Equal(d, particle.BuyMa2Beta[index]);
                index++;
            });
            index = 0;
            Assert.All(expectSellBeta1, d => {
                Assert.Equal(d, particle.SellMa1Beta[index]);
                index++;
            });
            index = 0;
            Assert.All(expectSellBeta2, d => {
                Assert.Equal(d, particle.SellMa2Beta[index]);
                index++;
            });
        }
        
        [Theory]
        [MemberData(nameof(TestData.MaMetrix), MemberType = typeof(TestData))]
        public void TestGetMaNumber(List<int> metrix, int expect)
        {
            var result = _qtsService.GetMaNumber(metrix);
            Assert.Equal(expect, result);
        }

        [Theory]
        [MemberData(nameof(TestData.MeatureX), MemberType = typeof(TestData))]
        public void TestMetureX(List<int> expectBuyMa1, List<int> expectBuyMa2, List<int> expectSellMa1, List<int> expectSellMa2)
        {
            var ramdom = new Random(343);

            // initialize nodes
            List<Particle> particles = new List<Particle>();
            particles.Add(new Particle());

            //_qtsService.MetureX(ramdom, particles, 10000000);

            //var result = particles.First().CurrentFitness;
            //var index = 0;
            //Assert.All(expectBuyMa1, d =>
            //{
            //    Assert.Equal(d, result.BuyMa1[index]);
            //    index++;
            //});
            //index = 0;
            //Assert.All(expectBuyMa2, d => {
            //    Assert.Equal(d, result.BuyMa2[index]);
            //    index++;
            //});
            //index = 0;
            //Assert.All(expectSellMa1, d => {
            //    Assert.Equal(d, result.SellMa1[index]);
            //    index++;
            //});
            //index = 0;
            //Assert.All(expectSellMa2, d => {
            //    Assert.Equal(d, result.SellMa2[index]);
            //    index++;
            //});
        }

        [Fact]
        public void TestGetFitness()
        {
            var testCase = new TestCase
            {
                Funds = 100000,
                BuyShortTermMa = 5,
                BuyLongTermMa = 20,
                SellShortTermMa = 5,
                SellLongTermMa = 20
            };
            var periodStart = new DateTime(2020, 1, 1, 0, 0, 0);
            var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
            var dataList = _historyRepository.GetRealData1yOf2603();
            var chartData = new ChartData
            {
                Name = "2603.TW",
                Price = new List<double?>()
            };

            chartData.Price = dataList.Select(s => s.Price).ToList();
            chartData.Timestamp = dataList.Select(s => s.Date).ToList();
            chartData.MaList.Add(5, _movingAvarageService.CalculateMovingAvarage(dataList, 5).Select(s => s.Price).ToList());
            chartData.MaList.Add(20, _movingAvarageService.CalculateMovingAvarage(dataList, 20).Select(s => s.Price).ToList());
            var result = _qtsService.GetFitness(testCase, dataList, chartData, periodStart);

            Assert.Equal(1297975, result);
        }

        public class TestData
        {
            public static IEnumerable<object[]> MaMetrix =>
            new List<object[]>
            {
                new object[] { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0 },  129 },
                new object[] { new List<int> { 0, 0, 0, 0, 0, 0, 0, 1 },  2 },
                new object[] { new List<int> { 0, 0, 1, 0, 0, 0, 0, 1 },  34 }
            };

            public static IEnumerable<object[]> MeatureX =>
            new List<object[]>
            {
                new object[] { new List<int> { 0, 1, 0, 0, 1, 1, 0, 0},  new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 }, new List<int> { 1, 0, 0, 1, 1, 0, 0, 0 }, new List<int> { 1, 1, 0, 1, 1, 1, 0, 0 } },
            };

            public static IEnumerable<object[]> ProbabilityParameters =>
            new List<object[]>
            {
                new object[] {
                    new StatusValue {
                        BuyMa1 = new List<int> { 0, 1, 0, 0, 1, 1, 0, 0 },
                        BuyMa2 = new List<int> { 0, 1, 0, 0, 1, 1, 0, 0 },
                        SellMa1 = new List<int> { 0, 1, 0, 0, 1, 1, 0, 0 },
                        SellMa2 = new List<int> { 0, 1, 0, 0, 1, 1, 0, 0 }
                    },
                    new StatusValue {
                        BuyMa1 = new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 },
                        BuyMa2 = new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 },
                        SellMa1 = new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 },
                        SellMa2 = new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 }
                    },
                    new List<double> { 0.497, 0.503, 0.5, 0.497, 0.5, 0.503, 0.497, 0.497 },
                    new List<double> { 0.497, 0.503, 0.5, 0.497, 0.5, 0.503, 0.497, 0.497 },
                    new List<double> { 0.497, 0.503, 0.5, 0.497, 0.5, 0.503, 0.497, 0.497 },
                    new List<double> { 0.497, 0.503, 0.5, 0.497, 0.5, 0.503, 0.497, 0.497 }
                },
            };

        }
    }
}