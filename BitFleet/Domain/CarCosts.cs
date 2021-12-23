using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitFleet.Domain
{
    public class CarCosts
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        [Required]
        public Car Car { get; set; }

        public float ServiceCosts { get; set; }


        public float FuelCosts { get; set; }

        public float TotalCosts { get; set; }

        public float ServiceCostsPerKm { get; set; }

        public float FuelCostsPerKm { get; set; }

        public float TotalCostsPerKm { get; set; }
    }
}