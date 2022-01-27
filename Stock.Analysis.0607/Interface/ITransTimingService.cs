using System.Collections.Generic;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface ITransTimingService
    {
        bool TrueCheckGoldCross(bool check, double? shortMaVal, double? longMaVal);
        bool TimeToBuy(List<double?> shortMaList, List<double?> longMaList, int index, bool hasQty);
        bool TimeToBuy(int index, List<double?> shortMaVal, List<double?> longMaVal, bool hasQty, bool check);
        bool TimeToSell(List<double?> shortMaList, List<double?> longMaList, int index, bool hasQty);
        bool TimeToSell(double currentPrice, double buyPrice, double sellPct, bool hasQty);
        bool TimeToSell(StockTransaction lastTrans, ref double maxPrice, double currentPrice, double currentTime, double sellPct, bool hasQty);
    }
}
