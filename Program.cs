using InternshipManagementSystem.Data;
using InternshipManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ Add Entity Framework Core (Scoped by default)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(10, 4, 32)) // Matches your XAMPP MariaDB
    ));

// ✅ Register repositories as Scoped (so they can use the Scoped AppDbContext)
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<InternshipRepository>();
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddScoped<CVRepository>();
builder.Services.AddScoped<CompanyProfileRepository>();
builder.Services.AddScoped<TransactionRepository>();

// Register the HttpClient and our Service together
builder.Services.AddHttpClient<GeminiService>();

// Email Service (Singleton is fine here as it just uses config)
builder.Services.AddSingleton<EmailService>();

// ✅ ADD SWAGGER SERVICES HERE ✅
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ ADD SWAGGER MIDDLEWARE HERE ✅
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HireNest API V1");
    c.RoutePrefix = "swagger"; // This makes it accessible at /swagger
});

app.UseAuthentication();  // ← Must be before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();