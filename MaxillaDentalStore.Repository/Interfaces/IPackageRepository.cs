using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    public interface IPackageRepository
    {

        // get all active packages (not expired) to display on the home page and in the packages section
        Task<IEnumerable<Package>> GetAllActivePackagesAsync();


        // get package details by id to display on the package details page
        Task<Package?> GetPackageWithDetailsAsync(int packageId);

        // add a new package to the database (admin functionality)
        Task AddAsync(Package package);

        // update an existing package in the database (admin functionality)
        void Update(Package package);

        // delete a package from the database (admin functionality)
        void Delete(Package package);
    }
}
