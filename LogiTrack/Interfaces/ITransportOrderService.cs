using LogiTrack.Models;

namespace LogiTrack.Interfaces
{
    public interface ITransportOrderService
    {
        TransportOrderDto GetTransportOrderById(int id);
        IEnumerable<TransportOrderDto> GetAllTransportOrders();
        int CreateTransportOrder(CreateTransportOrderDto dto);
        bool UpdateTransportOrder(int id, UpdateTransportOrder dto);
        bool DeleteTransportOrder(int id);
    }
}
