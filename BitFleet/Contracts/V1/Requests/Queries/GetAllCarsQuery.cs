using Microsoft.AspNetCore.Mvc;

namespace BitFleet.Contracts.V1.Requests.Queries
{
    public class GetAllCarsQuery
    {
        [FromQuery(Name = "isOnRide")]
        public bool? IsOnRide { get; set; }

        [FromQuery(Name = "isOnService")]
        public bool? IsOnService { get; set; }

        [FromQuery(Name = "brand")]
        public string Brand { get; set; }


    }
}