using Itenso.TimePeriod;
using Microsoft.AspNetCore.Mvc;

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
            TimeRange workingHours = new TimeRange(
                new DateTime(2025, 2, 5, 9, 0, 0),
                new DateTime(2025, 2, 5, 17, 0, 0)
            );
            var freeSlots = CalendarHelper.FindFreeSlotsWithinWorkingHours(_events, workingHours);
            return Ok(freeSlots.Select(slot => new { slot.Start, slot.End }));
        }

        // recuring events
        // update
        // delete
        // get event details by id
        // get events by date
    }
}
