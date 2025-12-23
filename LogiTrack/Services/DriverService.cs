using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

namespace LogiTrack.Services
{
    public class DriverService : IDriverService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DriverService> _logger;

        public DriverService(LogiTrackDbContext dbContext, IMapper mapper, ILogger<DriverService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public DriverDto GetById(int companyId, int id)
        {
            var company = GetCompanyById(companyId);

            var driver = _dbContext
                .Drivers
                .FirstOrDefault(x => x.Id == id);

            if (driver is null || driver.CompanyId != companyId)
                throw new NotFoundException("Driver not found");

            var result = _mapper.Map<DriverDto>(driver);

            return result;
        }

        public PageResult<DriverDto> GetAll(int companyId, DriverQuery query)
        {
            var company = GetCompanyById(companyId);

            var baseQuery = _dbContext
                .Drivers
                .Where(x => x.CompanyId == companyId)
                .Where(x => string.IsNullOrEmpty(query.SearchPhrase)
                || (x.FirstName.ToLower().Contains(query.SearchPhrase.ToLower())
                || x.LastName.ToLower().Contains(query.SearchPhrase.ToLower())
                || x.ContactEmail.ToLower().Contains(query.SearchPhrase.ToLower())));

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                var columnsSelectors = new Dictionary<string, Expression<Func<Driver, object>>>{

                    { nameof(Driver.FirstName), x => x.FirstName },
                    { nameof(Driver.LastName), x => x.LastName },
                    { nameof(Driver.LicenseDriving), x => x.LicenseDriving },
                    { nameof(Driver.ContactEmail), x => x.ContactEmail }

                };

                var selectedColumn = columnsSelectors[query.SortBy];

                baseQuery = query.SortDirection == SortDirection.ASC 
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            var drivers = baseQuery
                .Skip(query.PageSize * (query.PageNumber -1))
                .Take(query.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();

            var driversDtos = _mapper.Map<List<DriverDto>>(drivers);

            var result = new PageResult<DriverDto>(driversDtos, totalItemsCount, query.PageSize, query.PageNumber);

            return result;
        }

        public int CreateDriver(int companyId, CreateDriverDto dto)
        {
            var company = GetCompanyById(companyId);

            var driver = _mapper.Map<Driver>(dto);

            driver.CompanyId = companyId;

            _dbContext.Drivers.Add(driver);
            _dbContext.SaveChanges();

            return driver.Id;
        }

        public void UpdateDriver(int companyId, int id, UpdateDriverDto dto)
        {
            var company = GetCompanyById(companyId);

            var driver = _dbContext
                .Drivers
                .FirstOrDefault(x => x.Id == id && x.CompanyId == companyId);

            if (driver is null)
                throw new NotFoundException("Driver not found");

            driver.FirstName = dto.FirstName;
            driver.LastName = dto.LastName;
            driver.PersonalNumber = dto.PersonalNumber;
            driver.DateOfBirth = dto.DateOfBirth;
            driver.LicenseDriving = dto.LicenseDriving;
            driver.PhoneNumber = dto.PhoneNumber;
            driver.ContactEmail = dto.ContactEmail;

            _dbContext.SaveChanges();
        }

        public void DeleteDriver(int companyId, int id)
        {
            _logger.LogWarning($"Driver with id: {id}, Delete action invoked", id);

            var company = GetCompanyById(companyId);

            var driver = _dbContext
                .Drivers
                .FirstOrDefault(x => x.Id == id && x.CompanyId == companyId);

            if (driver is null)
                throw new NotFoundException("Driver not found");

            _dbContext.Drivers.Remove(driver);
            _dbContext.SaveChanges();
        }

        public void DeleteAllDrivers(int companyId)
        {
            var company = GetCompanyById(companyId);

            _dbContext.Drivers.RemoveRange(company.Drivers);
            _dbContext.SaveChanges();
        }

        private Company GetCompanyById(int companyId)
        {
            var company = _dbContext
                .Companies
                .Include(x => x.Drivers)
                .FirstOrDefault(x => x.Id == companyId);

            if(company is null)
                throw new NotFoundException("Company not found");

            return company;
        }
    }
}
