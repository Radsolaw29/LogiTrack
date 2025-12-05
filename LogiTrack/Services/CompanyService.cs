using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;

        public CompanyService(LogiTrackDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
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

            company.Name = dto.Name;
            company.Description = dto.Description;
            company.TaxNumber = dto.TaxNumber;
            company.ContactEmail = dto.ContactEmail;

            _dbContext.SaveChanges();
        }

        public void DeleteCompany(int id)
        {
            var company = _dbContext
                .Companies
                .FirstOrDefault(r => r.Id == id);

            if (company is null) 
                throw new NotFoundException("Company not found");

            _dbContext.Companies.Remove(company);
            _dbContext.SaveChanges();
        }

    }
}
