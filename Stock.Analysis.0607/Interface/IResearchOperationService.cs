using System;
using System.Collections.Generic;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface IResearchOperationService
    {
        List<ChartData> CalculateMaFromCsv(string path);
        ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList, double start, double end);
        List<StockTransaction> ProfitSettlement(double currentStock, List<StockModel> stockList, TestCase testCase, List<StockTransaction> myTrans, double periodEnd);
        double GetEarningsResults(List<StockTransaction> myTrans);
        List<StockTransaction> GetMyTransactions(ChartData data, List<StockModel> stockList, TestCase testCase, DateTime periodStart);
    }
}
