using System;
using IdentityNetCore.Data;
using IdentityNetCore.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var connString = builder.Configuration["ConnectionStrings:Default"];
var smtp = builder.Configuration.GetSection("Smtp");
ConfigureServices(builder.Services);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();
app.UseAuthentication();

app.MapDefaultControllerRoute();
app.MapRazorPages();



app.Run();


void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<ApplicationDBContext>(o => o.UseSqlServer(connString));
    services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>()
        .AddDefaultTokenProviders();

    services.Configure<IdentityOptions>(options =>
    {
        //This is for the password configuration
        options.Password.RequiredLength = 3;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        //This for the lockout configuration
        //This will enable the lockout even for the new user
        options.Lockout.AllowedForNewUsers = true;
        //This will defines how many attempts are allowed
        options.Lockout.MaxFailedAccessAttempts = 2;
        //This will defines the how long the lock is enabled
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);



        options.SignIn.RequireConfirmedEmail = false;
    });

    services.ConfigureApplicationCookie(option =>
    {
        option.LoginPath = "/Identity/Signin";
        option.AccessDeniedPath = "/Identity/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromHours(10);
    });

    services.Configure<SmtpOptions>(smtp);

    services.AddSingleton<IEmailSender, SmtpEmailSender>();
    services.AddAuthorization(option =>
    {
        option.AddPolicy("MemberDep", p => { p.RequireClaim("Department", "Tech").RequireRole("Member"); });

        option.AddPolicy("AdminDep", p => { p.RequireClaim("Department", "Tech").RequireRole("Admin"); });
    });
    services.AddControllersWithViews();
}