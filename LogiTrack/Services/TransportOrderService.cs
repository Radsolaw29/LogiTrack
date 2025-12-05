using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Services
{
    public class TransportOrderService : ITransportOrderService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<TransportOrderService> _logger;

        public TransportOrderService(LogiTrackDbContext dbContext, IMapper mapper, ILogger<TransportOrderService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
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

            if(transportOrder is null)
                throw new NotFoundException("Transport order not found");

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

        public void UpdateTransportOrder(int id, UpdateTransportOrder dto)
        {
            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id);

            if (transportOrder is null)
                throw new NotFoundException("Transport order not found");

            transportOrder.OrderName = dto.OrderName;
            transportOrder.Description = dto.Description;
            transportOrder.Price = dto.Price;
            transportOrder.CompanyId = dto.CompanyId;
            transportOrder.PickupAddressId = dto.PickupAddressId;
            transportOrder.DeliveryAddressId = dto.DeliveryAddressId;
            transportOrder.DriverId = dto.DriverId;
            transportOrder.TruckId = dto.TruckId;

            _dbContext.SaveChanges();
        }

        public void DeleteTransportOrder(int id)
        {
            _logger.LogWarning($"Transport order with id: {id} Delete action invoked", id);

            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id);

            if (transportOrder is null)
                throw new NotFoundException("Transport order not found");

            _dbContext.Orders.Remove(transportOrder);
            _dbContext.SaveChanges();
        }
    }
}
