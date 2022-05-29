using System.Collections.Generic;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface IResearchOperationService
    {
        List<ChartData> CalculateMaFromCsv(string path);
        void CalculateAllMa(ref List<StockModel> stockList);
        List<StockTransaction> ProfitSettlement(double currentStock, List<StockModelDTO> stockList, TestCase testCase, List<StockTransaction> myTrans, double periodEnd);
        double GetEarningsResults(List<StockTransaction> myTrans);
        List<StockTransaction> GetMyTransactions(List<StockModelDTO> stockList, TestCase testCase, double periodStartTimeStamp);
    }
}
