using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Services
{
    public class TruckService : ITruckService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<TruckService> _logger;

        public TruckService(LogiTrackDbContext dbContext, IMapper mapper, ILogger<TruckService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public TruckDto GetById(int companyId, int id)
        {
            var company = GetCompanyById(companyId);

            var truck = _dbContext
                .Trucks
                .FirstOrDefault(x => x.Id == id);

            if (truck is null || truck.CompanyId != companyId)
                throw new NotFoundException("Truck not found");

            var result = _mapper.Map<TruckDto>(truck);

            return result;
        }

        public IEnumerable<TruckDto> GetAll(int companyId)
        {
            var company = GetCompanyById(companyId);

            var tracksDtos = _mapper.Map<List<TruckDto>>(company.Trucks);

            return tracksDtos;
        }

        public int CreateTruck(int companyId, CreateTruckDto dto)
        {
            var company = GetCompanyById(companyId);

            var truck = _mapper.Map<Truck>(dto);

            truck.CompanyId = companyId;

            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            return truck.Id;
        }

        public void UpdateTruck(int companyId, int id, UpdateTruckDto dto)
        {
            var company = GetCompanyById(companyId);

            var truck = _dbContext
                .Trucks
                .FirstOrDefault(x => x.Id == id && x.CompanyId == companyId);

            if (truck is null)
                throw new NotFoundException("Truck not found");

            truck.RegistrationNumber = dto.RegistrationNumber;
            truck.Brand = dto.Brand;
            truck.Model = dto.Model;
            truck.Year = dto.Year;
            truck.Mileage = dto.Mileage;
            truck.CapacityTons = dto.CapacityTons;
            truck.Type = dto.Type;

            _dbContext.SaveChanges();
        }

        public void DeleteTruck(int companyId, int id)
        {
            _logger.LogWarning($"Truck with id: {id} Delete action invoked", id);

            var company = GetCompanyById(companyId);

            var truck = _dbContext
                .Trucks
                .FirstOrDefault(x => x.Id == id && x.CompanyId == companyId);

            if (truck is null)
                throw new NotFoundException("Truck not found");

            _dbContext.Trucks.Remove(truck);
            _dbContext.SaveChanges();
        }

        public void DeleteAllTrucks(int companyId)
        {
            var company = GetCompanyById(companyId);

            _dbContext.Trucks.RemoveRange(company.Trucks);
            _dbContext.SaveChanges();
        }

        private Company GetCompanyById(int companyId)
        {
            var company = _dbContext
                .Companies
                .Include(x => x.Trucks)
                .FirstOrDefault(x => x.Id == companyId);

            if (company is null)
                throw new NotFoundException("Company not found");

            return company;
        }
    }
}
