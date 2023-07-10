using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using  Microsoft.EntityFrameworkCore;
using App.Models;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Areas_Database_Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action=Index}")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;
        public DbManageController(AppDbContext dbContext){
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult DeleteDb(){
            return View();
        }
        [TempData]
        public string StatusMessage {get;set;}
        [HttpPost]
         public async Task<IActionResult> DeleteDbAsync(){


            var success =await  _dbContext.Database.EnsureDeletedAsync();
            StatusMessage =success ?"xoa thanh cong":"khong xoa duoc";
            return RedirectToAction(nameof(Index));
        }
         [HttpPost]
         public async Task<IActionResult> MigrateAsync(){
            Console.WriteLine(1);
            await  _dbContext.Database.MigrateAsync();
            StatusMessage ="Cap nhat database thanh cong";
            return RedirectToAction(nameof(Index));
        }
    }
}