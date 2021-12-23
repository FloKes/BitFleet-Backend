namespace BitFleet.Contracts.V1.Requests.CreateRequests
{
    public class CreateRideRequest
    {
        public string StartLocation { get; set; }

        public string EndLocation { get; set; }

        public string CarId { get; set; }
    }
}