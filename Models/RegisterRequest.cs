using System;
using System.ComponentModel.DataAnnotations;

namespace Training.Models;

public class RegisterRequest
{
        [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string? Password { get; set; }

        // Có thể thêm Role ở đây, hoặc để mặc định là "User" trong backend
        public string Role { get; set; } = "User";
}
