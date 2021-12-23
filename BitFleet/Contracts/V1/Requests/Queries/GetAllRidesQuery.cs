using System;
using Microsoft.AspNetCore.Mvc;

namespace BitFleet.Contracts.V1.Requests.Queries
{
    public class GetAllRidesQuery
    {
        [FromQuery(Name = "carId")]
        public Guid CarId { get; set; }

        [FromQuery(Name = "userId")] 
        public string UserId { get; set; }

        [FromQuery(Name = "isActive")] 
        public bool? IsActive { get; set; }
    }
}