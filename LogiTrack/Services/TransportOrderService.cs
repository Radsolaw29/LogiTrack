using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public IEnumerable<TransportOrderDto> GetAllTransportOrders(int companyId)
        {
            var company = GetCompanyById(companyId);

            var transportOrdersDtos = _mapper.Map<List<TransportOrderDto>>(company.TransportOrders);

            return transportOrdersDtos;
        }

        public TransportOrderDto GetTransportOrderById(int companyId, int id)
        {
            var company = GetCompanyById(companyId);

            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id);

            if(transportOrder is null || transportOrder.CompanyId != companyId)
                throw new NotFoundException("Transport order not found");

            var result = _mapper.Map<TransportOrderDto>(transportOrder);

            return result;
        }

        public int CreateTransportOrder(int companyId, CreateTransportOrderDto dto)
        {
            var company = GetCompanyById(companyId);

            var transportOrder = _mapper.Map<TransportOrder>(dto);

            transportOrder.CompanyId = companyId;

            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            return transportOrder.Id;
        }

        public void UpdateTransportOrder(int companyId, int id, UpdateTransportOrder dto)
        {
            var company = GetCompanyById(companyId);

            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id && x.CompanyId == companyId);

            if (transportOrder is null)
                throw new NotFoundException("Transport order not found");

            transportOrder.OrderName = dto.OrderName;
            transportOrder.Description = dto.Description;
            transportOrder.Price = dto.Price;
            transportOrder.PickupAddressId = dto.PickupAddressId;
            transportOrder.DeliveryAddressId = dto.DeliveryAddressId;
            transportOrder.DriverId = dto.DriverId;
            transportOrder.TruckId = dto.TruckId;

            _dbContext.SaveChanges();
        }

        public void DeleteTransportOrder(int companyId, int id)
        {
            _logger.LogWarning($"Transport order with id: {id} Delete action invoked", id);

            var company = GetCompanyById(companyId);

            var transportOrder = _dbContext
                .Orders
                .FirstOrDefault(x => x.Id == id && x.CompanyId == companyId);

            if (transportOrder is null)
                throw new NotFoundException("Transport order not found");

            _dbContext.Orders.Remove(transportOrder);
            _dbContext.SaveChanges();
        }

        public void DeleteAllTraansportOrders(int companyId)
        {
            var company = GetCompanyById(companyId);

            _dbContext.Orders.RemoveRange(company.TransportOrders);
            _dbContext.SaveChanges();
        }

        private Company GetCompanyById(int companyId)
        {
            var company = _dbContext
                .Companies
                .Include(x => x.TransportOrders)
                .FirstOrDefault(x => x.Id == companyId);

            if(company is null)
                throw new NotFoundException("Company not found");

            return company;
        }

    }
}
