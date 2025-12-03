using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface IDriverService
    {
        DriverDto GetById(int id);
        IEnumerable<DriverDto> GetAll();
        int CreateDriver(CreateDriverDto dto);
        bool UpdateDriver(int id, UpdateDriverDto dto);
        bool DeleteDriver(int id);
    }
}
