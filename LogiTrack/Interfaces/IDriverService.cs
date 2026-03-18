using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface IDriverService
    {
        DriverDto GetById(int companyId, int id);
        PageResult<DriverDto> GetAll(int companyId, DriverQuery query);
        int CreateDriver(int companyId, CreateDriverDto dto);
        void UpdateDriver(int companyId, int id, UpdateDriverDto dto);
        void DeleteDriver(int companyId, int id);
        void DeleteAllDrivers(int companyId);
    }
}