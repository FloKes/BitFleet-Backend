using System.Collections.Generic;

namespace BitFleet.Contracts.V1.Responses.AuthenticationResponses
{
    public class AuthFailedResponse
    {
        public IEnumerable<string> Errors { get; set; }
    }
}