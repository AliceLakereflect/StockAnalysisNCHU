using System.Collections.Generic;

namespace Stock.Analysis._0607.Service
{
    public interface ISlidingWindowService
    {
        List<SlidingWindow> GetSlidingWindows(Period period, PeriodEnum train, PeriodEnum test);
        List<SlidingWindow> GetSlidingWindows(Period period, PeriodEnum XStar);
    }
}