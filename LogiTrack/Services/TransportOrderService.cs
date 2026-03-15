using AutoMapper;
using LogiTrack.Authorization;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LogiTrack.Services
{
    public class TransportOrderService : ITransportOrderService
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<TransportOrderService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public TransportOrderService(LogiTrackDbContext dbContext, IMapper mapper, ILogger<TransportOrderService> logger,
            IAuthorizationService authorizationService, IUserContextService contextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = contextService;
        }

        public PageResult<TransportOrderDto> GetAllTransportOrders(int companyId, TransportOrderQuery query)
        {
            var company = GetCompanyById(companyId);

            var baseQuery = _dbContext
                .Orders
                .Where(x => query.SearchPhrase == null 
                ||(x.OrderName.ToLower().Contains(query.SearchPhrase.ToLower())
                || x.Description.ToLower().Contains(query.SearchPhrase.ToLower())));

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                var columnsSelectors = new Dictionary<string, Expression<Func<TransportOrder, object>>>
                {
                    { nameof(TransportOrder.OrderName), x => x.OrderName },
                    { nameof(TransportOrder.Description), r => r.Description }
                };

                var selectedColumns = columnsSelectors[query.SortBy];

                baseQuery = query.SortDirection == SortDirection.ASC
                    ? baseQuery.OrderBy(selectedColumns)
                    : baseQuery.OrderByDescending(selectedColumns);
            }

            var transportOrders = baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();

            var transportOrdersDtos = _mapper.Map<List<TransportOrderDto>>(transportOrders);

            var result = new PageResult<TransportOrderDto>(transportOrdersDtos, totalItemsCount, query.PageSize, query.PageNumber);

            return result;
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
            transportOrder.CreatedById = _userContextService.GetUserId;
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

            var authorizationResult =
                _authorizationService.AuthorizeAsync(_userContextService.User, transportOrder, new ResourcerceOperationRequirement(ResourceOperation.Update)).Result;

            if(!authorizationResult.Succeeded)
                throw new ForbidException("You don't have permission to update this transport order");

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

            var authorizationResult =
                _authorizationService.AuthorizeAsync(_userContextService.User, transportOrder, new ResourcerceOperationRequirement(ResourceOperation.Delete)).Result;

            if(!authorizationResult.Succeeded)
                throw new ForbidException("You don't have permission to delete this transport order");

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
