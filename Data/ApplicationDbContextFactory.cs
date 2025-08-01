using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // Thêm namespace này

namespace Training.Data // Đảm bảo namespace này khớp với namespace của DbContext của bạn
{
    // Kế thừa từ IDesignTimeDbContextFactory<TDbContext>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Bước 1: Lấy cấu hình từ appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Đặt base path là thư mục hiện tại của ứng dụng
                .AddJsonFile("appsettings.json") // Thêm appsettings.json
                .Build();

            // Bước 2: Lấy chuỗi kết nối
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Bước 3: Cấu hình DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString); // Sử dụng Npgsql cho PostgreSQL

            // Bước 4: Trả về một thể hiện của DbContext
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}