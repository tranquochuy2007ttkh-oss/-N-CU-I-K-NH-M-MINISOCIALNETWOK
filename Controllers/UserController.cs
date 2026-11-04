using MiniSocialMedia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ĐỀ_ÁN_CUỐI_KỲ_NHÓM_MINISOCIALNETWOK.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        private MyDbContext db = new MyDbContext();
        private int GetCurrentUserId()
        {
            return Session["UserId"] != null
                ? (int)Session["UserId"]
                : 1;
        }

        // Hiển thị danh sách người dùng
        public ActionResult Index(string searchString)
        {
            var users = db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                users = users.Where(u =>
                    u.Username.Contains(searchString) ||
                    u.FullName.Contains(searchString));
            }

            int currentUserId = GetCurrentUserId();

            // Lấy danh sách ID người mà mình đang theo dõi
            ViewBag.FollowingIds = db.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId)
                .ToList();

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.SearchString = searchString;

            return View(users.ToList());
        }
        // Hiển thị trang chỉnh sửa
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // Lưu thông tin sau khi chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User model)
        {
            var user = db.Users.Find(model.UserId);

            if (user == null)
                return HttpNotFound();

            // Không kiểm tra PasswordHash khi chỉnh sửa hồ sơ
            ModelState.Remove("PasswordHash");

            if (!ModelState.IsValid)
                return View(user);

            user.Username = model.Username.Trim();
            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.AvatarUrl = model.AvatarUrl;
            user.Bio = model.Bio;

            db.SaveChanges();

            TempData["Message"] = "Cập nhật thông tin thành công!";

            return RedirectToAction("Index");
        }
        // Theo dõi hoặc bỏ theo dõi người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleFollow(int followingId)
        {
            int currentUserId = GetCurrentUserId();

            // Không cho phép tự theo dõi chính mình
            if (currentUserId == followingId)
            {
                TempData["Message"] = "Bạn không thể tự theo dõi chính mình!";
                return RedirectToAction("Index");
            }

            // Kiểm tra người được theo dõi có tồn tại không
            var user = db.Users.Find(followingId);

            if (user == null)
                return HttpNotFound();

            // Kiểm tra đã theo dõi hay chưa
            var existingFollow = db.Follows.FirstOrDefault(f =>
                f.FollowerId == currentUserId &&
                f.FollowingId == followingId);

            if (existingFollow != null)
            {
                // Đã theo dõi thì bỏ theo dõi
                db.Follows.Remove(existingFollow);
                TempData["Message"] = "Đã bỏ theo dõi!";
            }
            else
            {
                // Chưa theo dõi thì thêm mới
                db.Follows.Add(new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = followingId,
                    CreatedAt = DateTime.Now
                });

                TempData["Message"] = "Đã theo dõi người dùng!";
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}