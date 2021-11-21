﻿using System;
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

        public bool TrueCheckGoldCross(bool check, double? shortMaVal, double? longMaVal)
        {
            if (!check && shortMaVal <= longMaVal)
            {
                check = true;
            }
            else if (check && shortMaVal >= longMaVal)
            {
                check = false;
            }
            return check;
        }

        // 黃金交叉
        public bool TimeToBuy(double? shortMaVal, double? longMaVal, bool hasQty, bool check)
        {
            return shortMaVal >= longMaVal && hasQty == false && check;
        }

        // 黃金交叉且均線向上
        public bool TimeToBuy(int index, List<double?> shortMaValList, List<double?> longMaValList, bool hasQty, bool check)
        {
            var shortMaVal = shortMaValList[index];
            var longMaVal = longMaValList[index];
            var condition1 = shortMaVal >= longMaVal && !hasQty && check;
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

        // 一般停損
        public bool TimeToSell(double currentPrice, double buyPrice, double sellPct, bool hasQty)
        {
            if (!hasQty)
            {
                return false;
            }
            var lossPrice = buyPrice * (100 - sellPct) / 100;
            var earnPrice = buyPrice * (100 + sellPct) / 100;
            if (currentPrice <= lossPrice || currentPrice >= earnPrice)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 移動停損
        public bool TimeToSell(StockTransaction lastTrans, ref double maxPrice, double currentPrice,
            double currentTime, double sellPct, bool hasQty)
        {
            if (!hasQty)
            {
                return false;
            }
            if (currentTime > lastTrans.TransTime)
            {
                if (currentPrice > maxPrice)
                {
                    maxPrice = currentPrice;
                }
                double stopPrice = calculateStopPrice(lastTrans.TransPrice, maxPrice, sellPct);
                if (currentPrice <= stopPrice)
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

        private static double calculateStopPrice(double lastTransPrice, double maxPrice, double sellPct)
        {
            var stopPrice = maxPrice * (100 - sellPct) / 100;
            var lossSellPrice = lastTransPrice * (100 - sellPct / 10) / 100;
            if (lossSellPrice > stopPrice)
            {
                return lossSellPrice;
            }
            else
            {
                return stopPrice;
            }
        }
    }

    public interface ITransTimingService
    {
        bool TrueCheckGoldCross(bool check, double? shortMaVal, double? longMaVal);
        bool TimeToBuy(double? shortMaVal, double? longMaVal, bool hasQty, bool check);
        bool TimeToBuy(int index, List<double?> shortMaVal, List<double?> longMaVal, bool hasQty, bool check);
        bool TimeToSell(double? shortMaVal, double? longMaVal, bool hasQty);
        bool TimeToSell(double currentPrice, double buyPrice, double sellPct, bool hasQty);
        bool TimeToSell(StockTransaction lastTrans, ref double maxPrice, double currentPrice, double currentTime, double sellPct, bool hasQty);
    }
}
