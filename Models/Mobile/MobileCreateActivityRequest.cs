using System;
using System.Collections.Generic;

namespace MiniStrava.Models.Mobile
{
    public class MobileCreateActivityRequest
    {
        public string? Name { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }

        public double? DistanceKm { get; set; }
        public string? Note { get; set; }

        public string? Type { get; set; }

        public string? PaceText { get; set; }
        public string? SpeedText { get; set; }

        public string? PhotoBase64 { get; set; }

        public List<MobileGpsPoint> Track { get; set; } = new();
    }

    public class MobileGpsPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
