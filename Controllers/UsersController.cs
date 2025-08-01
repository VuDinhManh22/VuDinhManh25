using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Training.Data;
using Training.Models;


namespace MySimpleApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context) // Tiêm DbContext vào constructor
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Sử dụng ToListAsync để lấy tất cả user từ DB
            return Ok(await _context.Users.ToListAsync());
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            // Sử dụng FirstOrDefaultAsync để tìm user theo ID
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (user == null)
            {
                return BadRequest("Dữ liệu người dùng không được rỗng");
            }

            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về 400 Bad Request với các lỗi validation
            }

            _context.Users.Add(user); // Thêm user vào DbContext
            await _context.SaveChangesAsync(); // Lưu thay đổi vào DB

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            // if (id != updatedUser.Id)
            // {
            //     return BadRequest("ID in URL does not match ID in body.");
            // }

            // // Attach đối tượng để EF Core theo dõi và đánh dấu là Modified
            // _context.Entry(updatedUser).State = EntityState.Modified;

            // try
            // {
            //     await _context.SaveChangesAsync(); // Lưu thay đổi vào DB
            // }
            // catch (DbUpdateConcurrencyException)
            // {
            //     // Kiểm tra xem user có tồn tại không
            //     if (!await UserExists(id))
            //     {
            //         return NotFound();
            //     }
            //     else
            //     {
            //         throw;
            //     }
            // }

            updatedUser.Id = id; // Luôn tin tưởng ID từ URL như bạn muốn

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(updatedUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id))
                {
                    return NotFound("Không tìm thấy người dùng để cập nhật.");
                }
                else
                {
                    // Vẫn ném lại lỗi này nếu bạn muốn phân biệt concurrency error
                    throw;
                }
            }
            catch (DbUpdateException ex) // Bắt lỗi DB khác (ví dụ: ràng buộc)
            {
                // Log lỗi
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật người dùng. Vui lòng thử lại.");
            }

            catch (Exception ex) // Bắt bất kỳ lỗi nào khác không mong muốn
            {
                // Log lỗi
                return StatusCode(500, "Đã xảy ra lỗi không xác định khi cập nhật người dùng.");
            }
            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            //     var userToRemove = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            //     if (userToRemove == null)
            //     {
            //         return NotFound();
            //     }

            //     _context.Users.Remove(userToRemove); // Đánh dấu user để xóa
            //     await _context.SaveChangesAsync(); // Lưu thay đổi vào DB

            //     return NoContent();
            // }

            // // Phương thức hỗ trợ kiểm tra sự tồn tại của user
            // private async Task<bool> UserExists(int id)
            // {
            //     return await _context.Users.AnyAsync(e => e.Id == id);
            // }
            try
            {
                var userToRemove = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (userToRemove == null)
                {
                    // Trả về 404 Not Found nếu không tìm thấy người dùng
                    return NotFound($"Người dùng với ID {id} không tồn tại.");
                }

                _context.Users.Remove(userToRemove); // Đánh dấu đối tượng để xóa
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

                // Trả về 204 No Content nếu xóa thành công
                return NoContent();
            }
            // Bắt DbUpdateException cho các lỗi liên quan đến DB (ví dụ: ràng buộc khóa ngoại nếu user đang được tham chiếu)
            catch (DbUpdateException ex)
            {
                // TODO: Log lỗi chi tiết hơn ở đây bằng _logger.LogError(ex, "...");
                // Ví dụ: Nếu người dùng có các liên kết khóa ngoại đang hoạt động
                // if (ex.InnerException?.Message.Contains("violates foreign key constraint") == true)
                // {
                //     return Conflict($"Không thể xóa người dùng với ID {id} vì có các bản ghi liên quan.");
                // }

                // Mặc định trả về 500 nếu là lỗi DB khác
                return StatusCode(500, $"Đã xảy ra lỗi cơ sở dữ liệu khi cố gắng xóa người dùng với ID {id}. Vui lòng thử lại.");
            }
            // Bắt Exception chung cho mọi lỗi không mong muốn khác
            catch (Exception ex)
            {
                // TODO: Log lỗi chi tiết hơn ở đây bằng _logger.LogError(ex, "...");
                return StatusCode(500, $"Đã xảy ra lỗi không xác định khi cố gắng xóa người dùng với ID {id}.");
            }
        }

        // Phương thức hỗ trợ để kiểm tra xem một người dùng có tồn tại không (nếu bạn vẫn sử dụng ở PutUser)
        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.Id == id);
        }
    }
}