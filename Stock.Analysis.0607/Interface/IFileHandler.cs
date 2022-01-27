using System.Collections.Generic;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface IFileHandler
    {
        List<List<StockModel>> ReadDataFromFile(string path);
        void OutputResult<T>(List<T> chartDataList, string fileName);
        void OutputCsv(List<ChartData> chartDataList, string fileName);
        void OutputTransaction(List<StockTransList> MyTransList, string fileName);
        void OutputEarn(List<StockTransList> MyTransList, string fileName);
        void OutputQTSResult(AlgorithmConst algorithmConst, double funds, StatusValue gBest, List<StockTransaction> transactions, string fileName);
        ChartData ReadMaByFile(string fileName);
        Queue<int> Readcsv(string fileName);
    }
}
