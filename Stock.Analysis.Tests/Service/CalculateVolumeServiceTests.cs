using Stock.Analysis._0607.Service;
using Xunit;

namespace Stock.Analysis.Tests.Service
{
    public class CalculateVolumeServiceTests
    {
        ICalculateVolumeService _calculateVolumeService = new CalculateVolumeService();
        public CalculateVolumeServiceTests()
        {
        }

        [Theory]
        [InlineData(100000, 10, 10000)]
        [InlineData(100000, 10.1999998092651, 9000)]
        [InlineData(104950.001716614, 11.25, 9000)]
        [InlineData(99100.0051498413, 11.0500001907349, 8000)]
        public void CalculateBuyingVolumeTest(double funds, double price, double expected){
            var result = _calculateVolumeService.CalculateBuyingVolume(funds, price);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100000)]
        public void CalculateSellingVolumeTest(int holdingVolume)
        {
            var result = _calculateVolumeService.CalculateSellingVolume(holdingVolume);
            Assert.Equal(holdingVolume, result);
        }

    }
}
