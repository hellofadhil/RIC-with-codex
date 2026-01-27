Setup Projects RIC Template

Backend, Membuat CRUD sederhana

1. Membuat Models di folder Core > Models > Entities
2. Menambahkan Models ke dalam OneProDbContext.cs
3. Membuat ViewModels di folder Core > ViewModels

4. Menyesuaikan Url Database nya di folder API > appsettings.Development.json
   contoh:

   "ConnectionStrings": {
   "DbContextConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RIC_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
   }

Sekarang Masuk ke bagian Controller, dll

5. Buat Function di Interfaces, dalam folder API > Interfaces
6. Lalu buat Repositories atau logika utama nya di folder API > Repositories

7. Daftarkan Function Interfaces dan Repositories ke DependencyInjection dalam
   file ServicesExtension.cs

   Contoh

   # services.AddScoped<IRicRepository, RicRepository>();

8. Buat Stored Procedure berdasarkan kebutuhan yang ada, contoh nya dalam file StoredProcedure.mdx
9. Lalu Buat dan terapkan Stored Procedure ke dalam Controller

10. Jalankan API dengan perintah

    cd OnePro.API
    dotnet run --urls "https://localhost:58644"

11. Dapatkan token dengan memanggil API lewat url https://login-v3.qa.idaman.pertamina.com/connect/token
12. Pasang Auth dengan format "Bearer $access_token"

13. Sesuaikan UrlLogin dan Authority di file appsettings.Development.json,

    Note: karena kalau tidak akan error seperti ini: www-authenticate: Bearer error="invalid_token",error_description="The issuer 'https://login-v3.qa.idaman.pertamina.com' is invalid"

14. Edit file AuthExtensions.cs
 
    .AddJwtBearer(options =>
        {
            options.Authority = jwtSettings.Authority;
            // options.Audience = jwtSettings.Audience;

            options.Audience = "idproo.api"; 
            options.RequireHttpsMetadata = false;
        });

    Note: Error  www-authenticate: Bearer error="invalid_token",error_description="The audience 'idproo.api' is invalid" 

15. Lalu Jalankan lagi API nya, dan selesai.


Frontend, Implementasi CRUD yang telah dibuat

1. Buat Interface sesuai yang dibutuhkan di dalam folder Web > Services > Interfaces
2. Buat Implement sesuai yang dibutuhkan di dalam folder Web > Services > Implement
3. Daftarkan Interface dan Implement yang telah di buat ke file ServicesExtension.cs
4. Buat Controllers untuk Tampilan nya
5. Lalu buat file Index.cshtml dengan format Judul/Index.cshtml, Contoh Views > RIC/Index.cshtml



Note: 

saya ubah file SubMenuRepository.cs