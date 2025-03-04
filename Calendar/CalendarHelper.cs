using Itenso.TimePeriod;
using System.Globalization;

namespace Calendar;


public class CalendarHelper
{
    public static bool IsConflict(CalendarEvent newEvent, List<CalendarEvent> existingEvents)
    {
        TimeRange newEventPeriod = new TimeRange(newEvent.Start, newEvent.End);

        foreach (var existingEvent in existingEvents)
        {
            TimeRange existingPeriod = new TimeRange(existingEvent.Start, existingEvent.End);
            if (newEventPeriod.OverlapsWith(existingPeriod))
            {
                return true; // Conflict found
            }
        }
        return false;
    }

    public static List<TimeRange> FindFreeSlots(List<CalendarEvent> events, DateTime dayStart, DateTime dayEnd)
    {
        TimeRange dayPeriod = new TimeRange(dayStart, dayEnd);
        TimePeriodCollection busyPeriods = new TimePeriodCollection();

        foreach (var ev in events)
        {
            busyPeriods.Add(new TimeRange(ev.Start, ev.End));
        }

        TimeGapCalculator<TimeRange> gapCalculator = new TimeGapCalculator<TimeRange>();
        ITimePeriodCollection gaps = gapCalculator.GetGaps(busyPeriods);



        return gaps
            .Where(gap => gap.Start >= dayStart && gap.End <= dayEnd)
            .Cast<TimeRange>()
            .ToList();
    }


    public static List<TimeRange> FindFreeSlotsWithinWorkingHours(List<CalendarEvent> events, TimeRange workingHours)
    {
        // Sort events by start time to process them sequentially
        events = [.. events.OrderBy(e => e.Start)];

        List<TimeRange> freeSlots = [];

        // Track the current pointer for working hours
        DateTime currentPointer = workingHours.Start;

        foreach (var ev in events)
        {
            // Skip events that are outside the working hours
            if (ev.End <= workingHours.Start || ev.Start >= workingHours.End)
            {
                continue;
            }

            // If there is a gap between the current pointer and the event's start time, add it as a free slot
            if (currentPointer < ev.Start)
            {
                DateTime freeSlotStart = currentPointer > workingHours.Start ? currentPointer : workingHours.Start;
                DateTime freeSlotEnd = ev.Start < workingHours.End ? ev.Start : workingHours.End;

                if (freeSlotStart < freeSlotEnd)
                {
                    freeSlots.Add(new TimeRange(freeSlotStart, freeSlotEnd));
                }
            }
            // Move the pointer to the end of the event (if it's within working hours)
            currentPointer = ev.End > currentPointer ? ev.End : currentPointer;
        }

        // Check for a free slot after the last event until the end of working hours
        if (currentPointer < workingHours.End)
        {
            freeSlots.Add(new TimeRange(currentPointer, workingHours.End));
        }

        return freeSlots;
    }


    public static TimePeriodCollection CreateDailyRecurrencePattern(DateTime startDate, int PeriodInHours, int totalWeekdays)
    {
        // Calculate the end date, excluding weekends
        DateTime currentDate = startDate;
        int weekdayCount = 0;

        while (weekdayCount < totalWeekdays)
        {
            // Exclude weekends (Saturday and Sunday)
            if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                weekdayCount++;

            // Move to the next day
            if (weekdayCount < totalWeekdays)
                currentDate = currentDate.AddDays(1);

        }

        DateTime endDate = currentDate; // End date with weekends excluded

        // Create a TimePeriodCollection to store the occurrences
        TimePeriodCollection occurrences = [];

        // Generate weekday occurrences
        DateTime nextOccurrence = startDate;
        while (nextOccurrence <= endDate)
        {
            // Exclude weekends (Saturday and Sunday)
            if (nextOccurrence.DayOfWeek != DayOfWeek.Saturday && nextOccurrence.DayOfWeek != DayOfWeek.Sunday)
                occurrences.Add(new TimeRange(nextOccurrence, nextOccurrence.AddHours(PeriodInHours))); // 1-hour events
            
            // Move to the next day
            nextOccurrence = nextOccurrence.AddDays(1);
        }

        return occurrences;
    }
}
