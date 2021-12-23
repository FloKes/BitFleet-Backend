using AutoMapper;
using BitFleet.Contracts.V1.Requests.Queries;
using BitFleet.Filters;

namespace BitFleet.Mapping
{
    public class RequestToDomainProfile : Profile
    {
        public RequestToDomainProfile()
        {
            CreateMap<GetAllCarsQuery, GetAllCarsFilter>();
            CreateMap<GetAllRidesQuery, GetAllRidesFilter>();
        }
    }
}