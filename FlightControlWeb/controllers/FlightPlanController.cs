using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlightControlWeb.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        [HttpGet("{id}", Name = "Get")]
        public async Task<FlightPlan> Get(string id)
        {
            Plane p = await FlightsSQL.Instance.GetPlaneById(id);
            if (p != null)
            {
                var fp = p.ConvertToFlightPlan();
                return fp;
            }
            return null;

        }

        [HttpPost]
        public void Post([FromBody] FlightPlan value)
        {
            if(value == null)
            {
                return;
            }
            if(value.Company_name == null || value.Initial_location == null || value.Segments == null)
            {
                return;
            }
            Plane p = new Plane(value, true, DistCalc.IdGenerator());
            var ts = new List<Plane>();
            ts.Add(p);
            FlightsSQL.Instance.SaveInternalFlights(ts);
        }
    }
}