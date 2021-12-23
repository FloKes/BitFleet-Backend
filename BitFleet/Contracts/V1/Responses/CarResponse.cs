using System;
using System.Collections.Generic;

namespace BitFleet.Contracts.V1.Responses
{
    public class CarResponse
    {
        public Guid Id { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int ModelYear { get; set; }

        public string FuelType { get; set; }

        public float KilometersPerLiter { get; set; }

        public int Mileage { get; set; }

        public int MileageWhenBought { get; set; }

        public int KilometersNeededBeforeService { get; set; }

        public int KilometersSinceLastService { get; set; }

        public bool NeedsService { get; set; }

        public bool IsOnRide { get; set; }

        public bool IsOnService { get; set; }

        public bool IsScheduledForService { get; set; }
    }
}