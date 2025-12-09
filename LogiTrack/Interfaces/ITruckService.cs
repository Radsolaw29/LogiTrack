using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ITruckService
    {
        IEnumerable<TruckDto> GetAll(int companyId);
        TruckDto GetById(int companyId, int id);
        int CreateTruck(int companyId, CreateTruckDto dto);
        void UpdateTruck(int companyId, int id, UpdateTruckDto dto);
        void DeleteTruck(int companyId, int id);
        void DeleteAllTrucks(int companyId);
    }
}
