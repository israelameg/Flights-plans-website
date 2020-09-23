using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using FlightControlWeb.Components;
using System.Text.Json;
using System.Net;
using System.IO;
using System.Text.Json.Serialization;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using Microsoft.Extensions.Caching.Memory;
using FlightControlWeb.Interface;

namespace FlightControlWeb.wwwroot
{
 
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : Controller
    {
        public FlightsController() { }

        IFlightSql myFlightSql = null;
        
        public void SetIFlightSql(IFlightSql mfs)
        {
            myFlightSql = mfs;
        }

        // /api/Flights?relative_to=2019-04-27T19:30:26Z
        [HttpGet]
        public async Task<IEnumerable<Flight>> Get([FromQuery(Name = "relative_to")] DateTime relativeTo)
        {
            DateTime relative_to = relativeTo.ToUniversalTime();
            bool syncAll = Request.Query.ContainsKey("sync_all");
            var all = await FlightsSQL.Instance.LoadAllFlights(syncAll,relative_to);
            return all;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}",Name ="Delete")]
        public void Delete(string id)
        {
            if(id == null)
            {
                return;
            }
            FlightsSQL.Instance.DeletePlane(id);
            if (myFlightSql != null)
            {
                myFlightSql.DeletePlane(id);
            }
        }



        // POST: api/flights/addInternal
        [HttpPost("addInternal")]
        public void Post([FromBody] FlightPlan value)
        {
            //convert flightPlans to Plane and put in list.
            if(value == null)
            {
                return;
            }
            if(value.Company_name == null || value.Initial_location == null || value.Segments == null)
            {
                return;
            }
            List<Plane> planes = new List<Plane>();
            var id = DistCalc.IdGenerator();
            var p = new Plane(value, false, id);
            planes.Add(p);
            FlightsSQL.Instance.SaveInternalFlights(planes);

            return;
        }



        [HttpGet("fromServer")]
        public async Task<string> GetFromServer([FromQuery(Name = "ServerName")] string value)
        {
            try
            {
                WebRequest request = WebRequest.Create(value);
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = await request.GetResponseAsync();

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    var responseFromServer = reader.ReadToEnd();
                    // Display the content.
                    response.Close();
                    string x = JsonConvert.DeserializeObject<string>(responseFromServer);

                    return x;
                }
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }

        }
        

    }
    public class Server
    {
        public string ServerName { get; set; }
        public string ServerId { get; set; }
    }

}
