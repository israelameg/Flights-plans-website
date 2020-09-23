using Dapper;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Newtonsoft.Json;
using FlightControlWeb.wwwroot;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using FlightControlWeb.Interface;

namespace FlightControlWeb.Components
{
    public sealed class FlightsSQL
    {
        //singleton
        private static FlightsSQL instance = null;
        public static FlightsSQL Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FlightsSQL();
                }
                return instance;
            }
        }

        private FlightsSQL() { }
        ~FlightsSQL() { }
        static Dictionary<string, Plane> internalFlights = new Dictionary<string, Plane>();
        static Dictionary<string, Plane> externalFlights = new Dictionary<string, Plane>();
        static Dictionary<string, Server> servers = new Dictionary<string, Server>();
        static Dictionary<string, string> flightToServer = new Dictionary<string, string>();

        //function loads all internal flights, according to time.
        public List<Plane> LoadInternalFlights(DateTime relativeTo)
        {
            List<Plane> relativeFlights = new List<Plane>();
            lock (internalFlights)
            {
                List<Plane> flights = internalFlights.Values.ToList();
                if (flights.Count == 0)
                {
                    return relativeFlights;
                }
                addPlanesToRelativeList(flights, relativeFlights, relativeTo);

            }

            return relativeFlights;

        }
        //method add planes from list to list, relative to time.
        private void addPlanesToRelativeList(List<Plane> flights, List<Plane> relativeFlights, DateTime relativeTo)
        {
            foreach (var p in flights)
            {
                var t = p.CalculatePosition(relativeTo);
                if (t != null)
                {
                    relativeFlights.Add(p);
                }
            }
        }
        //method adds flights from plane list to relativeFlights flights list, according to time.
        private void addPlanesToFlightList(List<Plane> flights, List<Flight> relativeFlights, DateTime relativeTo)
        {
            foreach (var p in flights)
            {
                var t = p.CalculatePosition(relativeTo);
                if (t != null)
                {
                    relativeFlights.Add(Flight.ConvertPlaneToFlight(p, relativeTo));
                }
            }
        }
        //method loads all external flights relative to time.
        public List<Flight> LoadExternalFlights(DateTime relativeTo)
        {
            var relativeFlights = new List<Flight>();
            lock (externalFlights)
            {
                var flights = externalFlights.Values.ToList();
                if (flights.Count == 0)
                {
                    return relativeFlights;
                }
                addPlanesToFlightList(flights, relativeFlights, relativeTo);

            }


            return relativeFlights;
        }
        //method get list of flights from given server.
        private async Task<string> GetFromServer(Server server, string strRequest)
        {
            WebRequest request = WebRequest.Create(strRequest);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = await request.GetResponseAsync();

            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                var responseFromServer = await reader.ReadToEndAsync();
                response.Close();
                return responseFromServer;
            }

        }

        //method get list of flights from all registered servers.
        private async Task<IEnumerable<Flight>> GetFromServersValues(List<Server> servs, DateTime relativeTo)
        {
            IEnumerable<Flight> serialized = null;
            var requestEnd = "/api/Flights?relative_to=" +
                relativeTo.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'") + "&sync_all";
            foreach (var s in servs)
            {
                try
                {
                    var strRequest = s.ServerName + requestEnd;
                    var fromServer = await GetFromServer(s, strRequest);
                    serialized = JsonConvert.DeserializeObject<IEnumerable<Flight>>(fromServer);
                    saveFlightToServer(serialized, s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return serialized;
        }
        // function recieve a list of external flights, and map then to the right server.
        private void saveFlightToServer(IEnumerable<Flight> flights, Server s)
        {
            if(flights == null || s == null)
            {
                return;
            }
            foreach(var flight in flights)
            {
                flightToServer.Add(flight.Flight_Id, s.ServerId);
            }
        }
        //function gets all flights from servers according to time.
        private async Task<List<Flight>> GetFromServers(DateTime relativeTo)
        {
            var flights = new List<Flight>();
            IEnumerable<Flight> serialized = null;
            var requestEnd = "/api/Flights?relative_to=" +
                relativeTo.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'") + "&sync_all";
            var servs = new List<Server>();

            lock (servers)
            {
                servs.AddRange(servers.Values.ToList());
            }
            serialized = await GetFromServersValues(servs, relativeTo);

            if (serialized == null)
            {
                return flights;
            }
            var serializedNotDoubled = new List<Flight>();
            foreach (var f in serialized)
            {
                f.IsExternal = true;
                if ((!internalFlights.ContainsKey(f.Flight_Id)) && (!externalFlights.ContainsKey(f.Flight_Id)))
                {
                    serializedNotDoubled.Add(f);

                }
            }
            flights.AddRange(serializedNotDoubled);

            return flights;
        }


        //function saves external planes to db.
        public void SaveExternalFlights(List<Plane> Planes)
        {
            lock (externalFlights)
            {
                foreach (var p in Planes)
                {
                    addPlaneToDic("external", p);
                }
            }
        }
        //method adds given plane to dictionary according to string.
        private void addPlaneToDic(string dic, Plane p)
        {
            bool isExt = false;
            if(dic == "external")
            {
                isExt = true;
            }
            if (isExt)
            {
                if (!(externalFlights.ContainsKey(p.FlightId)))
                {
                    externalFlights.Add(p.FlightId, p);
                }
            }
            else
            {
                if (!(internalFlights.ContainsKey(p.FlightId)))
                {
                    internalFlights.Add(p.FlightId, p);
                }
            }


        }
        //method saves external planes to db.
        public void SaveInternalFlights(List<Plane> Planes)
        {
            lock (internalFlights)
            {
                foreach (var p in Planes)
                {
                    addPlaneToDic("internal", p);
                }
            }

        }
        //method gets plane by given id.
        public async Task<Plane> GetPlaneById(string id)
        {
            lock (externalFlights)
            {
                if (externalFlights.ContainsKey(id))
                {
                    return externalFlights[id];
                }
            }
            lock (internalFlights)
            {
                if (internalFlights.ContainsKey(id))
                {
                    return internalFlights[id];
                }
            }
            if (!flightToServer.ContainsKey(id))
            {
                return null;
            }
            FlightPlan flightPlan = new FlightPlan();
            var server = servers[flightToServer[id]];
            Plane p = null;
            if (server == null)
            {
                p = new Plane(flightPlan, true, id);
                return p;
            }
            var strReq = server.ServerName + "/api/FlightPlan/" + id;
            var fromServer = await GetFromServer(server, strReq);
            try
            {
                flightPlan = JsonConvert.DeserializeObject<FlightPlan>(fromServer);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("got it");

            p = new Plane(flightPlan, true, id);
            return p;
        }


        //method returns all servers.
        public List<Server> LoadServers()
        {
            List<Server> serverList = new List<Server>();
            lock (servers)
            {
                serverList = servers.Values.ToList();
            }
            return serverList;

        }
        //method adds servers to dictionary.
        public void AddServer(Server s)
        {
            if (s == null)
            {
                return;
            }
            lock (servers)
            {
                if (!servers.ContainsKey(s.ServerId))
                {
                    servers.Add(s.ServerId, s);
                }
            }
        }

        /* Delete plane from the list */
        public void DeletePlane(string id)
        {
            if (id == null)
            {
                return;
            }
            bool toRet = false;
            lock (internalFlights)
            {
                if (internalFlights.ContainsKey(id))
                {
                    internalFlights.Remove(id);
                    toRet = true;
                }
            }
            if (toRet)
            {
                return;
            }
            lock (externalFlights)
            {
                if (externalFlights.ContainsKey(id))
                {
                    externalFlights.Remove(id);

                }
            }

        }



        /* Delete server from the list */
        public void DeleteServer(string id)
        {
            if (id == null)
            {
                return;
            }
            if (servers.ContainsKey(id))
            {
                lock (servers)
                {
                    servers.Remove(id);

                }

            }
        }
        //method loads all flights relative to time, from internal, and if syncall - also from external and servers.
        public async Task<List<Flight>> LoadAllFlights(bool syncAll, DateTime relativeTo)
        {

            List<Plane> internals = LoadInternalFlights(relativeTo);
            var flights = new List<Flight>();
            foreach (var p in internals)
            {
                var flight = Flight.ConvertPlaneToFlight(p, relativeTo);
                if (flight != null)
                {
                    flights.Add(flight);
                }
            }
            if (syncAll)
            {
                var externals = LoadExternalFlights(relativeTo).ToList();
                flights.AddRange(externals);
                var fromServer = await GetFromServers(relativeTo);
                flights.AddRange(fromServer);
            }

            return flights;
        }






    }
}
