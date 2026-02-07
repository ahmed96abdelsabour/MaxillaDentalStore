using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Repositories.Interfaces;
using MaxillaDentalStore.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using Xunit;
using MaxillaDentalStore.Services.Mapping;

namespace MaxillaDentalStore.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context; // Mocked In-Memory context
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            // Setup Mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCartRepository = new Mock<ICartRepository>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockDateTimeProvider = new Mock<IDateTimeProvider>();

            // Setup UnitOfWork properties
            _mockUnitOfWork.Setup(u => u.Carts).Returns(_mockCartRepository.Object);
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);

            // Setup Mapper
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>());
            _mapper = config.CreateMapper();

            // Setup In-Memory DbContext (as Service uses it for some operations or indirect refs)
            // Ideally Service should rely ONLY on Repos, but we have AppDbContext injected.
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestOrderDb")
                .Options;
            _context = new AppDbContext(options);

            // Setup DateTime
            _mockDateTimeProvider.Setup(d => d.UtcNow).Returns(new DateTime(2024, 1, 1, 12, 0, 0));

            // Init Service
            _orderService = new OrderService(_mockUnitOfWork.Object, _mapper, _context, _mockDateTimeProvider.Object);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_Should_Throw_When_CartEmpty()
        {
            // Arrange
            int userId = 1;
            _mockCartRepository.Setup(r => r.GetActiveCartDetailsAsync(userId))
                .ReturnsAsync((Cart)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _orderService.CreateOrderFromCartAsync(userId, new OrderCreateDto()));
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_Should_CreateOrder_When_Valid()
        {
            // Arrange
            int userId = 1;
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>
                {
                    new CartItem 
                    { 
                        ProductId = 10, 
                        Quantity = 2, 
                        Product = new Product { ProductId = 10, Name = "Test Product", Price = 100, Discount = 0, IsActive = true } 
                    }
                }
            };

            _mockCartRepository.Setup(r => r.GetActiveCartDetailsAsync(userId))
                .ReturnsAsync(cart);

            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(o => o.OrderId = 999) // Simulate DB ID generation
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.CreateOrderFromCartAsync(userId, new OrderCreateDto 
            { 
                ShippingAddress = "Cairo", 
                PhoneNumber = "123456" 
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.TotalPrice); // 100 * 2
            Assert.Single(result.OrderItems);
            Assert.Equal("Pending", result.OrderStatus);
            
            // Verify Interactions
            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockCartRepository.Verify(r => r.ClearCart(cart), Times.Once); // Verify Cart Cleared
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_Should_Throw_When_ProductInactive()
        {
            // Arrange
            int userId = 1;
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>
                {
                    new CartItem 
                    { 
                        ProductId = 10, 
                        Quantity = 1, 
                        Product = new Product { ProductId = 10, IsActive = false } // Inactive
                    }
                }
            };

            _mockCartRepository.Setup(r => r.GetActiveCartDetailsAsync(userId))
                .ReturnsAsync(cart);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _orderService.CreateOrderFromCartAsync(userId, new OrderCreateDto()));
            
            Assert.Contains("no longer available", ex.Message);
        }
    }
}
