using System.Collections.Generic;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface ITransTimingService
    {
        bool TrueCheckGoldCross(bool check, double? shortMaVal, double? longMaVal);
        bool TimeToBuy(double? shortMaVal, double? longMaVal, double? prevShortMaVal, double? prevLongMaVal, bool hasQty);
        bool TimeToBuy(int index, List<double?> shortMaVal, List<double?> longMaVal, bool hasQty, bool check);
        bool TimeToSell(double? shortMaVal, double? longMaVal, double? prevShortMaVal, double? prevLongMaVal, bool hasQty);
        bool TimeToSell(double currentPrice, double buyPrice, double sellPct, bool hasQty);
        bool TimeToSell(StockTransaction lastTrans, ref double maxPrice, double currentPrice, double currentTime, double sellPct, bool hasQty);
    }
}
