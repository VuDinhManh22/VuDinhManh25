using System;
using System.ComponentModel.DataAnnotations;

namespace Training.Models;

public class Product
{
      public int Id { get; set; }

      [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
      [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự.")]
      public string? Name { get; set; }

      [Required(ErrorMessage = "Giá sản phẩm là bắt buộc.")]
      [Range(0.01, 100000.00, ErrorMessage = "Giá sản phẩm phải nằm trong khoảng từ 0.01 đến 100000.")]
      public decimal Price { get; set; }

       [StringLength(500, ErrorMessage = "Mô tả sản phẩm không được vượt quá 500 ký tự.")]
      public string? Description { get; set; }
}
