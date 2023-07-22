using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using App.Data;


namespace App.Areas.Admin.Blog.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Blog")]
    [Route("admin/blog/category/[action]/{id?}")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Category
        //[Route("admin")]

        public async Task<IActionResult> Index()
        {

            var qr = (from c in _context.Categories select c)
                    .Include(c => c.CategoryChildren)
                    .Include(c => c.ParentCategory);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return View(categories);

        }

        // GET: Admin/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            Console.WriteLine($"day la details {category.Slug}");
            return View(category);
        }
        private void CreateSelectItems(List<Category> source, List<Category> des, int level)
        {
            foreach (var category in source)
            {


                des.Add(new Category()
                {
                    Id = category.Id,
                    Title = string.Concat(Enumerable.Repeat("---", level)) + category.Title,
                    ParentCategoryId = category.ParentCategoryId
                });
                if (category.CategoryChildren?.Count > 0)
                {
                    CreateSelectItems(category.CategoryChildren.ToList(), des, level + 1);
                }
            }

        }
        private List<Category> RemoveChildren(List<Category> source, List<Category> des)
        {
            //List<Category> updatedDes = new List<Category>(des);
            
            foreach (var category in source)
            {;
               des = des.Where(c =>c.Id != category.Id).ToList();
                if (category.CategoryChildren?.Count > 0)
                {
                    des = RemoveChildren(category.CategoryChildren.ToList(), des);
                }
            }
            return des;
        }

        // GET: Admin/Category/Create
        public async Task<IActionResult> Create()
        {
            // ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug");
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.CategoryChildren)
                    .Include(c => c.ParentCategory);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            categories.Insert(0, new Category()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });
            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title", -1);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.ParentCategoryId.Value == -1)
                    category.ParentCategoryId = null;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }

            // ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentCategoryId);
            var listcategory = await _context.Categories.ToListAsync();
            listcategory.Insert(0, new Category()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });
            //ViewData["ParentCategoryId"] = new SelectList(await GetItemsSelectCategorie(), "Id", "Title", category.ParentCategoryId);
            return View(category);
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.CategoryChildren)
                    .Include(c => c.ParentCategory);
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            categories.Insert(0, new Category()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });
            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            Console.WriteLine(items.Count);
            if (category.CategoryChildren?.Count > 0)
            {
               items = RemoveChildren(category.CategoryChildren.ToList(), items);
            }
            Console.WriteLine(items.Count);
            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title", -1);

            return View(category);
        }

        // POST: Admin/Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentCategoryId,Title,Description,Slug")] Category category)
        {
            Console.WriteLine("day la edit post");
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.ParentCategoryId == -1)
                    {
                        category.ParentCategoryId = null;
                    }
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var listcategory = await _context.Categories.ToListAsync();
            listcategory.Insert(0, new Category()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });
            ViewData["ParentCategoryId"] = new SelectList(listcategory, "Id", "Title", category.ParentCategoryId);
            return View(category);
        }

        // GET: Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.CategoryChildren)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            foreach (var categoryChild in category.CategoryChildren)
            {
                categoryChild.ParentCategoryId = category.ParentCategoryId;
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
