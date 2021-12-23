using System;

namespace BitFleet.Contracts.V1.Responses
{
    public class CarCostsResponse
    {
        public Guid Id { get; set; }

        public Guid CarId { get; set; }

        public float ServiceCosts { get; set; }

        public float FuelCosts { get; set; }

        public float TotalCosts{ get; set; }

        public float ServiceCostsPerKm { get; set; }

        public float FuelCostsPerKm { get; set; }

        public float TotalCostsPerKm { get; set; }
    }
}