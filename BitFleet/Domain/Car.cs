using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BitFleet.Services.DTOs;


namespace BitFleet.Domain
{
    public class Car
    {
        [Key] public Guid Id { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int ModelYear { get; set; }

        public string FuelType { get; set; }


        //needed for calculating Cost per Kilometer

        public int MileageWhenBought { get; set; }

        public int Mileage { get; set; }

        public float KilometersPerLiter { get; set; }

        public bool IsOnService { get; set; }

        public bool IsScheduledForService { get; set; }

        public bool NeedsService { get; set; }

        public int KilometersNeededBeforeService { get; set; }

        public int KilometersSinceLastService { get; set; }

        public bool IsOnRide { get; set; }

        public CarCosts CarCosts { get; set; }

        public virtual ICollection<Ride> Rides { get; set; }

        public virtual ICollection<Malfunction> Malfunctions { get; set; }

        public virtual ICollection<VehicleService> VehicleServices { get; set; }
    }
}