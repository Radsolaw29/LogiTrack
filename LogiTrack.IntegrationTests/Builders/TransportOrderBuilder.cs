using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class TransportOrderBuilder
    {

        private TransportOrder _order = new TransportOrder()
        {
            Id = 1,
            OrderName = "Transport order test",
            Description = "Test description",
            Price = 2200,
            PickupAddressId = 1,
            DeliveryAddressId = 2,
            DriverId = 1,
            TruckId = 1,
            CompanyId = 1,
            CreatedById = 1
        };

        public TransportOrder Build() => _order;

        public TransportOrderBuilder WithId(int id)
        {
            _order.Id = id;
            return this;
        }

        public TransportOrderBuilder WithName(string name)
        {
            _order.OrderName = name;
            return this;
        }
    }
}
