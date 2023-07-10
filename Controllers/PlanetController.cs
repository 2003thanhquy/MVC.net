using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class PlanetController : Controller
    {
        private readonly PlanetService _planet;
        private readonly ILogger<PlanetController> _logger;  
        private readonly IWebHostEnvironment _env;
        public PlanetController (PlanetService planet,ILogger<PlanetController> logger,IWebHostEnvironment env){
            _planet = planet;
            _logger = logger;
            _env = env;
        }
        [Route("Day-la-index.html")]
        public IActionResult Index()
        {
            return View();
        }
        [BindProperty(SupportsGet =true,Name = "action")]
        public string Name {get;set;}
        public IActionResult Mercury(){

            var planet = _planet.Where(p=>p.Name == Name).FirstOrDefault();
            return View("Detail",planet);

        }
        public IActionResult PlanetInfo(int? id){
            var planet = _planet.Where(p => p.Id == id).FirstOrDefault();
            if(planet==null){
                return Content("Day la Planet");
            }
            return View("Detail",planet);
        }
    }
}