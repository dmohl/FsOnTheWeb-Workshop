using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GuitarExample.Models;

namespace GuitarExample.Controllers
{
    public class GuitarsController : Controller
    {
        protected IList<Guitar> Guitars = new List<Guitar>();

        public GuitarsController()
        {
            if (System.IO.File.Exists(@"c:\temp\Guitars.txt"))
            {
                var fileContent = System.IO.File.ReadAllText(@"c:\temp\Guitars.txt");
                Guitars = fileContent.Split(',').Where(x => !string.IsNullOrEmpty(x.Trim())).Select(x => new Guitar {Name = x}).ToList();
            }
        }

        public ActionResult Index()
        {
            return View(Guitars);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Guitar guitar)
        {
            var isNameValue = !string.IsNullOrEmpty(guitar.Name);
            if (!isNameValue && !ModelState.IsValid || isNameValue && !ModelState.IsValid || !isNameValue && ModelState.IsValid)
            {
                return View(guitar);
            }
            else
            {
                var content = Guitars.Aggregate("", (acc, x) => acc + x.Name + ",");
                System.IO.File.WriteAllText(@"c:\temp\Guitars.txt", content + guitar.Name);
                return RedirectToAction("Index");
            }
        }
    }
}
