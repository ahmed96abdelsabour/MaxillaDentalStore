using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Implementations
{
    public class PackageRepository : IPackageRepository
    {
        private readonly AppDbContext _context;

        public PackageRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. Get all active packages with performance optimization
        // We use AsNoTracking because this is a read-only operation for the UI
        public async Task<IEnumerable<Package>> GetAllActivePackagesAsync()
        {
            return await _context.Packages
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Get package by id (basic lookup without related data)
        public async Task<Package?> GetByIdAsync(int packageId)
        {
            return await _context.Packages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PackageId == packageId);
        }

        // 2. Get package with its items and the actual products using Eager Loading
        // We use Include then ThenInclude to reach the final Product data in one SQL Join
        public async Task<Package?> GetPackageWithDetailsAsync(int packageId)
        {
            return await _context.Packages
                .AsNoTracking()
                .Include(p => p.PackageItems)       // The join table
                    .ThenInclude(pi => pi.Product)  // The actual product data
                .FirstOrDefaultAsync(p => p.PackageId == packageId);
        }

        // 3. Add a new package (Admin use)
        // Operation is async to not block the thread during DB I/O
        public async Task AddAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
        }

        // 4. Update an existing package
        // This is synchronous as it only marks the entity as 'Modified' in the ChangeTracker
        public void Update(Package package)
        {
            _context.Packages.Update(package);
        }

        // 5. Delete a package
        public void Delete(Package package)
        {
            _context.Packages.Remove(package);
        }
    }
}
