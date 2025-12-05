using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface IDriverService
    {
        DriverDto GetById(int id);
        IEnumerable<DriverDto> GetAll();
        int CreateDriver(CreateDriverDto dto);
        void UpdateDriver(int id, UpdateDriverDto dto);
        void DeleteDriver(int id);
    }
}
