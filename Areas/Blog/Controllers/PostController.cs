using App.Areas.Blog.Models;
using App.Data;
using App.Models;
using App.Models.Blog;
using App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        [TempData]
        public string StatusMessage { get; set; }
        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public const int ITEMS_PER_PAGE = 10;
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize = ITEMS_PER_PAGE)
        {
            var posts = _context.Posts
                .Include(P => P.Author)
                .OrderByDescending(p => p.DateUpdated);

            int totalPosts = await posts.CountAsync();
            int countPages = (int)Math.Ceiling((double)totalPosts / pagesize);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;
            var PagingModel = new PagingModel()
            {
                currentpage = currentPage,
                countpages = countPages,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };

            ViewBag.pagingModel = PagingModel;
            ViewBag.totalPosts = totalPosts;
            ViewBag.postIndex = (currentPage - 1) * pagesize;
            var postsInPage = await posts.Skip((currentPage - 1) * pagesize)
                       .Take(pagesize)
                       .Include(p => p.PostCategories)
                       .ThenInclude(pc => pc.Category)
                       .ToListAsync();
            return View(postsInPage);

        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();
            return View(post);

        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            _context.Remove(post);
            await _context.SaveChangesAsync();

            StatusMessage = $"Da so thanh cong {post.Title}";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {

            var post = _context.Posts.Where(p => p.PostId == id)
               .Include(p => p.Author)
               .FirstOrDefault();

            if (post == null)
                return NotFound();
            return View(post);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
                return NotFound();

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            var editPost = new CreatePostModel()
            {
                PostId = post.PostId,   

                Title = post.Title,
                Description = post.Description,
                Slug = post.Slug,
                Content = post.Content,
                Published = post.Published,
                CategoryIds = post.PostCategories.Select(pc => pc.CategoryId).ToArray()
            };

            return View(editPost);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryIds")] CreatePostModel post)
        {
            if (id != post.PostId) return NotFound();
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
                ModelState.SetModelValue("Slug", new ValueProviderResult(post.Slug));

                ModelState.Clear();
                TryValidateModel(post);
            }
            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != post.PostId))
            {
                ModelState.AddModelError("Slug", "Nhap Chuoi Url Khac");
                return View(post);

            }
            if (post.CategoryIds == null) {
                post.CategoryIds = new int[] { };
                ModelState.Clear();
                TryValidateModel(post);
            }


            ModelState.Remove("AuthorId");
            ModelState.Remove("Author");
            ModelState.Remove("PostCategories");
            
            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null) return NotFound();

                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Slug = post.Slug;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.DateUpdated = DateTime.Now;

                   
                    var oldCategoryIds = postUpdate.PostCategories.Select(pc => pc.CategoryId).ToArray();
                    var newCategoryIds = post.CategoryIds;
                    var deletedCategoryIds = oldCategoryIds.Except(newCategoryIds).ToArray();
                    var addedCategoryIds = newCategoryIds.Except(oldCategoryIds).ToArray();
                    foreach (var CateId in deletedCategoryIds)
                    {
                        var postCategory = await _context.PostCategories.FirstOrDefaultAsync(pc => pc.CategoryId == CateId && pc.PostId == post.PostId);
                        _context.PostCategories.Remove(postCategory);
                    }
                    foreach (var CateId in addedCategoryIds)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            CategoryId = CateId,
                            Post = postUpdate
                        });
                    }
                    await _context.SaveChangesAsync();
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Posts.Any(p=>p.PostId==post.PostId))
                        return NotFound();
                    else
                        throw;
                }
                StatusMessage = "Vua Cap nhat bai viet";
                return RedirectToAction(nameof(Index));

            }
            else
            {
                // var error = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                // error.ForEach(p=>Console.WriteLine(p));
            }
            return View(post);

        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIds")] CreatePostModel post)
        {

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
                ModelState.SetModelValue("Slug", new ValueProviderResult(post.Slug));

                ModelState.Clear();
                TryValidateModel(post);
            }
            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Nhap Chuoi Url Khac");
                return View(post);

            }

            ModelState.Remove("AuthorId");
            ModelState.Remove("Author");
            ModelState.Remove("PostCategories");

            if (ModelState.IsValid)
            {

                post.DateCreated = DateTime.Now;
                post.DateUpdated = DateTime.Now;
                var user = await _userManager.GetUserAsync(User);
                post.AuthorId = user.Id;
                _context.Add(post);
                if (post.CategoryIds != null)
                {
                    foreach (var CateId in post.CategoryIds)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            CategoryId = CateId,
                            Post = post
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Vua tao bai viet moi";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // var error = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                // error.ForEach(p=>Console.WriteLine(p));
            }
            return View(post);

        }

    }

}