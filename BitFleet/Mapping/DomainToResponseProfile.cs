using AutoMapper;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Domain;
using BitFleet.Services;
using BitFleet.Services.DTOs;

namespace BitFleet.Mapping
{
    public class DomainToResponseProfile : Profile
    {
        public DomainToResponseProfile()
        {
            CreateMap<Car, CarResponse>();

            CreateMap<UserDto, UserResponse>();

            CreateMap<MalfunctionDto, MalfunctionResponse>();

            CreateMap<CarCosts, CarCostsResponse>();

            CreateMap<VehicleService, VehicleServiceResponse>()
                .ForMember(dest => dest.Car,opt =>
                    opt.MapFrom(src => new CarResponse
                    {
                        Brand = src.Car.Brand,
                        Model = src.Car.Model,
                        ModelYear = src.Car.ModelYear,
                        Mileage = src.Car.Mileage,
                        IsOnRide = src.Car.IsOnRide,
                        NeedsService = src.Car.NeedsService,
                        IsOnService = src.Car.IsOnService,
                        IsScheduledForService = src.Car.IsScheduledForService,
                        FuelType = src.Car.FuelType,
                        Id = src.Car.Id
                    }));


            CreateMap<RideDto, RideResponse>()
                .ForMember(dest => dest.Car, opt =>
                    opt.MapFrom(src => new CarResponse
                    {
                        Brand = src.Car.Brand,
                        Model = src.Car.Model,
                        ModelYear = src.Car.ModelYear,
                        Mileage = src.Car.Mileage,
                        IsOnRide = src.Car.IsOnRide,
                        NeedsService = src.Car.NeedsService,
                        IsOnService = src.Car.IsOnService,
                        IsScheduledForService = src.Car.IsScheduledForService,
                        FuelType = src.Car.FuelType,
                        Id = src.Car.Id
                    })).ForMember(dest => dest.User, opt =>
                    opt.MapFrom(src => new UserResponse
                    {
                        FirstName = src.User.FirstName,
                        LastName = src.User.LastName,
                        IsOnRide = src.User.IsOnRide,
                        Role = src.User.Role,
                        Id = src.User.Id,
                        Username = src.User.Username
                    }));
        }
    }
}