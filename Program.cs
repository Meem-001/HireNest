using InternshipManagementSystem.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using InternshipManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register services
builder.Services.AddSingleton<DbHelper>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<InternshipRepository>();
builder.Services.AddSingleton<ApplicationRepository>();
builder.Services.AddSingleton<CVRepository>();
builder.Services.AddSingleton<TransactionRepository>();
builder.Services.AddScoped<CompanyProfileRepository>();
// Register the HttpClient and our Service together
builder.Services.AddHttpClient<GeminiService>();

// ✅ ADD SWAGGER SERVICES HERE ✅
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<EmailService>();

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