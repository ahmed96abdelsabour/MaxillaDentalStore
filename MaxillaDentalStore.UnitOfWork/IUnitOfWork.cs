using MaxillaDentalStore.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.UnitOfWork
{
    // Idisposable is implemented to ensure that any resources used by the unit of work
    // can be properly released when they are no longer needed, preventing memory leaks and ensuring efficient resource management.
    public interface IUnitOfWork : IDisposable 
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IPackageRepository Packages { get; }
        IReviewRepository Reviews { get; }
        ICartRepository Carts { get; }
        ICategoryRepository Categories { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        Task<int> CommitAsync();
    }
}
