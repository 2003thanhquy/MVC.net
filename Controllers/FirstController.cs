using App.Services;
using Microsoft.AspNetCore.Mvc;
namespace App.Controllers;

public class  FirstController : Controller{

    private readonly ILogger<FirstController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly ProductService _productService;
    public FirstController(ILogger<FirstController> logger,IWebHostEnvironment env, ProductService productService){
        _logger = logger;
        _env = env;
        _productService = productService;   
    }
    //this.HttpContext
    //this.Request
    //this.Response
    //this.routeData

    //this.User
    //this.ModelState
    //this.ViewData
    //this.ViewBag
    //this.Url
    //this.TempData

    public IActionResult Bird(){
       
        string filepath = Path.Combine(_env.ContentRootPath,"Files","Bird.jpg");

        var bytes = System.IO.File.ReadAllBytes(filepath);   

        return File(bytes,"image/jpg");
        
    }
    public JsonResult PricePhone(){
        return Json(new {
            
            name ="Phone",
            PricePhone = "100k"
        });
    }
    public IActionResult Google(){
        string url="https://google.com";
        return  Redirect(url);
    }
    public IActionResult xinchao1(){
        return View();
    }
    public IActionResult ViewProduct(int? id){
        var product = _productService.Where(p => p.Id  == id.Value).FirstOrDefault();
        if(product == null){
            return NotFound();
        }
        return View(product);
    }
    public IActionResult ViewProduct2(int? id){
        var product = _productService.Where(p => p.Id  == id.Value).FirstOrDefault();
        if(product == null){
            return NotFound();
        }
        ViewData["product"] = product;
        ViewData["Titile"] =$"San pham {id}";
        return View();
    }
    
     public IActionResult ViewProduct3(int? id){
        var product = _productService.Where(p => p.Id  == id.Value).FirstOrDefault();
        if(product == null){
            return NotFound();
        }
       ViewBag.product = product;
        return View();
    }
    public string Index() =>"day la Index";
    public string IHai() =>"day la IHai";

}