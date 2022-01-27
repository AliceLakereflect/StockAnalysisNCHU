using System.Collections.Generic;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public interface IMovingAvarageService
    {
        List<StockModel?> CalculateMovingAvarage(List<StockModel> stockList, int avgDay);
    }
}
