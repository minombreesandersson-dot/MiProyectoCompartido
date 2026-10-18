using Microsoft.EntityFrameworkCore;
using MiAppWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Registrar controladores y vistas para MVC
builder.Services.AddControllersWithViews();

// Registrar la conexión a SQLite
builder.Services.AddDbContext<AplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
