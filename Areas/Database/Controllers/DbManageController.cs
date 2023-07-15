using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using App.Data;

namespace MVC.Areas_Database_Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action=Index}")]
    
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DbManageController(AppDbContext dbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;

        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }
        [TempData]
        public string StatusMessage { get; set; }
        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {


            var success = await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "xoa thanh cong" : "khong xoa duoc";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> MigrateAsync()
        {
            Console.WriteLine(1);
            await _dbContext.Database.MigrateAsync();
            StatusMessage = "Cap nhat database thanh cong";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SeedDataAsync()
        {
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var roleName = (string)r.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(roleName);
                if (rfound == null) { }
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            //admin, admin@example.com, admin123
            var userAdmin = await _userManager.FindByNameAsync("admin");
            if (userAdmin == null)
            {
                var user = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };

                await _userManager.CreateAsync(user,"123");
                await _userManager.AddToRoleAsync(user,RoleName.Administrator);


            }
            StatusMessage ="Vua seed database";
            return  RedirectToAction("Index");
        }



    }
}
