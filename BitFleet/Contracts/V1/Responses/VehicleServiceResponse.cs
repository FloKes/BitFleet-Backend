using System;
using BitFleet.Domain;

namespace BitFleet.Contracts.V1.Responses
{
    public class VehicleServiceResponse
    {
        public Guid Id { get; set; }

        public CarResponse Car { get; set; }

        public string Description { get; set; }

        public float Cost { get; set; }

        public bool IsFinished { get; set; }

        public bool IsActive { get; set; }

        public bool IsScheduled { get; set; }

        public DateTime DateTime { get; set; }
    }
}