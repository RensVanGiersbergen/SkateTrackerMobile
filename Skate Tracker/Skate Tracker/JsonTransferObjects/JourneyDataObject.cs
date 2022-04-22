using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skate_Tracker.JsonTransferObjects
{
    public class JourneyDataObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public float MaxSpeed { get; set; }
        public int TotalTime { get; set; }
        public int RideTime { get; set; }
        public int PauseTime { get; set; }
    }
}
