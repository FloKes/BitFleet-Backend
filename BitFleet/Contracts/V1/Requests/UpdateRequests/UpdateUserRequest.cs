namespace BitFleet.Contracts.V1.Requests.UpdateRequests
{
    public class UpdateUserRequest
    {
        public string UserName { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}