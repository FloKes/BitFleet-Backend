using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BitFleet.Extensions
{
    public static class GeneralExtensions
    {
        public static string GetUserId(this HttpContext httpContext)
        {
            if (httpContext.User == null)
            {
                return string.Empty;
            }

            return httpContext.User.Claims.Single(claim => claim.Type == "userId").Value;
        }

    }
}