using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using App.Data;
using Bogus;
using App.Models.Blog;
using Microsoft.AspNetCore.Razor.Language;
using Bogus.DataSets;

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
            var userAdmin = await _userManager.FindByEmailAsync("admin@example.com");
            if (userAdmin == null)
            {
                var user = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };

                await _userManager.CreateAsync(user, "123");
                await _userManager.AddToRoleAsync(user, RoleName.Administrator);


            }
            SeedPostCategory();
            StatusMessage = "Vua seed database";
            return RedirectToAction("Index");
        }

        private async void SeedPostCategory()
        {
            try
            {
                _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(category=>category.Description.Contains("@fakeData")));
                _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(p=>p.Content.Contains("@fakeData")));
                var fakeCategory = new Faker<Category>();
                int cm = 1;
                fakeCategory.RuleFor(c => c.Title, fk => $"Cm{cm++} {fk.Lorem.Sentence(1, 2).Trim('.')}");
                fakeCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "@fakeData");
                fakeCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

                var cate1 = fakeCategory.Generate();
                var cate11 = fakeCategory.Generate();
                var cate12 = fakeCategory.Generate();
                var cate2 = fakeCategory.Generate();
                var cate21 = fakeCategory.Generate();
                var cate211 = fakeCategory.Generate();

                cate11.ParentCategory = cate1;
                cate12.ParentCategory = cate1;
                cate21.ParentCategory = cate2;
                cate211.ParentCategory = cate21;

                var categories = new Category[] { cate1, cate11, cate12, cate2, cate21, cate211 };
                _dbContext.Categories.AddRange(categories);
                _dbContext.SaveChanges();


                //Post
                var rCateIndex = new Random();
                int bv = 1;
                var user =  _userManager.GetUserAsync(this.User).Result;
                var fakePost = new Faker<Post>();
                fakePost.RuleFor(p=>p.AuthorId, f =>user.Id);
                fakePost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7)+"@fakeData");
                fakePost.RuleFor(p=>p.DateCreated , f=>f.Date.Between(DateTime.Now.AddDays(-30), DateTime.Now));
                fakePost.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
                fakePost.RuleFor(p=>p.Published,f=>true);
                fakePost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
                fakePost.RuleFor(p => p.Title, f =>$"Bai {bv++} "+f.Lorem.Sentence(3, 4).Trim('.'));

                List<Post> posts = new List<Post>();
                List<PostCategory> postCategories = new List<PostCategory>();   

                for(int i = 0;i<40;i++){
                    var post = fakePost.Generate();
                    post.DateUpdated = post.DateCreated;
                    posts.Add(post);
                    var postcategory = new PostCategory{
                        Category = categories[rCateIndex.Next(5)],
                        Post = post,
                    };
                    postCategories.Add(postcategory);
                }
                _dbContext.Posts.AddRange(posts);
                _dbContext.PostCategories.AddRange(postCategories);
                _dbContext.SaveChanges();   


            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

    }
}
