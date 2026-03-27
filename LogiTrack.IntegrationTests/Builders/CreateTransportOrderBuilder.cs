using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class CreateTransportOrderBuilder
    {

        private CreateTransportOrderDto _dto = new CreateTransportOrderDto()
        {
            OrderName = "Transport order test",
            Description = "Test description",
            Price = 2200,
            PickupAddressId = 1,
            DeliveryAddressId = 2,
            DriverId = 1,
            TruckId = 1
        };

        public CreateTransportOrderDto Build() => _dto;

    }
}
