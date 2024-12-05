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

//This is for Enabling the Entity Framework to use the sql server as db
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connString));

//This is for enabling the Identity entityFramework to use the current DBcontext to run the seed script
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

//Enabling CROS 
//This will enable the cross in the service pipe line
//Note : In which application has the data to send in that application we need configre the CROS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApp1", builder =>
    {
        builder.WithOrigins("https://localhost:65137") // Replace with App1's origin // Note should not give the / after port number
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

//This for configure the option with signIn restriction for the password
builder.Services.Configure<IdentityOptions>(options => {
    options.Password.RequiredLength = 3;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;

    //Enabling the maxmimum attempt Failed and timeout for restore
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

    //Enabling confirmation mail
    options.SignIn.RequireConfirmedEmail = true;
});

//Configuring cookie-based Authentication
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Identity/Signin";
    options.AccessDeniedPath = "/Identity/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(10);
});

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();




app.UseRouting();

//we were defining the policy and we are using the policy right now
app.UseCors("AllowApp1");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();