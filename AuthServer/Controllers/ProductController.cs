using Auth.Core.DTOs;
using Auth.Core.Entities;
using Auth.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Apı.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : CustomBaseController
    {
        private readonly IServiceGeneric<Product,ProductDto> _productService;
        public ProductController(IServiceGeneric<Product, ProductDto> productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            return CreateActionResult(await _productService.GetAllAsync());

        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductDto productDto)
        {
            return CreateActionResult(await _productService.AddAsync(productDto));

        }
        [HttpPut]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            return CreateActionResult(await _productService.RemoveAsync(id));

        }

    }
}
