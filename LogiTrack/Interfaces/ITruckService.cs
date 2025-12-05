using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ITruckService
    {
        IEnumerable<TruckDto> GetAll();
        TruckDto GetById(int id);
        int CreateTruck(CreateTruckDto dto);
        void UpdateTruck(int id, UpdateTruckDto dto);
        void DeleteTruck(int id);
    }
}
