using Bimss.Application;
using Bimss.Infrastructure;
using Bimss.Infrastructure.Authorization;
using Bimss.Infrastructure.Identity.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddBimssInfrastructure(builder.Configuration);
builder.Services.AddBimssApplication();
builder.Services.AddBimssAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DevelopmentIdentitySeeder.SeedAsync(app.Services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

// Exposed so Bimss.IntegrationTests can boot this host via WebApplicationFactory<Program>.
public partial class Program;
