using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Models;

namespace LogiTrack.Services
{
    public class AddressService : IAddressService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;

        public AddressService(LogiTrackDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public AddressDto GetById(int id)
        {
            var address = _dbContext
               .Addresses
               .FirstOrDefault(x => x.Id == id);

            if (address is null) return null;

            var result = _mapper.Map<AddressDto>(address);

            return result;
        }

        public IEnumerable<AddressDto> GetAll()
        {
            var addresses = _dbContext
                .Addresses
                .ToList();

            var addressesDtos = _mapper.Map<List<AddressDto>>(addresses);

            return addressesDtos;
        }

        public int CreateAddress(CreateAddressDto dto)
        {
            var address = _mapper.Map<Address>(dto);

            _dbContext.Addresses.Add(address);
            _dbContext.SaveChanges();

            return address.Id;
        }

        public bool UpdateAddress(int id, UpdateAddressDto dto)
        {
            var address = _dbContext
                .Addresses
                .FirstOrDefault(x => x.Id == id);

            if (address is null) return false;

            address.Country = dto.Country;
            address.City = dto.City;
            address.Street = dto.Street;
            address.PostalCode = dto.PostalCode;

            _dbContext.SaveChanges();

            return true;
        }

        public bool DeleteAdderss(int id)
        {
            var address = _dbContext
                .Addresses
                .FirstOrDefault(x => x.Id == id);

            if(address is null) return false;

            _dbContext.Addresses.Remove(address);
            _dbContext.SaveChanges();

            return true;
        }

    }
}
