using System;
using MedievalCityBuilder.Roads;
using Xunit;

namespace MedievalCityBuilder.Tests
{
    public class RoadCostRoundingTests
    {
        // Settings order: GoldPerUnit, StonePerUnit, MinGold, MinStone,
        // UpkeepStonePerUnitPerPeriod, UsageUpkeepPercent.
        private static RoadCostSettings NearLimitSettings() =>
            new RoadCostSettings(2, 1, 2, 1, 92233720368547758L, 100);

        [Fact]
        public void Upkeep_NumeratorNearLongMax_DoesNotWrapAround()
        {
            // base = 92233720368547758, numerator = base * 100 = 9223372036854775800.
            // The old (numerator + 99) / 100 wrapped negative here.
            long upkeep = RoadCostCalculator.GetSegmentUpkeepStone(NearLimitSettings(), 1.0, 1);

            Assert.Equal(184467440737095516L, upkeep);
        }

        [Fact]
        public void Upkeep_NumeratorOverflow_ThrowsOverflowException()
        {
            Assert.Throws<OverflowException>(
                () => RoadCostCalculator.GetSegmentUpkeepStone(NearLimitSettings(), 1.0, 2));
        }
    }
}

