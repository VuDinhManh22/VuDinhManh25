using System;
using System.ComponentModel.DataAnnotations;

namespace Training.Models;

public class LoginRequest
{
     [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string? Password { get; set; }
}
