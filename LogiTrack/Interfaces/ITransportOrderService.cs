using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ITransportOrderService
    {
        TransportOrderDto GetTransportOrderById(int id);
        IEnumerable<TransportOrderDto> GetAllTransportOrders();
        int CreateTransportOrder(CreateTransportOrderDto dto);
        void UpdateTransportOrder(int id, UpdateTransportOrder dto);
        void DeleteTransportOrder(int id);
    }
}
