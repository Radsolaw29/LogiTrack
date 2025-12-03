using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Models;

namespace LogiTrack
{
    public class LogiTrackMappingProfile : Profile
    {

        public LogiTrackMappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(m => m.Country, c => c.MapFrom(s => s.Address.Country))
                .ForMember(m => m.City, c => c.MapFrom(s => s.Address.City))
                .ForMember(m => m.Street, c => c.MapFrom(s => s.Address.Street))
                .ForMember(m => m.PostalCode, c => c.MapFrom(s => s.Address.PostalCode))
                .ForMember(m => m.Orders, c => c.MapFrom(s => s.TransportOrders))
                .ForMember(m => m.Drivers, c => c.MapFrom(s => s.Drivers))
                .ForMember(m => m.Trucks, c => c.MapFrom(s => s.Trucks));

            CreateMap<Address, AddressDto>();
            CreateMap<TransportOrder, TransportOrderDto>();
            CreateMap<Driver, DriverDto>();
            CreateMap<Truck, TruckDto>();

            CreateMap<CreateCompanyDto, Company>()
                .ForMember(r => r.Address, c => c.MapFrom(dto => new Address()
                { Country = dto.Country, City = dto.City, Street = dto.Street, PostalCode = dto.PostalCode }));

            CreateMap<CreateAddressDto, Address>();
            CreateMap<CreateDriverDto, Driver>();
            CreateMap<CreateTruckDto, Truck>();
            CreateMap<CreateTransportOrderDto, TransportOrder>();
        }
    }
}
