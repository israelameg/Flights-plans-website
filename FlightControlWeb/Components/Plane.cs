using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace FlightControlWeb.Components
{

    public class Flight
    {

        [JsonProperty("flight_id")]
        public string Flight_Id { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("passengers")]
        public int Passengers { get; set; }
        [JsonProperty("company_name")]
        public string Company_Name { get; set; }
        [JsonProperty("date_time")]
        public DateTime Date_Time { get; set; }
        [JsonProperty("is_external")]
        public bool IsExternal { get; set; }


        public Flight() { }
        private Flight(string id, Double longt, Double lat, int pass, string company, DateTime dt, bool ise)
        {
            Flight_Id = id;
            Longitude = longt;
            Latitude = lat;
            Passengers = pass;
            Company_Name = company;
            Date_Time = dt;
            IsExternal = ise;
        }

        public static Flight ConvertPlaneToFlight(Plane p,DateTime dateTime)
        {
            var f = p.CalculatePosition(dateTime);
            if(f == null)
            {
                return null;
            }
            var flight = new Flight(p.FlightId,f[1],f[0],p.Passengers,p.CompanyName,dateTime,p.IsExternal);
            return flight;
        }

    }

    public class Plane
    {


        public string FlightId { get; set; }
        public initial_location PlaneLocation { get; set; }
        public bool IsExternal { get; set; }
        public int Passengers { get; set; }
        public string CompanyName { get; set; }
        public Segment[] Segments { get; set; }
        private DateTime LandingTime { get; set; }

        public Plane(initial_location p, string id, bool ixExt, int pass, string comp, Segment[] s)
        {
            PlaneLocation = p;
            FlightId = id;
            IsExternal = ixExt;
            Passengers = pass;
            CompanyName = comp;
            Segments = s;

        }

        public Plane(FlightPlan flightPlan, bool isExt, string id)
        {
            FlightId = id;
            IsExternal = isExt;

            CompanyName = flightPlan.Company_name;
            Passengers = flightPlan.Passengers;
            Segments = flightPlan.Segments;
            PlaneLocation = flightPlan.Initial_location;
            var landing = DateTime.Parse(PlaneLocation.Date_Time).ToUniversalTime();
            LandingTime = CalculateLandingTime(Segments, landing);
        }

        public Plane Copy()
        {
            var fp = this.ConvertToFlightPlan();
            Plane p = new Plane(fp, this.IsExternal, this.FlightId);
            return p;
        }

        public Double[] CalculatePosition(DateTime dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            var startDate = DateTime.Parse(PlaneLocation.Date_Time).ToUniversalTime();
            if (dateTime < startDate || dateTime >= LandingTime)
            {
                return null;
            }
            if (DateTime.Equals(dateTime, startDate))
            {
                Double[] d  = { this.PlaneLocation.Latitude, this.PlaneLocation.Longitude};
                return d;
            }
            var timeDiff = dateTime.Subtract(startDate);
            int secs = (int)timeDiff.TotalSeconds;

            int totalSecs = 0, index = 0;
            foreach (var s in this.Segments)
            {
                totalSecs += s.Timespan_Seconds;
                if (totalSecs >= secs)
                {
                    break;
                }
                index++;
            }
            double initLat, initLongt;
            double endLat = this.Segments[index].Latitude, endLongt = this.Segments[index].Longitude;
            int secDiff = totalSecs - secs;
            if (index == 0)
            {
                initLat = this.PlaneLocation.Latitude;
                initLongt = this.PlaneLocation.Longitude;
            }
            else
            {
                initLat = this.Segments[index - 1].Latitude;
                initLongt = this.Segments[index - 1].Longitude;
            }
            secDiff = this.Segments[index].Timespan_Seconds - secDiff;
            var latDiff = initLat - endLat;
            var longtDiff = initLongt - endLongt;
            var ts = this.Segments[index].Timespan_Seconds;
            var timeRelation = (1.0 * secDiff) / (1.0 * ts);
            var newLat = (double)initLat - timeRelation * (double)latDiff;
            var newLongt = (double)initLongt - timeRelation * (double)longtDiff;
            Double[] latlng = { newLat, newLongt };
            return latlng;
        }


        public FlightPlan ConvertToFlightPlan()
        {
            var fp = new FlightPlan();
            fp.SetParams(this.Passengers, this.CompanyName, this.PlaneLocation, this.Segments);
            return fp;
        }
        private DateTime CalculateLandingTime(Segment[] s, DateTime startDate)
        {


            DateTime endDate = startDate.AddSeconds(0);
            foreach (var seg in s)
            {
                endDate = endDate.AddSeconds(seg.Timespan_Seconds);
            }

            return endDate;
        }

    }
}