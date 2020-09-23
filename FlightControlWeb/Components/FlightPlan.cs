using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Components
{
    public class FlightPlan
    {
        public FlightPlan() { }
        public void SetParams(int pass, string company, initial_location init, Segment[] s)
        {
            Passengers = pass;
            Company_name = company;
            Initial_location = init;
            Segments = s;
        }
        [JsonProperty("initial_location")]
        public initial_location Initial_location { get; set; }
        //public string Name { get; set; }
        [JsonProperty("passengers")]
        public int Passengers { get; set; }
        [JsonProperty("company_name")]
        public string Company_name { get; set; }

        //public initial_location Initial_location { get; set; }
        [JsonProperty("segments")]
        public Segment[] Segments { get; set; }
    }
    public class initial_location
    {
        public initial_location() { }
        public void SetParams(double lat, double longt, string dt)
        {
            Latitude = lat;
            Longitude = longt;
            Date_Time = dt;
        }
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("date_time")]
        public string Date_Time { get; set; }

    }

    public class Segment
    {
        public Segment() { }
        public void SetParams(double lat, double longt,int ts)
        {
            Latitude = lat;
            Longitude = longt;
            Timespan_Seconds = ts;
        }
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("timespan_seconds")]
        public int Timespan_Seconds { get; set; }
    }

}
