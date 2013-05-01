using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity.Models;
using DataAccess;


namespace MvcBasic.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var productRepository = new Repository<Products>();

            Products p = new Products { Name = "Nasi Goreng", Price = 2.5 };
            productRepository.SaveOrUpdate(p);

            return View(productRepository.GetAll());
        }

    }
}
