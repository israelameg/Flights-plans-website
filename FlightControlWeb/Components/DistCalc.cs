using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Components
{
    public class DistCalc
    {
        public static double GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = Deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = Deg2rad(lon2 - lon1);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double Deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }

        public static double Rad2deg(double deg)
        {
            return deg * 180 / Math.PI;
        }

        public static double AngleFromCoordinate(double lat1, double long1, double lat2, double long2)
        {

            double dLon = (long2 - long1);

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

            double brng = Math.Atan2(y, x);

            brng = (brng * 180 / Math.PI + 360) % 360;

            return brng;
        }

        public static double KmToMs(double KmDist)
        {
            return KmDist * 0.277778;
        }
        public static double[] GetNewCoords(double lat1, double long1, double bearing, double dist)
        {
            double R = 6378.1; //Radius of the Earth
            //double dist = KmToMs(Kmdist);
            double lati1 = Deg2rad(lat1), distR = dist / R;
            double longt1 = Deg2rad(long1);
            double lat2 = Math.Asin(Math.Sin(lati1) * Math.Cos(distR) + Math.Cos(lati1) * Math.Sin(distR) * Math.Cos(bearing));
            double long2 = longt1 + Math.Atan2(Math.Sin(bearing) * Math.Sin(distR) * Math.Cos(lat1),
             Math.Cos(distR) - Math.Sin(lat1) * Math.Sin(lat2));
            double[] newCoords = { Rad2deg(lat2), Rad2deg(long2) };
            return newCoords;
        }
        public static string IdGenerator()
        {
            Random rand = new Random();
            int length = rand.Next(6, 10);
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            char[] Alphanumeric = (letters + letters.ToUpper() + "0123456789").ToCharArray();
            for (int i = 0; i < length; i++)
            {
                builder.Append(Alphanumeric[rand.Next(Alphanumeric.Length)]);
            }

            return builder.ToString();
        }
    }
}
