using System;
using Stock.Analysis._0607.Service;
using Stock.Analysis._0607.Models;
using Xunit;
using System.Collections.Generic;
using Stock.Analysis.Tests.MockData;
using System.Linq;
using Moq;
using AutoMapper;

namespace Stock.Analysis.Tests.Service
{
    public class QTSAlgorithmServiceTests
    {
        private readonly IQTSAlgorithmService _qtsService;
        private readonly IResearchOperationService _researchOperationService;
        private readonly IMovingAvarageService _movingAvarageService = new MovingAvarageService();
        private readonly ITransTimingService _transTimingService = new TransTimingService();
        private readonly ICalculateVolumeService _calculateVolumeService = new CalculateVolumeService();
        private readonly Mock<IFileHandler> _fileHandler = new Mock<IFileHandler>();
        private readonly IRepository _historyRepository = new HistoryRepository();
        public QTSAlgorithmServiceTests()
        {
            _researchOperationService = new ResearchOperationService(_movingAvarageService, _transTimingService, _calculateVolumeService, _fileHandler.Object);
            _qtsService = new QTSAlgorithmService(_researchOperationService);
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
            var result = Utils.GetMaNumber(metrix);
            Assert.Equal(expect, result);
        }

        [Theory]
        [MemberData(nameof(TestData.MeatureX), MemberType = typeof(TestData))]
        public void TestMetureX(List<int> expectBuyMa1, List<int> expectBuyMa2, List<int> expectSellMa1, List<int> expectSellMa2)
        {
            //var random = new Random(343);

            // initialize nodes
            List<Particle> particles = new List<Particle>();
            particles.Add(new Particle());
            var list = new List<int> { 1158, 24406, 21033, 31385, 7810, 15177, 27360, 11270, 5061, 18977, 20657, 10440, 25369, 6807, 30273, 31878, 32476, 21913, 8341, 22714, 4334, 669, 27165, 7081, 13774, 26165, 30449, 17304, 29439, 12003, 15962, 8765, 27825, 25647, 3476, 25716, 7837, 19448, 10194, 16947, 7217, 14856, 10115, 27328, 7149, 32235, 1236, 289, 21005, 25344, 25974, 15573, 24168, 30416, 6561, 705, 20888, 2463, 27225, 2669, 3419, 7489, 7408, 25456, 1081, 13871, 20428, 28457, 7271, 17114, 5226, 8880, 14828, 3391, 27299, 9484, 13314, 27324, 302, 7591, 14783, 7714, 24423, 18175, 27124, 8203, 23630, 6557, 8790, 16750, 1747, 9035, 18907, 23664, 25425, 24194, 6759, 1917, 8792, 7714, 11688, 32408, 13487, 24933, 20798, 14063, 24729, 9238, 26144, 20659, 15448, 32510, 20283, 23334, 12784, 11424, 13148, 7926, 28717, 17958, 5711, 25289, 27879, 19610, 3401, 24089, 23685, 21081, 4739, 20034, 17088 };
            var random = new Random();
            var cRandom = new Queue<int>();
            list.ForEach(i => cRandom.Enqueue(i));

            _qtsService.MeatureX(cRandom, random, particles, 10000000);

            var result = particles.First().CurrentFitness;
            var index = 0;
            Assert.All(expectBuyMa1, d =>
            {
                Assert.Equal(d, result.BuyMa1[index]);
                index++;
            });
            index = 0;
            Assert.All(expectBuyMa2, d =>
            {
                Assert.Equal(d, result.BuyMa2[index]);
                index++;
            });
            index = 0;
            Assert.All(expectSellMa1, d =>
            {
                Assert.Equal(d, result.SellMa1[index]);
                index++;
            });
            index = 0;
            Assert.All(expectSellMa2, d =>
            {
                Assert.Equal(d, result.SellMa2[index]);
                index++;
            });
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
            var periodStart = Utils.ConvertToUnixTimestamp(new DateTime(2020, 1, 1, 0, 0, 0));
            var periodEnd = new DateTime(2021, 6, 30, 0, 0, 0);
            var dataList = _historyRepository.GetRealData1yOf2603();
            dataList = _movingAvarageService.CalculateMovingAvarage(dataList, 5);
            dataList = _movingAvarageService.CalculateMovingAvarage(dataList, 20);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StockModel, StockModelDTO>();
                cfg.AddProfile<StockModelDTO>();
            });
            var mapper = config.CreateMapper();
            var stockListDto = mapper.Map<List<StockModel>, List<StockModelDTO>>(dataList);

            var result = _qtsService.GetFitness(testCase, stockListDto.OrderBy(s => s.Date).ToList(), periodStart);

            Assert.Equal(1297974 + testCase.Funds, Math.Round(result));
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
                new object[] { new List<int> { 1, 0, 0, 0, 1, 1, 0, 1},  new List<int> { 1, 0, 0, 1, 0, 1, 0, 0 }, new List<int> { 0,0,1,0,1,1,0,1 }, new List<int> { 1,0,0,0,0,1,1,1 } },
            };

            public static IEnumerable<object[]> ProbabilityParameters =>
            new List<object[]>
            {
                new object[] {
                    new StatusValue {
                        BuyMa1 = new List<int> { 1, 0, 0, 0, 1, 1, 0, 1 },
                        BuyMa2 = new List<int> { 0, 1, 0, 0, 1, 1, 0, 0 },
                        SellMa1 = new List<int> { 0, 1, 0, 0, 1, 1, 0, 0 },
                        SellMa2 = new List<int> { 0, 1, 0, 0, 1, 1, 0, 0 },
                        Fitness = 1
                    },
                    new StatusValue {
                        BuyMa1 = new List<int> { 1, 1, 0, 0, 1, 0, 1, 1 },
                        BuyMa2 = new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 },
                        SellMa1 = new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 },
                        SellMa2 = new List<int> { 1, 0, 0, 1, 1, 0, 1, 1 }
                    },
                    new List<double> { 0.5, 0.497, 0.5, 0.5, 0.5, 0.503, 0.497, 0.5 },
                    new List<double> { 0.497, 0.503, 0.5, 0.497, 0.5, 0.503, 0.497, 0.497 },
                    new List<double> { 0.497, 0.503, 0.5, 0.497, 0.5, 0.503, 0.497, 0.497 },
                    new List<double> { 0.497, 0.503, 0.5, 0.497, 0.5, 0.503, 0.497, 0.497 }
                },
            };

        }
    }
}
