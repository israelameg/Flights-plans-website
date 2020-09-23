using FlightControlWeb.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Interface
{
    public interface IFlightSql
    {
        public IFlightSql Instancee { get; }
        //Plane GetPlaneById(string id);
        void DeletePlane(string id);
    }
}
