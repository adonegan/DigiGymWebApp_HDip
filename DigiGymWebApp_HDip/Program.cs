using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Helpers;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging();
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Remove AddDefaultIdentity, add IdentityRole as parameter because I'm using roles
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();

        //Ensures Dependency Injection knows about my custom Claims maker
        builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, DigiGymClaimsFactory>();

        // Role-based authorisation
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("TrainerOnly", policy => policy.RequireRole("Trainer"));
            options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
        });

        
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddScoped<CalorieCounterService, CalorieCounterService>();
        builder.Services.AddScoped<BMIService, BMIService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}