namespace BitFleet.Contracts.V1.Requests.CreateRequests
{
    public class CreateCarRequest
    {
        public string Brand { get; set; }

        public string Model { get; set; }

        public int ModelYear { get; set; }

        public string FuelType { get; set; }

        public float KilometersPerLiter { get; set; }

        public int MileageWhenBought { get; set; }

        public int KilometersNeededBeforeService { get; set; }
    }
}