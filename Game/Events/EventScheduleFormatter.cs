using System;
using System.Collections.Generic;
using System.Linq;

namespace MTA.Game.Events;

/// <summary>
///     Utility class for formatting event schedule descriptions
/// </summary>
public static class EventScheduleFormatter {
    /// <summary>
    ///     Get a formatted string describing when an event runs (for NPC dialogs)
    /// </summary>
    /// <param name="schedules">The event schedules to format</param>
    /// <returns>Human-readable schedule description</returns>
    public static string FormatScheduleDescription(IEnumerable<EventSchedule> schedules) {
        var schedulesList = schedules.ToList();
        switch (schedulesList.Count) {
            case 0:
                return "Schedule not available.";
            // Check if it's every hour (same minute/second across all hours, no day restrictions)
            case 24 when schedulesList.All(s => !s.DayOfWeek.HasValue): {
                var firstSchedule = schedulesList[0];
                var minute = firstSchedule.Minute;
                var second = firstSchedule.Second;

                // Verify all schedules have the same minute and second, and cover all 24 hours
                var allHours = schedulesList.Select(s => s.Hour).OrderBy(h => h).ToList();
                var expectedHours = Enumerable.Range(0, 24).ToList();

                if (schedulesList.All(s => s.Minute == minute && s.Second == second) &&
                    allHours.SequenceEqual(expectedHours)) {
                    if (second == 0)
                        return $"at :{minute:D2} of every hour";
                    return $"at :{minute:D2}:{second:D2} of every hour";
                }

                break;
            }
        }

        // Group schedules by pattern
        var timeGroups = schedulesList.GroupBy(s => new { s.Hour, s.Minute, s.Second }).ToList();

        if (timeGroups.Count == 1) {
            var schedule = schedulesList[0];
            var timeStr = $"{schedule.Hour:D2}:{schedule.Minute:D2}";

            // Check if it's every day
            if (!schedule.DayOfWeek.HasValue) return $"daily at {timeStr}";

            // Specific day(s)
            var days = schedulesList.Where(s => s.DayOfWeek.HasValue).Select(s => s.DayOfWeek!.Value).Distinct()
                .OrderBy(d => d).ToList();

            if (days.Count == 1)
                return $"every {days[0]} at {timeStr}";

            // Check if days form a consecutive range
            var dayRange = FormatDayRange(days);
            return $"at {timeStr} on {dayRange}";
        }

        // Multiple different times - check if they can be grouped better
        // Group by minute/second pattern (ignoring hour) to detect "every hour" patterns
        var minuteSecondGroups = schedulesList
            .Where(s => !s.DayOfWeek.HasValue)
            .GroupBy(s => new { s.Minute, s.Second })
            .Where(g => g.Count() == 24 && g.Select(s => s.Hour).OrderBy(h => h).SequenceEqual(Enumerable.Range(0, 24)))
            .ToList();

        if (minuteSecondGroups.Count == 1) {
            var pattern = minuteSecondGroups[0].First();
            return pattern.Second == 0
                ? $"at :{pattern.Minute:D2} of every hour"
                : $"at :{pattern.Minute:D2}:{pattern.Second:D2} of every hour";
        }

        // Multiple different times
        var timeStrings = timeGroups.Select(g => {
            var s = g.First();
            var timeStr = $"{s.Hour:D2}:{s.Minute:D2}";
            if (!s.DayOfWeek.HasValue) return timeStr;
            var days = g.Where(sch => sch.DayOfWeek.HasValue).Select(sch => sch.DayOfWeek!.Value).Distinct()
                .OrderBy(d => d).ToList();
            var dayRange = FormatDayRange(days);
            return $"{timeStr} on {dayRange}";
        }).ToList();

        return timeStrings.Count == 2
            ? $"daily at {timeStrings[0]} and {timeStrings[1]}"
            : $"at {string.Join(", ", timeStrings.Take(timeStrings.Count - 1))}, and {timeStrings.Last()}";
    }

    /// <summary>
    ///     Formats a list of days into a readable string, grouping consecutive days as ranges
    /// </summary>
    /// <param name="days">List of days to format</param>
    /// <returns>Formatted string (e.g., "Monday-Friday" or "Monday-Wednesday, Friday")</returns>
    public static string FormatDayRange(List<DayOfWeek> days) {
        switch (days.Count) {
            case 0:
                return string.Empty;
            case 1:
                return days[0].ToString();
        }

        // Sort days (DayOfWeek enum: Sunday=0, Monday=1, ..., Saturday=6)
        days = days.OrderBy(d => d).ToList();

        var ranges = new List<string>();
        var start = days[0];
        var end = days[0];

        for (var i = 1; i < days.Count; i++) {
            // Check if current day is consecutive to the previous
            // DayOfWeek wraps: 0=Sunday, 1=Monday, ..., 6=Saturday, then back to 0
            var currentDayValue = (int)days[i];
            var previousDayValue = (int)end;
            var isConsecutive = currentDayValue == previousDayValue + 1 ||
                                (previousDayValue == 6 && currentDayValue == 0); // Saturday to Sunday wrap

            if (isConsecutive) { }
            else {
                // End current range and start a new one
                ranges.Add(start == end ? start.ToString() : $"{start}-{end}");
                start = days[i];
            }

            end = days[i];
        }

        // Add the last range
        ranges.Add(start == end ? start.ToString() : $"{start}-{end}");

        return string.Join(", ", ranges);
    }
}