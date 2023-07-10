using System.Net;
using App.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using App.ExtendsMethods;
using Microsoft.AspNetCore.Routing.Constraints;
using App.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

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
