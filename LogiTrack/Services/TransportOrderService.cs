using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Services
{
    public class TransportOrderService : ITransportOrderService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;

        public TransportOrderService(LogiTrackDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public IEnumerable<TransportOrderDto> GetAllTransportOrders()
        {
            var transportOrders = _dbContext
                .Orders
                .ToList();

            var transportOrdersDtos = _mapper.Map<List<TransportOrderDto>>(transportOrders);

            return transportOrdersDtos;
        }

        public TransportOrderDto GetTransportOrderById(int id)
        {
            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id);

            if(transportOrder is null) return null;

            var result = _mapper.Map<TransportOrderDto>(transportOrder);

            return result;
        }

        public int CreateTransportOrder(CreateTransportOrderDto dto)
        {
            var transportOrder = _mapper.Map<TransportOrder>(dto);

            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            return transportOrder.Id;
        }

        public bool UpdateTransportOrder(int id, UpdateTransportOrder dto)
        {
            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id);

            if (transportOrder is null) return false;

            transportOrder.OrderName = dto.OrderName;
            transportOrder.Description = dto.Description;
            transportOrder.Price = dto.Price;
            transportOrder.CompanyId = dto.CompanyId;
            transportOrder.PickupAddressId = dto.PickupAddressId;
            transportOrder.DeliveryAddressId = dto.DeliveryAddressId;
            transportOrder.DriverId = dto.DriverId;
            transportOrder.TruckId = dto.TruckId;

            _dbContext.SaveChanges();

            return true;
        }

        public bool DeleteTransportOrder(int id)
        {
            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id);

            if (transportOrder is null) return false;

            _dbContext.Orders.Remove(transportOrder);
            _dbContext.SaveChanges();

            return true;
        }
    }
}
