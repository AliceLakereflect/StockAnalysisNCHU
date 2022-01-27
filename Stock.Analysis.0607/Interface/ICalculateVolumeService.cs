namespace Stock.Analysis._0607.Service
{
    public interface ICalculateVolumeService
    {
        int CalculateBuyingVolume(double funds, double price);
        int CalculateBuyingVolumeOddShares(double funds, double price);
        int CalculateSellingVolume(decimal holdingVolumn);
    }
}
