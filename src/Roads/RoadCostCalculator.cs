using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Pure functions for calculating road construction costs and ongoing upkeep stone requirements.
    /// Deducting resources, paying construction time, and validating terrain are separate concerns
    /// handled by other systems (see RoadPlacementValidator).
    /// </summary>
    public static class RoadCostCalculator
    {
        private const double MaxAllowedLength = 1000000.0;

        /// <summary>
        /// Calculates the construction cost (gold and stone) for a road segment of given length.
        /// </summary>
        /// <param name="settings">Road cost settings.</param>
        /// <param name="length">Length of the segment in world units.</param>
        /// <returns>The gold and stone cost.</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when length is NaN, infinite, negative, or exceeds 1,000,000.</exception>
        /// <exception cref="OverflowException">Thrown when cost calculation overflows long.</exception>
        public static RoadCost GetSegmentCost(RoadCostSettings settings, double length)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "RoadCostSettings must not be null.");
            }
            if (double.IsNaN(length) || double.IsInfinity(length))
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be a finite number.");
            }
            if (length < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be >= 0.");
            }
            if (length > MaxAllowedLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds the sanity cap of 1,000,000.");
            }

            // Every started world unit is charged (ceiling). Length 0 gives 0 units.
            long units = (long)Math.Ceiling(length);

            long calculatedGold;
            long calculatedStone;
            checked
            {
                calculatedGold = units * settings.GoldPerUnit;
                calculatedStone = units * settings.StonePerUnit;
            }

            long gold = Math.Max(settings.MinGold, calculatedGold);
            long stone = Math.Max(settings.MinStone, calculatedStone);

            return new RoadCost(gold, stone);
        }

        /// <summary>
        /// Calculates the construction cost (gold and stone) for a road segment in a RoadGraph by its ID.
        /// </summary>
        /// <param name="settings">Road cost settings.</param>
        /// <param name="graph">The road graph.</param>
        /// <param name="id">The segment ID.</param>
        /// <returns>The gold and stone cost.</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings or graph is null.</exception>
        /// <exception cref="ArgumentException">Propagated from graph.GetSegment if the segment ID is unknown.</exception>
        public static RoadCost GetSegmentCost(RoadCostSettings settings, RoadGraph graph, RoadSegmentId id)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "RoadCostSettings must not be null.");
            }
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph), "RoadGraph must not be null.");
            }

            double length = graph.GetSegmentLength(id);
            return GetSegmentCost(settings, length);
        }

        /// <summary>
        /// Calculates the total construction cost for all road segments in a RoadGraph, summed in segment ID order.
        /// An empty graph returns RoadCost.Zero.
        /// </summary>
        /// <param name="settings">Road cost settings.</param>
        /// <param name="graph">The road graph.</param>
        /// <returns>The total gold and stone cost.</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings or graph is null.</exception>
        public static RoadCost GetGraphCost(RoadCostSettings settings, RoadGraph graph)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "RoadCostSettings must not be null.");
            }
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph), "RoadGraph must not be null.");
            }

            RoadCost total = RoadCost.Zero;
            foreach (var segment in graph.Segments)
            {
                double length = graph.GetSegmentLength(segment.Id);
                RoadCost segCost = GetSegmentCost(settings, length);
                total = total + segCost; // uses checked operator +
            }

            return total;
        }

        /// <summary>
        /// Calculates the stone upkeep required for a road segment of given length and usage points over one period.
        /// </summary>
        /// <remarks>
        /// Usage raises upkeep linearly (DESIGN.md: maintenance increases with road usage).
        /// Rounding rule: always rounded UP (integer ceiling division: ceil(base * UsageUpkeepPercent * usagePoints / 100)),
        /// so upkeep is never undercharged.
        /// </remarks>
        /// <param name="settings">Road cost settings.</param>
        /// <param name="length">Length of the segment in world units.</param>
        /// <param name="usagePoints">Number of usage points on the segment (must be >= 0).</param>
        /// <returns>The stone upkeep amount.</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when length is invalid or usagePoints is negative.</exception>
        /// <exception cref="OverflowException">Thrown when calculation overflows long.</exception>
        public static long GetSegmentUpkeepStone(RoadCostSettings settings, double length, int usagePoints)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "RoadCostSettings must not be null.");
            }
            if (double.IsNaN(length) || double.IsInfinity(length))
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be a finite number.");
            }
            if (length < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be >= 0.");
            }
            if (length > MaxAllowedLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds the sanity cap of 1,000,000.");
            }
            if (usagePoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(usagePoints), "UsagePoints must be >= 0.");
            }

            long units = (long)Math.Ceiling(length);
            long baseUpkeep;
            checked
            {
                baseUpkeep = units * settings.UpkeepStonePerUnitPerPeriod;
            }

            if (usagePoints == 0 || settings.UsageUpkeepPercent == 0)
            {
                return baseUpkeep;
            }

            // extra = ceil(baseUpkeep * UsageUpkeepPercent * usagePoints / 100)
            // Using integer ceiling division: (numerator + 99) / 100 for positive numerator.
            long numerator;
            checked
            {
                numerator = baseUpkeep * settings.UsageUpkeepPercent * usagePoints;
            }

            long extra = (numerator + 99) / 100;

            long totalUpkeep;
            checked
            {
                totalUpkeep = baseUpkeep + extra;
            }

            return totalUpkeep;
        }

        /// <summary>
        /// Calculates the total stone upkeep for all road segments in a RoadGraph, summed in segment ID order,
        /// using a callback to determine usage points for each segment.
        /// An empty graph returns 0 without calling the callback.
        /// </summary>
        /// <param name="settings">Road cost settings.</param>
        /// <param name="graph">The road graph.</param>
        /// <param name="usagePointsForSegment">Callback returning usage points for a given segment ID.</param>
        /// <returns>The total stone upkeep.</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings, graph, or usagePointsForSegment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the callback returns a negative usage points value.</exception>
        public static long GetGraphUpkeepStone(RoadCostSettings settings, RoadGraph graph, Func<RoadSegmentId, int> usagePointsForSegment)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "RoadCostSettings must not be null.");
            }
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph), "RoadGraph must not be null.");
            }
            if (usagePointsForSegment == null)
            {
                throw new ArgumentNullException(nameof(usagePointsForSegment), "Usage points callback must not be null.");
            }

            long totalUpkeep = 0;
            foreach (var segment in graph.Segments)
            {
                int usage = usagePointsForSegment(segment.Id);
                if (usage < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(usagePointsForSegment), "Callback returned negative usage points.");
                }

                double length = graph.GetSegmentLength(segment.Id);
                long segUpkeep = GetSegmentUpkeepStone(settings, length, usage);
                checked
                {
                    totalUpkeep += segUpkeep;
                }
            }

            return totalUpkeep;
        }
    }
}