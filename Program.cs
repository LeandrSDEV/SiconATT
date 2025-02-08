using Microsoft.EntityFrameworkCore;
using Servidor.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BancoContext>(x
        => x.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(3, 0, 38))));

builder.Services.AddScoped<AbareService>();
builder.Services.AddScoped<CupiraService>();
builder.Services.AddScoped<CansancaoService>();
builder.Services.AddScoped<XiqueXiqueService>();
builder.Services.AddScoped<AlcinopolisService>();
builder.Services.AddScoped<CafarnaumService>();
builder.Services.AddScoped<IndiaporaService>();

builder.Services.AddScoped<MatriculaService>();
builder.Services.AddScoped<SecretariaService>();
builder.Services.AddScoped<ServidorService>();
builder.Services.AddScoped<CategoriaService>();

builder.Services.AddScoped<CleanupService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
