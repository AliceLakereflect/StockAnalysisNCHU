using System;
using Stock.Analysis._0607.Service;
using Xunit;

namespace Stock.Analysis.Tests.Service
{
    public class SlidingWindowServiceTests
    {
        private readonly ISlidingWindowService _slidingWindowService;
        public SlidingWindowServiceTests()
        {
            _slidingWindowService = new SlidingWindowService();
        }

        [Theory]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.M, PeriodEnum.M)]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.Q, PeriodEnum.M)]
        [InlineData(2020, 2020, 7, 1, 10, 31, PeriodEnum.H, PeriodEnum.M)]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.Y, PeriodEnum.M)]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.Y, PeriodEnum.Q)]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.Y, PeriodEnum.H)]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.Y, PeriodEnum.Y)]

        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.M, PeriodEnum.M)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Q, PeriodEnum.M)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.H, PeriodEnum.M)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Y, PeriodEnum.M)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Q, PeriodEnum.Q)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.H, PeriodEnum.Q)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Y, PeriodEnum.Q)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.H, PeriodEnum.H)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Y, PeriodEnum.H)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Y, PeriodEnum.Y)]

        [InlineData(2010, 2012, 1, 1, 12, 31, PeriodEnum.M, PeriodEnum.M)]
        [InlineData(2010, 2012, 1, 1, 12, 31, PeriodEnum.Q, PeriodEnum.M)]
        [InlineData(2010, 2012, 1, 1, 12, 31, PeriodEnum.H, PeriodEnum.M)]
        public void GetSlidingWindowsTestX2X(int startTimeYear,
                                            int endTimeYear,
                                            int startTimeMonth,
                                            int startTimeDay,
                                            int endTimeMonth,
                                            int endTimeDay,
                                            PeriodEnum train,
                                            PeriodEnum test)
        {
            var period = new Period { Start = new DateTime(startTimeYear, startTimeMonth, startTimeDay, 0, 0, 0), End = new DateTime(endTimeYear, endTimeMonth, endTimeDay, 0, 0, 0) };
            var result = _slidingWindowService.GetSlidingWindows(period, train, test);
            var expectedTestCount = ((endTimeYear - startTimeYear) * 12 + endTimeMonth - startTimeMonth + 1) / (int)test;
            Assert.Equal(expectedTestCount, result.Count);
            var i = startTimeMonth;
            Assert.All(result, (r) =>
             {
                 var expectedTrainStartMonth = i - (int)train;
                 var expectedTrainStartYear = convertYear(startTimeYear, expectedTrainStartMonth);
                 var expectedTrainEndMonth = i - 1;
                 var expectedTrainEndYear = convertYear(startTimeYear, expectedTrainEndMonth);
                 var expectedTestStartMonth = i;
                 var expectedTestStartYear = convertYear(startTimeYear, expectedTestStartMonth);
                 var expectedTestEndMonth = i + (int)test - 1;
                 var expectedTestEndYear = convertYear(startTimeYear, expectedTestEndMonth);

                 Assert.Equal(new DateTime(expectedTrainStartYear, convertMonth(expectedTrainStartMonth), 1, 0, 0, 0), r.TrainPeriod.Start);
                 Assert.Equal(new DateTime(expectedTrainEndYear, convertMonth(expectedTrainEndMonth), DateTime.DaysInMonth(expectedTrainEndYear, convertMonth(expectedTrainEndMonth)), 0, 0, 0), r.TrainPeriod.End);
                 Assert.Equal(new DateTime(expectedTestStartYear, convertMonth(expectedTestStartMonth), 1, 0, 0, 0), r.TestPeriod.Start);
                 Assert.Equal(new DateTime(expectedTestEndYear, convertMonth(expectedTestEndMonth), DateTime.DaysInMonth(expectedTestEndYear, convertMonth(expectedTestEndMonth)), 0, 0, 0), r.TestPeriod.End);
                 i += (int)test;
             });
        }

        [Theory]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.Q)]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.H)]
        [InlineData(2020, 2020, 4, 1, 7, 31, PeriodEnum.Y)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Q)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.H)]
        [InlineData(2012, 2012, 1, 1, 12, 31, PeriodEnum.Y)]
        [InlineData(2010, 2012, 1, 1, 12, 31, PeriodEnum.Q)]
        [InlineData(2010, 2012, 1, 1, 12, 31, PeriodEnum.H)]
        [InlineData(2010, 2012, 1, 1, 12, 31, PeriodEnum.Y)]
        public void GetSlidingWindowsTestXStar(int startTimeYear,
                                            int endTimeYear,
                                            int startTimeMonth,
                                            int startTimeDay,
                                            int endTimeMonth,
                                            int endTimeDay,
                                            PeriodEnum XStar)
        {
            var period = new Period { Start = new DateTime(startTimeYear, startTimeMonth, startTimeDay, 0, 0, 0), End = new DateTime(endTimeYear, endTimeMonth, endTimeDay, 0, 0, 0) };
            var result = _slidingWindowService.GetSlidingWindows(period, XStar);
            var expectedTestCount = ((endTimeYear - startTimeYear) * 12 + endTimeMonth - startTimeMonth + 1) / (int)XStar;
            Assert.Equal(expectedTestCount, result.Count);
            var i = new DateTime(startTimeYear, startTimeMonth, startTimeDay, 0, 0, 0);
            Assert.All(result, (r) =>
            {
                var expectedStartMonth = i.Month;
                var expectedEndMonth = i.AddMonths((int)XStar - 1).Month;
                var expectedTrainYear = i.AddYears(-1).Year;
                var expectedTestYear = i.Year;

                Assert.Equal(new DateTime(expectedTrainYear, convertMonth(expectedStartMonth), 1, 0, 0, 0), r.TrainPeriod.Start);
                Assert.Equal(new DateTime(expectedTrainYear, convertMonth(expectedEndMonth), DateTime.DaysInMonth(expectedTrainYear, expectedEndMonth), 0, 0, 0), r.TrainPeriod.End);
                Assert.Equal(new DateTime(expectedTestYear, convertMonth(expectedStartMonth), 1, 0, 0, 0), r.TestPeriod.Start);
                Assert.Equal(new DateTime(expectedTestYear, convertMonth(expectedEndMonth), DateTime.DaysInMonth(expectedTestYear, expectedEndMonth), 0, 0, 0), r.TestPeriod.End);
                i = i.AddMonths((int)XStar);
            });
        }

        [Fact]
        public void Test()
        {
            var periodStart = new DateTime(2011, 12, 1, 0, 0, 0);
            var periodEnd = new DateTime(2020, 11, 30, 0, 0, 0);
            var period = new Period { Start = periodStart, End = periodEnd };
            var slidingWindows = _slidingWindowService.GetSlidingWindows(period, PeriodEnum.M, PeriodEnum.M);
            Assert.NotEmpty(slidingWindows);
        }

        private int convertMonth(int month)
        {
            if (month == 0 || month % 12 == 0) return 12;

            if (month < 0) return (month + 12) % 12;

            return month % 12;
        }

        private int convertYear(int y, int m)
        {
            if (m % 12 == 0 || m < 0) return y - 1 + m / 12;
            else return y + m / 12;
        }
    }
}