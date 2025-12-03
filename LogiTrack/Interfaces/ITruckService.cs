using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ITruckService
    {
        IEnumerable<TruckDto> GetAll();
        TruckDto GetById(int id);
        int CreateTruck(CreateTruckDto dto);
        bool UpdateTruck(int id, UpdateTruckDto dto);
        bool DeleteTruck(int id);
    }
}
