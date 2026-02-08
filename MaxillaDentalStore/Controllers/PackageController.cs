using Microsoft.AspNetCore.Mvc;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;

namespace MaxillaDentalStore.API.Controllers
{
    /// <summary>
    /// Package Controller - Manages product bundles/packages
    /// Follows Clean Architecture: Zero business logic, only delegates to IPackageService
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        /// <summary>
        /// Constructor - Injects IPackageService dependency
        /// </summary>
        /// <param name="packageService">Package service for business logic</param>
        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        /// <summary>
        /// Get all available packages
        /// </summary>
        /// <returns>List of all packages</returns>
        /// <response code="200">Returns all packages</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PackageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PackageDto>>> GetAllPackages()
        {
            var packages = await _packageService.GetAllPackagesAsync();
            return Ok(packages);
        }

        /// <summary>
        /// Get a specific package by ID with all products inside
        /// Includes detailed product information mapped from PackageItem join entity
        /// </summary>
        /// <param name="id">Package ID</param>
        /// <returns>Package details with list of products</returns>
        /// <response code="200">Returns the package with products</response>
        /// <response code="404">Package not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PackageDto>> GetPackageById(int id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            
            if (package == null)
            {
                return NotFound($"Package with ID {id} not found");
            }

            return Ok(package);
        }

        /// <summary>
        /// Get all products inside a specific package
        /// Returns detailed product information from the package
        /// </summary>
        /// <param name="packageId">Package ID</param>
        /// <returns>List of products in the package</returns>
        /// <response code="200">Returns products in package</response>
        /// <response code="404">Package not found</response>
        [HttpGet("{packageId}/products")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetPackageProducts(int packageId)
        {
            var products = await _packageService.GetPackageItemsAsync(packageId);
            
            if (products == null)
            {
                return NotFound($"Package with ID {packageId} not found");
            }

            return Ok(products);
        }

        /// <summary>
        /// Create a new package with multiple products
        /// </summary>
        /// <param name="dto">Package creation details including product IDs</param>
        /// <returns>Created status</returns>
        /// <response code="201">Package successfully created</response>
        /// <response code="400">Invalid request data</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Request body cannot be null");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Package name is required");
            }

            if (dto.Price <= 0)
            {
                return BadRequest("Package price must be greater than 0");
            }

            var result = await _packageService.CreatePackageAsync(dto);
            
            if (!result)
            {
                return BadRequest("Failed to create package");
            }

            return StatusCode(StatusCodes.Status201Created, "Package created successfully");
        }

        /// <summary>
        /// Add a product to an existing package
        /// Manages the many-to-many relationship via PackageItem join entity
        /// </summary>
        /// <param name="packageId">Package ID</param>
        /// <param name="productId">Product ID to add</param>
        /// <returns>Success status</returns>
        /// <response code="200">Product successfully added to package</response>
        /// <response code="400">Invalid request or product already in package</response>
        /// <response code="404">Package or product not found</response>
        [HttpPost("{packageId}/add-product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddProductToPackage(int packageId, int productId)
        {
            if (packageId <= 0)
            {
                return BadRequest("Invalid package ID");
            }

            if (productId <= 0)
            {
                return BadRequest("Invalid product ID");
            }

            try
            {
                var result = await _packageService.AddProductToPackageAsync(packageId, productId);
                
                if (!result)
                {
                    return NotFound($"Package with ID {packageId} not found");
                }

                return Ok("Product added to package successfully");
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a package
        /// </summary>
        /// <param name="id">Package ID to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Package successfully deleted</response>
        /// <response code="404">Package not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var result = await _packageService.DeletePackageAsync(id);
            
            if (!result)
            {
                return NotFound($"Package with ID {id} not found");
            }

            return NoContent();
        }
    }
}
