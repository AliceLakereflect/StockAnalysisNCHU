using System;
using System.Collections.Generic;

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

        public bool TimeToBuy(double? shortMaVal, double? longMaVal, bool hasQty, bool missedBuying)
        {
            return shortMaVal >= longMaVal && hasQty == false && !missedBuying;
        }

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

        public bool TimeToSell(double? shortMaVal, double? longMaVal, bool hasQty)
        {
            return shortMaVal <= longMaVal && hasQty == true;
        }

    }

    public interface ITransTimingService
    {
        void IfMissedBuying(ref bool missedBuying, ref bool first, double? shortMaVal, double? longMaVal);
        bool TimeToBuy(double? shortMaVal, double? longMaVal, bool hasQty, bool missedBuying);
        bool TimeToBuy(int index, List<double?> shortMaVal, List<double?> longMaVal, bool hasQty, bool missedBuying);
        bool TimeToSell(double? shortMaVal, double? longMaVal, bool hasQty);
    }
}
