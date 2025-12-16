using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

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

        public PageResult<AddressDto> GetAll(AddressQuery query)
        {
            var baseQuery = _dbContext
                .Addresses
                .Where(x => query.SearchPhrase == null ||
                (x.Country.ToLower().Contains(query.SearchPhrase.ToLower())
                || x.City.ToLower().Contains(query.SearchPhrase.ToLower())
                || x.Street.ToLower().Contains(query.SearchPhrase.ToLower())));

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                var columnsSelectors = new Dictionary<string, Expression<Func<Address, object>>>
                {
                    { nameof(Address.Country), r => r.Country },
                    { nameof(Address.City), r => r.City },
                    { nameof(Address.Street) , r => r.Street } 
                };

                var selectedColumn = columnsSelectors[query.SortBy];

                baseQuery = query.SortDirection == SortDirection.ASC 
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }


            var addresses = baseQuery.Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();


            var addressesDtos = _mapper.Map<List<AddressDto>>(addresses);

            var result = new PageResult<AddressDto>(addressesDtos, totalItemsCount, query.PageSize, query.PageNumber);

            return result;
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
