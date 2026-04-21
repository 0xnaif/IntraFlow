using IntraFlow.Application.Abstractions;
using IntraFlow.Application.DependencyInjection;
using IntraFlow.Infrastructure.DependencyInjection;
using IntraFlow.Infrastructure.Identity;
using IntraFlow.Infrastructure.Persistence;
using IntraFlow.Web.Seed;
using IntraFlow.Web.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    // Remove Persistant signing
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanApprove", policy =>
    policy.RequireRole("Approver", "Admin"));

    options.AddPolicy("IsAdmin", policy =>
    policy.RequireRole("Admin"));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await IdentitySeeder.SeedAsync(app.Services);
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
    pattern: "{controller=Requests}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


//app.MapGet("/db-check", async (IntraFlow.Infrastructure.Persistence.AppDbContext db) =>
//{
//    var canConnect = await db.Database.CanConnectAsync();
//    return Results.Ok(new { canConnect });
//});

//app.MapGet("/admin-only", () => "Hello, boss!")
//    .RequireAuthorization(policy => policy.RequireRole("Admin")); 

app.Run();


public partial class Program { }