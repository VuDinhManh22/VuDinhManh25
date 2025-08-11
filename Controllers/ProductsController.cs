using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Training.Data; 
using Training.Models; 
using Microsoft.AspNetCore.Authorization;
using Training.DTOs; // Để sử dụng Exception

namespace Training.Controllers
{
    [Route("api/product")] 
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context /*, ILogger<ProductsController> logger */)
        {
            _context = context;
        }

        
       [HttpGet]
       [AllowAnonymous] // Hoặc [Authorize] tùy thuộc vào logic của bạn
       public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
       [FromQuery] int pageNumber = 1,
       [FromQuery] int pageSize = 10)
      {
          var products = await _context.Products
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

          var productDtos = products.Select(p => new ProductDto
      {
         Id = p.Id,
         Name = p.Name,
         Price = p.Price,
         Description = p.Description
     });

         return Ok(productDtos);
}

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var productDto = await _context.Products
                                  .Where(p => p.Id == id)
                                  .Select(p => new ProductDto 
                                  {
                                      Id = p.Id,
                                      Name = p.Name,
                                      Price = p.Price,
                                      Description = p.Description
                                  })
                                  .FirstOrDefaultAsync();

            if (productDto == null)
            {
               return NotFound();
         }

              return Ok(productDto);
    }

        // POST: api/products
        // Tạo một sản phẩm mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (product == null)
            {
                return BadRequest("Dữ liệu sản phẩm không được rỗng.");
            }

           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            try
            {
                _context.Products.Add(product); 
                await _context.SaveChangesAsync(); 

                
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (DbUpdateException ex) 
            {
                return StatusCode(500, "Có lỗi xảy ra khi lưu sản phẩm vào cơ sở dữ liệu. Vui lòng thử lại.");
            }
            catch (Exception ex) 
            {
                
                return StatusCode(500, "Đã xảy ra lỗi không xác định khi tạo sản phẩm.");
            }
        }

        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(int id, Product updatedProduct)
        {
          
            updatedProduct.Id = id;

            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            _context.Entry(updatedProduct).State = EntityState.Modified; 
            try
            {
                await _context.SaveChangesAsync(); 
            }
            catch (DbUpdateConcurrencyException) 
            {
                
                if (!await ProductExists(id))
                {
                    return NotFound($"Không tìm thấy sản phẩm với ID {id} để cập nhật.");
                }
                else
                {
                    
                    throw; 
                }
            }
            catch (DbUpdateException ex) 
            {
                
                return StatusCode(500, $"Đã xảy ra lỗi cơ sở dữ liệu khi cập nhật sản phẩm với ID {id}. Vui lòng thử lại.");
            }
            catch (Exception ex) 
            {
                
                return StatusCode(500, $"Đã xảy ra lỗi không xác định khi cập nhật sản phẩm với ID {id}.");
            }

            return NoContent();
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var productToRemove = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (productToRemove == null)
                {
                    return NotFound($"Sản phẩm với ID {id} không tồn tại.");
                }

                _context.Products.Remove(productToRemove); 
                await _context.SaveChangesAsync(); 

               
                return NoContent();
            }
            
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi cơ sở dữ liệu khi cố gắng xóa sản phẩm với ID {id}. Vui lòng thử lại.");
            }
           
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Đã xảy ra lỗi không xác định khi cố gắng xóa sản phẩm với ID {id}.");
            }
        }

       
        private async Task<bool> ProductExists(int id)
        {
            return await _context.Products.AnyAsync(e => e.Id == id);
        }
    }
}