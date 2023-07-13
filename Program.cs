using System.Net;
using App.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using App.ExtendsMethods;
using Microsoft.AspNetCore.Routing.Constraints;
using App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using App.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddOptions();
var mailsetting = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddSingleton<IEmailSender,SendMailService>();

builder.Services.Configure<RazorViewEngineOptions>(option =>{
    //{0} ten action
    // {1} ten controller
    // {2} ten area
    option.ViewLocationFormats.Add("/MyView/{1}/{0}"+ RazorViewEngine.ViewExtension);
});

builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<PlanetService>();


builder.Services.AddDbContext<AppDbContext>(options =>{
    string connectString = builder.Configuration.GetConnectionString("AppMvcConnectString");
    
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 33));
    options.UseMySql(connectString, serverVersion);
});

builder.Services.AddIdentity<AppUser,IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();



builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;
});

builder.Services.ConfigureApplicationCookie(options=>{
    options.LoginPath="/login";
    options.LogoutPath="/logout";
    options.AccessDeniedPath ="/khongthetruycap.html";
});


builder.Services.AddAuthentication()
                .AddGoogle(options=>{
                    var ggConfig = builder.Configuration.GetSection("Authentication:Google");
                    options.ClientId = ggConfig["ClientId"];
                    options.ClientSecret =  ggConfig["ClientSecret"];
                    //Default :SignIn goole
                    options.CallbackPath ="/dang-nhap-tu-google";
                })
                .AddFacebook(options =>{
                    var ggConfig = builder.Configuration.GetSection("Authentication:Facebook");
                    options.ClientId = ggConfig["AppId"];
                    options.ClientSecret =  ggConfig["AppSecret"];
                    //Default :SignIn goole
                    options.CallbackPath ="/dang-nhap-tu-facebook";
                });

builder.Services.AddSingleton<IdentityErrorDescriber,AppIdentityErrorDescriber>();
builder.Services.AddAuthorization(options =>{
    options.AddPolicy("ViewManage",builder =>{
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});

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
app.UseAuthentication();//xac dinh user
app.UseAuthorization();//xac dinh truy cap

app.AppStatusCodePage(); //Tuy bien loi tu 400 - 599


app.MapGet("/sayhello",async context => {
    await context.Response.WriteAsync("Hello world");
});

app.MapAreaControllerRoute(
    name :"Product",
    areaName : "ProductManage",
    pattern:"{controller}/{action=Index}/{id?}"

);
// app.MapAreaControllerRoute(
//     name :"Contact",
//     areaName : "Contact",
//     pattern:"{controller}/{action=Index}/{id?}"

// );


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name:"first",
    pattern:"/xemsanpham/{id?}",
    defaults: new {
        controller= "First",
        action ="ViewProduct"
    },
    constraints : new {
        id = new RangeRouteConstraint(2,4),
    }
);
app.MapRazorPages();
app.Run();
