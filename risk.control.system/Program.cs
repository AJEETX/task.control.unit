using System.Configuration;
using System.Reflection;
using System.Text;

using Highsoft.Web.Mvc.Charts;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Permission;
using risk.control.system.Seeds;
using risk.control.system.Services;

using SmartBreadcrumbs.Extensions;

using static Org.BouncyCastle.Math.EC.ECCurve;

using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
    options.TagName = "nav";
    options.TagClasses = "";
    options.OlClasses = "breadcrumb";
    options.LiClasses = "breadcrumb-item";
    options.ActiveLiClasses = "breadcrumb-item active";
});

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// For FileUpload
builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 1000000; // In case of multipart
    x.ValueLengthLimit = 1000000; //not recommended value
    x.MemoryBufferThreshold = 1000000;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClaimsInvestigationService, ClaimsInvestigationService>();
builder.Services.AddScoped<IEmpanelledAgencyService, EmpanelledAgencyService>();
builder.Services.AddScoped<IClaimPolicyService, ClaimPolicyService>();
builder.Services.AddScoped<IICheckifyService, ICheckifyService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFtpService, FtpService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMailboxService, MailboxService>();

builder.Services.AddScoped<IInboxMailService, InboxMailService>();
builder.Services.AddScoped<ISentMailService, SentMailService>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddScoped<ITrashMailService, TrashMailService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailService, MailService>();
// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddNToastNotifyNoty(new NotyOptions
    {
        ProgressBar = true,
        Timeout = 1000,
        Modal = true,
        Type = Enums.NotificationTypesNoty.Info
    })
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
var isProd = builder.Configuration.GetSection("IsProd").Value;
var prod = bool.Parse(isProd);
if (prod)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
         options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite("Data Source=itaskify_1_0_0_0.db"));
}

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddIdentityCore<VendorApplicationUser>(o => o.User.RequireUniqueEmail = true)
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddIdentityCore<ClientCompanyApplicationUser>(o => o.User.RequireUniqueEmail = true)
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

//builder.Services.AddAuthentication(options =>
//{
//    // custom scheme defined in .AddPolicyScheme() below
//    options.DefaultScheme = "JWT_OR_COOKIE";
//    options.DefaultChallengeScheme = "JWT_OR_COOKIE";
//})
//    .AddCookie("Cookies", options =>
//    {
//        options.LoginPath = "/Account/Login";
//        options.ExpireTimeSpan = TimeSpan.FromDays(1);
//        //options.LogoutPath = "/Account/Logout";
//        //options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
//        //options.Cookie.HttpOnly = true;
//        //// Only use this when the sites are on different domains
//        //options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
//    })
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = "https://localhost:7208/",
//            ValidAudience = "https://localhost:7208/",
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@1"))
//        };
//    })
//    // this is the key piece!
//    .AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
//    {
//        // runs on each request
//        options.ForwardDefaultSelector = context =>
//        {
//            // filter by auth type
//            string authorization = context.Request.Headers[HeaderNames.Authorization];
//            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
//                return "Bearer";

//            // otherwise always check for cookie auth
//            return "Cookies";
//        };
//    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = (context) =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        options.Cookie.Name = Guid.NewGuid().ToString() + "authCookie";
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.Cookie.HttpOnly = true;
        // Only use this when the sites are on different domains
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Domain = "check.azurewebsites.com";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CookieManager = new CookieManager();
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "iCheckify Utility",
        Description = "iCheckify API ",
        TermsOfService = new Uri("https://icheckify.co.in"),
        Contact = new OpenApiContact
        {
            Name = "iCheckify Team",
            Email = "hi@icheckify.co.in",
            Url = new Uri("https://icheckify.co.in"),
        },
        License = new OpenApiLicense
        {
            Name = "Use under OpenApiLicense",
            Url = new Uri("https://icheckify.co.in"),
        }
    });
});

var app = builder.Build();
app.UseSwagger();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
//app.UseStatusCodePagesWithRedirects("/Home/Error?code={0}");
app.UseHttpsRedirection();

await DatabaseSeed.SeedDatabase(app);

app.UseHttpLogging();

app.UseStaticFiles();

app.UseRouting();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});
app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always,
        HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
        MinimumSameSitePolicy = SameSiteMode.Strict
    }); ;
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseNToastNotify();
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.Value.Contains("/swagger/", StringComparison.OrdinalIgnoreCase))
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            context.Response.Redirect("/account/login");
            return;
        }
    }

    await next();
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();