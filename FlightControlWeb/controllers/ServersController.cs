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
using FlightControlWeb.wwwroot;

namespace FlightControlWeb.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServersController : ControllerBase
    {


        // GET: api/Servers
        [HttpGet]
        public JsonResult Get()
        {
            var servers = FlightsSQL.Instance.LoadServers();
            var serializedServers = System.Text.Json.JsonSerializer.Serialize(servers);
            return new JsonResult(serializedServers);
        }


        [HttpPost]
        public void Post([FromBody]Server value)
        {
            if(value == null)
            {
                return;
            }
            if(value.ServerId == null || value.ServerName == null)
            {
                return;
            }
            FlightsSQL.Instance.AddServer(value);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            if(id == null)
            {
                return;
            }
            FlightsSQL.Instance.DeleteServer(id);
        }
    }
}
