using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skate_Tracker.JsonTransferObjects
{
    public class PositionDataObject
    {
        public int JourneyID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float Speed { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
