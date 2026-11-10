using MiniSocialMedia.Models;
using System.Linq;
using System.Web.Mvc;

namespace MiniSocialMedia.Controllers
{
    public class HomeController : Controller
    {
        private MyDbContext db = new MyDbContext();

        public ActionResult Index()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Newsfeed", "Post");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Trang giới thiệu về ứng dụng mạng xã hội thu nhỏ.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Trang liên hệ hỗ trợ nhóm phát triển.";
            return View();
        }
    }
}