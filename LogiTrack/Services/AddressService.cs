using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;

namespace LogiTrack.Services
{
    public class AddressService : IAddressService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AddressService> _logger;

        public AddressService(LogiTrackDbContext dbContext, IMapper mapper, ILogger<AddressService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public AddressDto GetById(int id)
        {
            var address = _dbContext
               .Addresses
               .FirstOrDefault(x => x.Id == id);

            if (address is null) 
                throw new NotFoundException("Address not found");

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

        public void UpdateAddress(int id, UpdateAddressDto dto)
        {
            var address = _dbContext
                .Addresses
                .FirstOrDefault(x => x.Id == id);

            if (address is null)
                throw new NotFoundException("Address not found");

            address.Country = dto.Country;
            address.City = dto.City;
            address.Street = dto.Street;
            address.PostalCode = dto.PostalCode;

            _dbContext.SaveChanges();
        }

        public void DeleteAdderss(int id)
        {
            _logger.LogWarning($"Address with id: {id} Delete action invoked", id);

            var address = _dbContext
                .Addresses
                .FirstOrDefault(x => x.Id == id);

            if(address is null)
                throw new NotFoundException("Address not found");

            _dbContext.Addresses.Remove(address);
            _dbContext.SaveChanges();
        }

    }
}
