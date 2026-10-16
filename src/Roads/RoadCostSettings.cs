using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Immutable settings for road construction cost and ongoing maintenance.
    /// All values are PLACEHOLDERS pending playtesting (DESIGN.md defers road cost and
    /// maintenance rates to prototyping; see ARCHITECTURE.md prototype-first rule).
    /// </summary>
    /// <remarks>
    /// A "unit" is one world unit of segment length. A "period" is one simulation period;
    /// the real length of a simulation period is decided later and is not modelled here.
    /// </remarks>
    public sealed record RoadCostSettings
    {
        /// <summary>
        /// Gold cost per world unit of segment length (must be >= 0).
        /// PLACEHOLDER: 2 pending playtesting.
        /// </summary>
        public long GoldPerUnit { get; init; }

        /// <summary>
        /// Stone cost per world unit of segment length (must be >= 0).
        /// PLACEHOLDER: 1 pending playtesting.
        /// </summary>
        public long StonePerUnit { get; init; }

        /// <summary>
        /// Minimum gold cost for any segment, even a zero-length one (must be >= 0).
        /// PLACEHOLDER: 2 pending playtesting.
        /// </summary>
        public long MinGold { get; init; }

        /// <summary>
        /// Minimum stone cost for any segment, even a zero-length one (must be >= 0).
        /// PLACEHOLDER: 1 pending playtesting.
        /// </summary>
        public long MinStone { get; init; }

        /// <summary>
        /// Stone consumed per world unit per simulation period of upkeep (must be >= 0).
        /// PLACEHOLDER: 1 pending playtesting.
        /// </summary>
        public long UpkeepStonePerUnitPerPeriod { get; init; }

        /// <summary>
        /// Extra upkeep percentage added per usage point. Usage points are counted by the
        /// caller (e.g. number of citizens or buildings served by the segment).
        /// PLACEHOLDER: 5 pending playtesting.
        /// </summary>
        public int UsageUpkeepPercent { get; init; }

        /// <summary>
        /// Creates a new RoadCostSettings instance with validation.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is negative.</exception>
        public RoadCostSettings(
            long goldPerUnit,
            long stonePerUnit,
            long minGold,
            long minStone,
            long upkeepStonePerUnitPerPeriod,
            int usageUpkeepPercent)
        {
            if (goldPerUnit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(goldPerUnit), "GoldPerUnit must be >= 0.");
            }
            if (stonePerUnit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stonePerUnit), "StonePerUnit must be >= 0.");
            }
            if (minGold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minGold), "MinGold must be >= 0.");
            }
            if (minStone < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minStone), "MinStone must be >= 0.");
            }
            if (upkeepStonePerUnitPerPeriod < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(upkeepStonePerUnitPerPeriod), "UpkeepStonePerUnitPerPeriod must be >= 0.");
            }
            if (usageUpkeepPercent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(usageUpkeepPercent), "UsageUpkeepPercent must be >= 0.");
            }

            GoldPerUnit = goldPerUnit;
            StonePerUnit = stonePerUnit;
            MinGold = minGold;
            MinStone = minStone;
            UpkeepStonePerUnitPerPeriod = upkeepStonePerUnitPerPeriod;
            UsageUpkeepPercent = usageUpkeepPercent;
        }

        /// <summary>
        /// Default road cost settings with placeholder values.
        /// </summary>
        public static readonly RoadCostSettings Default = new RoadCostSettings(
            goldPerUnit: 2,               // PLACEHOLDER: pending playtesting
            stonePerUnit: 1,             // PLACEHOLDER: pending playtesting
            minGold: 2,                  // PLACEHOLDER: pending playtesting
            minStone: 1,                 // PLACEHOLDER: pending playtesting
            upkeepStonePerUnitPerPeriod: 1, // PLACEHOLDER: pending playtesting
            usageUpkeepPercent: 5          // PLACEHOLDER: pending playtesting
        );
    }
}