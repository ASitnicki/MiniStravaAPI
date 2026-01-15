using MiniStrava.Models.DBObjects;
using System.Text;
using System.Xml.Linq;

namespace MiniStrava.Utils
{
    public static class GpxBuilder
    {
        public static byte[] Build(Activity activity)
        {
            XNamespace ns = "http://www.topografix.com/GPX/1/1";

            var trkseg = new XElement(ns + "trkseg",
                activity.TrackPoints
                    .OrderBy(p => p.Sequence)
                    .Select(p =>
                    {
                        var trkpt = new XElement(ns + "trkpt",
                            new XAttribute("lat", p.Latitude),
                            new XAttribute("lon", p.Longitude),
                            new XElement(ns + "time", p.Timestamp.UtcDateTime.ToString("o"))
                        );

                        if (p.ElevationMeters.HasValue)
                            trkpt.Add(new XElement(ns + "ele", p.ElevationMeters.Value));

                        return trkpt;
                    })
            );

            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(ns + "gpx",
                    new XAttribute("version", "1.1"),
                    new XAttribute("creator", "MiniStravaAPI"),
                    new XElement(ns + "trk",
                        new XElement(ns + "name", activity.Name ?? "Activity"),
                        new XElement(ns + "type", activity.ActivityType.ToString()),
                        trkseg
                    )
                )
            );

            return Encoding.UTF8.GetBytes(doc.ToString(SaveOptions.DisableFormatting));
        }
    }
}
