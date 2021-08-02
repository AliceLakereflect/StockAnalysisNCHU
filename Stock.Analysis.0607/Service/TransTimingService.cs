using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class TransTimingService: ITransTimingService
    {
        public TransTimingService()
        {
        }

        public void IfMissedBuying(ref bool missedBuying, ref bool first, double? shortMaVal, double? longMaVal)
        {
            if (first && shortMaVal >= longMaVal)
            {
                missedBuying = true;
            }
            first = false;
            if (missedBuying && shortMaVal < longMaVal)
            {
                missedBuying = false;
            }
        }

        // 黃金交叉
        public bool TimeToBuy(double? shortMaVal, double? longMaVal, bool hasQty, bool missedBuying)
        {
            return shortMaVal >= longMaVal && hasQty == false && !missedBuying;
        }

        // 黃金交叉且均線向上
        public bool TimeToBuy(int index, List<double?> shortMaValList, List<double?> longMaValList, bool hasQty, bool missedBuying)
        {
            var shortMaVal = shortMaValList[index];
            var longMaVal = longMaValList[index];
            var condition1 = shortMaVal >= longMaVal && !hasQty && !missedBuying;
            bool condition2;
            if (index == 0)
            {
                condition2 = true;
            }
            else
            {
                var shortMaGoUp = shortMaValList[index - 1] < shortMaVal;
                var LongMaGoUp = longMaValList[index - 1] < longMaVal;
                condition2 = shortMaGoUp && LongMaGoUp;
            }
            return condition1 && condition2;
        }

        // 死亡交叉
        public bool TimeToSell(double? shortMaVal, double? longMaVal, bool hasQty)
        {
            return shortMaVal <= longMaVal && hasQty == true;
        }

        // 移動停損
        public bool TimeToSell(double lastTransTime, ref double maxPrice, double price, double currentTime, bool hasQty)
        {
            if (!hasQty)
            {
                return false;
            }
            if (currentTime > lastTransTime)
            {
                if (price > maxPrice)
                {
                    maxPrice = price;
                }

                var stopPrice = maxPrice * (1 - 0.1);
                if (price <= stopPrice)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }

    public interface ITransTimingService
    {
        void IfMissedBuying(ref bool missedBuying, ref bool first, double? shortMaVal, double? longMaVal);
        bool TimeToBuy(double? shortMaVal, double? longMaVal, bool hasQty, bool missedBuying);
        bool TimeToBuy(int index, List<double?> shortMaVal, List<double?> longMaVal, bool hasQty, bool missedBuying);
        bool TimeToSell(double? shortMaVal, double? longMaVal, bool hasQty);
        bool TimeToSell(double lastTransTime, ref double maxPrice, double currentPrice, double currentTime, bool hasQty);
    }
}
