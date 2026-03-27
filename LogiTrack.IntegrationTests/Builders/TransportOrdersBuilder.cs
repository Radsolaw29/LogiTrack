using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class TransportOrdersBuilder
    {

        private List<TransportOrder> _orders = new List<TransportOrder>
        {
            new TransportOrder
            {
                Id = 1,
                OrderName = "First order",
                Description = "Description test",
                Price = 1850,
                CompanyId = 1,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1,
                CreatedById = 1
            },

            new TransportOrder
            {
                Id = 2,
                OrderName = "Second order",
                Description = "Description test 2",
                Price = 2850,
                CompanyId = 1,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1
            },

            new TransportOrder
            {
                Id = 3,
                OrderName = "Third order",
                Description = "Description test 3",
                Price = 2250,
                CompanyId = 1,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1
            },
        };

        public List<TransportOrder> Build() => _orders;
    }
}
