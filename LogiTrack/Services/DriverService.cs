using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Models;

namespace LogiTrack.Services
{
    public class DriverService : IDriverService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;

        public DriverService(LogiTrackDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public DriverDto GetById(int id)
        {
            var driver = _dbContext
                .Drivers
                .FirstOrDefault(x => x.Id == id);

            if (driver is null) return null;

            var result = _mapper.Map<DriverDto>(driver);

            return result;
        }

        public IEnumerable<DriverDto> GetAll()
        {
            var drivers = _dbContext
                .Drivers
                .ToList();

            var driversDtos = _mapper.Map<List<DriverDto>>(drivers);

            return driversDtos;
        }

        public int CreateDriver(CreateDriverDto dto)
        {
            var driver = _mapper.Map<Driver>(dto);

            _dbContext.Drivers.Add(driver);
            _dbContext.SaveChanges();

            return driver.Id;
        }

        public bool UpdateDriver(int id, UpdateDriverDto dto)
        {
            var driver = _dbContext
                .Drivers
                .FirstOrDefault(x => x.Id == id);

            if (driver is null) return false;

            driver.FirstName = dto.FirstName;
            driver.LastName = dto.LastName;
            driver.PersonalNumber = dto.PersonalNumber;
            driver.DateOfBirth = dto.DateOfBirth;
            driver.LicenseDriving = dto.LicenseDriving;
            driver.PhoneNumber = dto.PhoneNumber;
            driver.ContactEmail = dto.ContactEmail;

            _dbContext.SaveChanges();

            return true;
        }

        public bool DeleteDriver(int id)
        {
            var driver = _dbContext
                .Drivers
                .FirstOrDefault(x => x.Id == id);

            if (driver is null) return false;

            _dbContext.Drivers.Remove(driver);
            _dbContext.SaveChanges();

            return true;
        }
    }
}
