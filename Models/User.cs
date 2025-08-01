using System;
using System.ComponentModel.DataAnnotations;

namespace Training.Models;

public class User
{
   public int Id { get; set; }

        [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên người dùng không được vượt quá 100 ký tự.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        public string? Email { get; set; } 

        // --- THÊM CÁC TRƯỜNG MỚI CHO BẢO MẬT ---
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        // Lưu ý: Mật khẩu này sẽ được băm (hash) trước khi lưu vào DB
        public string? Password { get; set; } 

        [Required(ErrorMessage = "Vai trò người dùng là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Vai trò không được vượt quá 50 ký tự.")]
        public string Role { get; set; } = "User"; // Giá trị mặc định là "User"
        // --- KẾT THÚC CÁC TRƯỜNG MỚI ---
}
