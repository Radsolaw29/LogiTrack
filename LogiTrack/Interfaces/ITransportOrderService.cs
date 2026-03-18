using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ITransportOrderService
    {
        TransportOrderDto GetTransportOrderById(int companyId, int id);
        PageResult<TransportOrderDto> GetAllTransportOrders(int companyId, TransportOrderQuery query);
        int CreateTransportOrder(int companyId, CreateTransportOrderDto dto);
        void UpdateTransportOrder(int companyId, int id, UpdateTransportOrder dto);
        void DeleteTransportOrder(int companyId, int id);
        void DeleteAllTraansportOrders(int companyId);
    }
}