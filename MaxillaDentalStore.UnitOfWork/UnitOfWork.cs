using MaxillaDentalStore.Data;
using MaxillaDentalStore.Repositories.Implementations;
using MaxillaDentalStore.Repositories.Interfaces;
using MaxillaDentalStore.Repository.Implementations;
using MaxillaDentalStore.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaxillaDentalStore.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;


        public IUserRepository Users { get; }
        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }
        public ICategoryRepository Categories { get; }
        public ICartRepository Carts { get; }
        public IReviewRepository Reviews { get; }
        public IPackageRepository Packages { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public INotificationRepository Notifications { get; }

        // The constructor of the UnitOfWork class takes an instance of AppDbContext and instances of the repositories as parameters.
        // we inject the AppDbContext and repositories through the constructor, allowing for better separation of concerns and easier testing.
        // As DI is used in the application, the DI container will automatically resolve and inject the required dependencies when creating an instance of the UnitOfWork class.
        public UnitOfWork(AppDbContext context ,
            IUserRepository users , IProductRepository products , IOrderRepository orders , 
            IPackageRepository packages , IReviewRepository reviews , ICartRepository carts , ICategoryRepository categories,
            IRefreshTokenRepository refreshTokens, INotificationRepository notifications
            )
        {
            _context = context;
            Users = users;
            Products = products;
            Orders = orders;
            Categories = categories;
            Packages = packages;
            Carts = carts;
            Reviews = reviews;
            RefreshTokens = refreshTokens;
            Notifications = notifications;
        }
        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
