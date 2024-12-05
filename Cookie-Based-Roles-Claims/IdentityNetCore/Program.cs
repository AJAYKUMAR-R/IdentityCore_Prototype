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

app.MapGet("/hi", () => "Hello!");

app.MapDefaultControllerRoute();
app.MapRazorPages();





app.Run();

void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<ApplicationDBContext>(o => o.UseSqlServer(connString));
    services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>();

    services.Configure<IdentityOptions>(options =>
    {
        options.Password.RequiredLength = 3;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 3;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

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

    //This is where the policy is enable prevent the action
    //it is declarative authorization

    services.AddAuthorization(option =>
    {
        //This will enable the declarative authorization
        //we are defining the policy which means a user should have Claim Department : tech and role should be member
        option.AddPolicy("MemberDep", p => { p.RequireClaim("Department", "Tech").RequireRole("Member"); });

        //Department should be Tech and role should be admin to access that method
        option.AddPolicy("AdminDep", p => { p.RequireClaim("Department", "Tech").RequireRole("Admin"); });
    });
    services.AddControllersWithViews();
}