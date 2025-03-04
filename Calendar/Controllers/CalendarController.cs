// Ignore Spelling: startdate

using Itenso.TimePeriod;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Calendar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController: ControllerBase
    {
        private static List<CalendarEvent> _events = new();

        [HttpGet("events")]
        public IActionResult GetEvents()
        {
            return Ok(_events);
        }

        [HttpPost("add-event")]
        public IActionResult AddEvent([FromBody] CalendarEvent newEvent)
        {
            if (CalendarHelper.IsConflict(newEvent, _events))
            {
                return Conflict("The event conflicts with an existing event.");
            }

            _events.Add(newEvent);
            return Ok("Event added successfully.");
        }

        [HttpGet("free-slots")]
        public IActionResult GetFreeSlots([FromQuery] DateTime dayStart, [FromQuery] DateTime dayEnd)
        {
            var freeSlots = CalendarHelper.FindFreeSlots(_events, dayStart, dayEnd);
            return Ok(freeSlots.Select(slot => new { slot.Start, slot.End }));
        }
        [HttpGet("free-slots-within-working-hours")]
        public IActionResult FindFreeSlotsWithinWorkingHours([FromQuery] DateTime dayStart, [FromQuery] DateTime dayEnd)
        {
            TimeRange workingHours = new(dayStart,dayEnd);

            var freeSlots = CalendarHelper.FindFreeSlotsWithinWorkingHours(_events, workingHours);
            return Ok(freeSlots.Select(slot => new { slot.Start, slot.End }));
        }

        [HttpPost("daily-recurrence-events")]
        public IActionResult CreateDailyRecurrencePattern([FromBody] RecurrenceEvent recurrenceEvent) 
        {
           var recurrenceTimeCollection =  CalendarHelper.CreateDailyRecurrencePattern(recurrenceEvent.DayStart, recurrenceEvent.PeriodInHours, recurrenceEvent.TotalWeekdays);

            List<CalendarEvent> events = recurrenceTimeCollection.Select(x =>
                                                                       new CalendarEvent
                                                                        {
                                                                            Title = recurrenceEvent.Title,
                                                                            Start = x.Start,
                                                                            End = x.End
                                                                        }).ToList();

            _events.AddRange(events);
            return Ok(_events);
        }
        


    }
}
