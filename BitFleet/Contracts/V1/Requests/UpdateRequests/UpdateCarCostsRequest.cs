namespace BitFleet.Contracts.V1.Requests.UpdateRequests
{
    public class UpdateCarCostsRequest
    {
        public float? ServiceCosts { get; set; }

        public float? FuelCosts { get; set; }

        public float? ServiceCostsToAdd { get; set; }

        public float? FuelCostsToAdd  { get; set; }
    }
}