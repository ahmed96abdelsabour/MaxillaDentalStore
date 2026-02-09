using Microsoft.AspNetCore.Mvc;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MaxillaDentalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PackageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPackages()
        {
            var packages = await _packageService.GetAllPackagesAsync();
            return Ok(packages);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPackageById(int id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            
            if (package == null)
            {
                return NotFound(new { message = $"Package with ID {id} not found" });
            }

            return Ok(package);
        }

        [HttpGet("{packageId}/products")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPackageProducts(int packageId)
        {
            var products = await _packageService.GetPackageItemsAsync(packageId);
            
            if (products == null)
            {
                return NotFound(new { message = $"Package with ID {packageId} not found" });
            }

            return Ok(products);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body cannot be null" });

            // Basic validation could also be improved with FluentValidation, but keeping checks here for now matching style
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "Package name is required" });
            if (dto.Price <= 0) return BadRequest(new { message = "Package price must be greater than 0" });

            try
            {
                var result = await _packageService.CreatePackageAsync(dto);
                if (!result) return BadRequest(new { message = "Failed to create package" });
                return StatusCode(StatusCodes.Status201Created, new { message = "Package created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{packageId}/add-product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddProductToPackage(int packageId, int productId)
        {
            try
            {
                var result = await _packageService.AddProductToPackageAsync(packageId, productId);
                
                if (!result)
                {
                    return NotFound(new { message = $"Package with ID {packageId} not found" });
                }

                return Ok(new { message = "Product added to package successfully" });
            }
            catch (Exception ex)
            {
                // Should distinguish between NotFound and BadRequest ideally in service,
                // but currently service returns bool. 
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var result = await _packageService.DeletePackageAsync(id);
            
            if (!result)
            {
                return NotFound(new { message = $"Package with ID {id} not found" });
            }

            return NoContent();
        }
    }
}
