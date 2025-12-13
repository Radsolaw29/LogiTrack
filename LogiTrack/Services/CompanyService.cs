using AutoMapper;
using LogiTrack.Authorization;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LogiTrack.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public CompanyService(LogiTrackDbContext dbContext, IMapper mapper, ILogger<CompanyService> logger, 
            IAuthorizationService authorizationService, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
        }

        public IEnumerable<CompanyDto> GetAll() 
        {
            var companies = _dbContext
                .Companies
                .Include(c => c.Address)
                .Include(c => c.TransportOrders)
                .Include(c => c.Drivers)
                .Include(c => c.Trucks)
                .ToList();

            var companiesDtos = _mapper.Map<List<CompanyDto>>(companies);

            return companiesDtos;
        }

        public CompanyDto GetById(int id)
        {
            var company = _dbContext
                .Companies
                .Include(c => c.Address)
                .Include(c => c.TransportOrders)
                .Include(c => c.Drivers)
                .Include(c => c.Trucks)
                .FirstOrDefault(r => r.Id == id);

            if(company is null) 
                throw new NotFoundException("Company not found");

            var result = _mapper.Map<CompanyDto>(company);

            return result;
        }

        public int CreateCompany(CreateCompanyDto dto)
        {
            var company = _mapper.Map<Company>(dto);

            company.CreatedById = _userContextService.GetUserId;
            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            return company.Id;
        }

        public void UpdateCompany(int id, UpdateCompanyDto dto)
        {
            var company = _dbContext
                .Companies
                .FirstOrDefault(r => r.Id == id);

            if(company is null) 
                throw new NotFoundException("Company not found");

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, company, new ResourcerceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authorizationResult.Succeeded)
                throw new ForbidException("You do not have permission to access this resource.");

            company.Name = dto.Name;
            company.Description = dto.Description;
            company.TaxNumber = dto.TaxNumber;
            company.ContactEmail = dto.ContactEmail;

            _dbContext.SaveChanges();
        }

        public void DeleteCompany(int id)
        {
            _logger.LogWarning($"Company with id: {id} Delete action invoked", id);

            var company = _dbContext
                .Companies
                .FirstOrDefault(r => r.Id == id);

            if (company is null) 
                throw new NotFoundException("Company not found");

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, company, new ResourcerceOperationRequirement(ResourceOperation.Delete)).Result;

            if (!authorizationResult.Succeeded)
                throw new ForbidException("You do not have permission to access this resource.");

            _dbContext.Companies.Remove(company);
            _dbContext.SaveChanges();
        }

    }
}
