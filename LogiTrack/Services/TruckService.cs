using AutoMapper;
using LogiTrack.Authorization;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LogiTrack.Services
{
    public class TruckService : ITruckService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<TruckService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public TruckService(LogiTrackDbContext dbContext, IMapper mapper, ILogger<TruckService> logger,
            IAuthorizationService authorizationService, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
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

        public PageResult<TruckDto> GetAll(int companyId, TruckQuery query)
        {
            var company = GetCompanyById(companyId);

            var baseQuery = _dbContext
                .Trucks
                .Where(x => x.CompanyId == companyId)
                .Where(x => string.IsNullOrEmpty(query.SearchPhrase)
                || (x.RegistrationNumber.ToLower().Contains(query.SearchPhrase.ToLower())
                || x.Brand.ToLower().Contains(query.SearchPhrase.ToLower())
                || x.Model.ToLower().Contains(query.SearchPhrase.ToLower())));

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                var columnsSelectors = new Dictionary<string, Expression<Func<Truck, object>>>
                {
                    { nameof(Truck.RegistrationNumber), x => x.RegistrationNumber },
                    { nameof(Truck.Brand), x => x.Brand },
                    { nameof(Truck.Model), x => x.Model }
                };

                var selectedColumn = columnsSelectors[query.SortBy];

                baseQuery = query.SortDirection == SortDirection.ASC
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            var truck = baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();

            var tracksDtos = _mapper.Map<List<TruckDto>>(truck);

            var result = new PageResult<TruckDto>(tracksDtos, totalItemsCount, query.PageSize, query.PageNumber);

            return result;
        }

        public int CreateTruck(int companyId, CreateTruckDto dto)
        {
            var company = GetCompanyById(companyId);

            var truck = _mapper.Map<Truck>(dto);

            truck.CompanyId = companyId;
            truck.CreatedById = _userContextService.GetUserId;
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

            var authorizationResult =
                _authorizationService.AuthorizeAsync(_userContextService.User, truck, new ResourcerceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authorizationResult.Succeeded)
                throw new ForbidException("You don't have permission to update this truck");

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

            var authorizationResult =
                _authorizationService.AuthorizeAsync(_userContextService.User, truck, new ResourcerceOperationRequirement(ResourceOperation.Delete)).Result;

            if (!authorizationResult.Succeeded)
                throw new ForbidException("You don't have permission to delete this truck");

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