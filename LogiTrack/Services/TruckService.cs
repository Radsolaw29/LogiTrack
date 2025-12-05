using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;

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

        public TruckDto GetById(int id)
        {
            var truck = _dbContext
                .Trucks
                .FirstOrDefault(x => x.Id == id);

            if (truck is null)
                throw new NotFoundException("Truck not found");

            var result = _mapper.Map<TruckDto>(truck);

            return result;
        }

        public IEnumerable<TruckDto> GetAll()
        {
            var tracks = _dbContext
                .Trucks
                .ToList();

            var tracksDtos = _mapper.Map<List<TruckDto>>(tracks);

            return tracksDtos;
        }

        public int CreateTruck(CreateTruckDto dto)
        {
            var truck = _mapper.Map<Truck>(dto);

            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            return truck.Id;
        }

        public void UpdateTruck(int id, UpdateTruckDto dto)
        {
            var truck = _dbContext
                .Trucks
                .FirstOrDefault(x => x.Id == id);

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

        public void DeleteTruck(int id)
        {
            _logger.LogWarning($"Truck with id: {id} Delete action invoked", id);

            var truck = _dbContext
                .Trucks
                .FirstOrDefault(x => x.Id == id);

            if (truck is null)
                throw new NotFoundException("Truck not found");

            _dbContext.Trucks.Remove(truck);
            _dbContext.SaveChanges();
        }
    }
}
