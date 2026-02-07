using AutoMapper;
using Castle.Core.Logging;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using LogiTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LogiTrack.Tests.Services
{
    public class TransportOrderServiceTests
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly TransportOrderService _sut;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public TransportOrderServiceTests()
        {

            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TransportOrder, TransportOrderDto>();
                cfg.CreateMap<CreateTransportOrderDto, TransportOrder>();
            });

            _mapper = mapperConfig.CreateMapper();

            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _userContextServiceMock = new Mock<IUserContextService>();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(1);

            _userContextServiceMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var loggerMock = new Mock<ILogger<TransportOrderService>>();

            _sut = new TransportOrderService(_dbContext, _mapper, loggerMock.Object, _authorizationServiceMock.Object, _userContextServiceMock.Object);
        }


    }
}
