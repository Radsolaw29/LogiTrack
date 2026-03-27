using LogiTrack.Entities;
using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class UpdateTransportOrderBuilder
    {

        private UpdateTransportOrder _dto = new UpdateTransportOrder()
        {
            OrderName = "Transport order",
            Description = "Description",
            Price = 3000,
            CompanyId = 1,
            PickupAddressId = 1,
            DeliveryAddressId = 2,
            DriverId = 1,
            TruckId = 1
        };

        public UpdateTransportOrder Build() => _dto;

        public UpdateTransportOrderBuilder WithName(string name)
        {
            _dto.OrderName = name;
            return this;
        }
    }
}
