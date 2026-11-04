using MiniSocialMedia.Models;
using MiniSocialMedia.Models.ViewModle;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace MiniSocialMedia.Controllers
{
    public class PostController : Controller
    {
        private MyDbContext db = new MyDbContext();

        // Lấy ID người dùng đang đăng nhập từ Session
        private int GetCurrentUserId()
        {
            return Session["UserId"] != null ? (int)Session["UserId"] : 1; // Giả lập UserId = 1 nếu chưa đăng nhập
        }
        // GET: Post
        public ActionResult Newsfeed(string searchString, int? page)
        {
            int currentUserId = GetCurrentUserId();
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var followingIds = db.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId)
                .ToList();

            var postsQuery = db.Posts
                .Where(p => p.UserId == currentUserId ||
                            followingIds.Contains(p.UserId));

            // Tìm kiếm theo nội dung bài viết
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                postsQuery = postsQuery.Where(p =>
                    p.Content.Contains(searchString));
            }

            // Sắp xếp bài viết mới nhất
            postsQuery = postsQuery
                .OrderByDescending(p => p.CreatedAt);

            var viewModel = new NewsfeedVM
            {
                Posts = postsQuery.ToPagedList(pageNumber, pageSize),
                CurrentUserId = currentUserId
            };

            // Lấy ID các bài viết đang hiển thị
            var postIds = viewModel.Posts
                .Select(p => p.PostId)
                .ToList();

            // Lấy tổng số lượt Like của từng bài viết
            ViewBag.LikeCounts = db.Likes
                .Where(l => postIds.Contains(l.PostId))
                .GroupBy(l => l.PostId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Lấy danh sách bài viết mà người dùng hiện tại đã Like
            ViewBag.LikedPostIds = db.Likes
                .Where(l => postIds.Contains(l.PostId)
                         && l.UserId == currentUserId)
                .Select(l => l.PostId)
                .ToList();

            ViewBag.SearchString = searchString;

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreatePost(string Content, HttpPostedFileBase ImageFile)
        {
            int currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["Message"] = "Nội dung bài viết không được để trống!";
                return RedirectToAction("Newsfeed");
            }

            if (Content.Trim().Length > 2000)
            {
                TempData["Message"] = "Nội dung không được vượt quá 2000 ký tự!";
                return RedirectToAction("Newsfeed");
            }

            var post = new Post
            {
                UserId = currentUserId,
                Content = Content.Trim(),
                CreatedAt = DateTime.Now
            };

            db.Posts.Add(post);
            db.SaveChanges();

            TempData["Message"] = "Đăng bài viết thành công!";

            return RedirectToAction("Newsfeed");
        }
        public ActionResult CreateComment(int postId, string Content)
        {
            int currentUserId = GetCurrentUserId();

            // Kiểm tra bài viết có tồn tại không
            var post = db.Posts.Find(postId);

            if (post == null)
            {
                TempData["Message"] = "Bài viết không tồn tại!";
                return RedirectToAction("Newsfeed");
            }

            // Kiểm tra nội dung
            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["Message"] = "Nội dung bình luận không được để trống!";
                return RedirectToAction("Newsfeed");
            }

            if (Content.Trim().Length > 1000)
            {
                TempData["Message"] = "Bình luận tối đa 1000 ký tự!";
                return RedirectToAction("Newsfeed");
            }

            // Tạo bình luận mới
            var comment = new Comment
            {
                PostId = postId,
                UserId = currentUserId,
                Content = Content.Trim(),
                CreatedAt = DateTime.Now
            };

            db.Comments.Add(comment);
            db.SaveChanges();

            TempData["Message"] = "Bình luận thành công!";

            return RedirectToAction("Newsfeed");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(int id)
        {
            int currentUserId = GetCurrentUserId();

            var comment = db.Comments.Find(id);

            if (comment == null)
            {
                TempData["Message"] = "Bình luận không tồn tại!";
                return RedirectToAction("Newsfeed");
            }

            if (comment.UserId != currentUserId)
            {
                TempData["Message"] = "Bạn không có quyền xóa bình luận này!";
                return RedirectToAction("Newsfeed");
            }

            db.Comments.Remove(comment);
            db.SaveChanges();

            TempData["Message"] = "Xóa bình luận thành công!";

            return RedirectToAction("Newsfeed");
        }
        // SỬA BÌNH LUẬN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditComment(int id, string Content)
        {
            int currentUserId = GetCurrentUserId();

            // Tìm bình luận cần sửa
            var comment = db.Comments.Find(id);

            if (comment == null)
            {
                TempData["Message"] = "Bình luận không tồn tại!";
                return RedirectToAction("Newsfeed");
            }

            // Chỉ người tạo bình luận mới được sửa
            if (comment.UserId != currentUserId)
            {
                TempData["Message"] = "Bạn không có quyền sửa bình luận này!";
                return RedirectToAction("Newsfeed");
            }

            // Kiểm tra nội dung
            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["Message"] = "Nội dung bình luận không được để trống!";
                return RedirectToAction("Newsfeed");
            }

            if (Content.Trim().Length > 1000)
            {
                TempData["Message"] = "Bình luận tối đa 1000 ký tự!";
                return RedirectToAction("Newsfeed");
            }

            // Cập nhật nội dung bình luận
            comment.Content = Content.Trim();

            db.SaveChanges();

            TempData["Message"] = "Sửa bình luận thành công!";

            return RedirectToAction("Newsfeed");
        }

        // 3. XÓA BÀI ĐĂNG (Ràng buộc nghiệp vụ: Chỉ xóa bài của chính mình)
        [HttpPost]
        // POST: Post/DeletePost
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(int id)
        {
            int currentUserId = GetCurrentUserId();

            var post = db.Posts.Find(id);

            if (post == null)
            {
                TempData["Message"] = "Bài viết không tồn tại!";
                return RedirectToAction("Newsfeed");
            }

            // Chỉ chủ bài viết mới được xóa
            if (post.UserId != currentUserId)
            {
                TempData["Message"] = "Bạn không có quyền xóa bài viết này!";
                return RedirectToAction("Newsfeed");
            }

            db.Posts.Remove(post);
            db.SaveChanges();

            TempData["Message"] = "Xóa bài viết thành công!";

            return RedirectToAction("Newsfeed");
        }

        // 4. THẢ LIKE / UNLIKE (AJAX)
        // GET: Post/EditPost/1
        [HttpGet]
        public ActionResult EditPost(int id)
        {
            int currentUserId = GetCurrentUserId();

            var post = db.Posts.Find(id);

            if (post == null)
            {
                return HttpNotFound("Bài viết không tồn tại.");
            }

            // Chỉ chủ bài viết mới được sửa
            if (post.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(
                    403, "Bạn không có quyền sửa bài viết này.");
            }

            return View(post);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(Post model)
        {
            int currentUserId = GetCurrentUserId();

            var post = db.Posts.Find(model.PostId);

            if (post == null)
            {
                return HttpNotFound("Bài viết không tồn tại.");
            }

            // Kiểm tra quyền sở hữu
            if (post.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(403);
            }

            // Kiểm tra nội dung
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                ModelState.AddModelError(
                    "Content",
                    "Nội dung không được để trống.");
            }
            else if (model.Content.Trim().Length > 2000)
            {
                ModelState.AddModelError(
                    "Content",
                    "Nội dung tối đa 2000 ký tự.");
            }

            if (!ModelState.IsValid)
            {
                return View(post);
            }

            post.Content = model.Content.Trim();

            db.SaveChanges();

            TempData["Message"] = "Cập nhật bài viết thành công!";

            return RedirectToAction("Newsfeed");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleLike(int postId)
        {
            int currentUserId = GetCurrentUserId();

            // Kiểm tra bài viết có tồn tại không
            var post = db.Posts.Find(postId);

            if (post == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Bài viết không tồn tại!"
                });
            }

            // Kiểm tra người dùng đã Like bài viết chưa
            var existingLike = db.Likes.FirstOrDefault(l =>
                l.PostId == postId &&
                l.UserId == currentUserId);

            bool isLiked;

            if (existingLike != null)
            {
                // Đã Like thì xóa để Unlike
                db.Likes.Remove(existingLike);
                isLiked = false;
            }
            else
            {
                // Chưa Like thì thêm mới
                db.Likes.Add(new Like
                {
                    PostId = postId,
                    UserId = currentUserId,
                    CreatedAt = DateTime.Now
                });

                isLiked = true;
            }

            db.SaveChanges();

            int totalLikes = db.Likes.Count(l => l.PostId == postId);

            return Json(new
            {
                success = true,
                isLiked = isLiked,
                totalLikes = totalLikes
            });
        }
        public ActionResult TestConnection()
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    int count = db.Posts.Count();

                    return Content(
                        "Kết nối SQL Server thành công! " +
                        "Số bài viết: " + count
                    );
                }
            }
            catch (Exception ex)
            {
                return Content("Lỗi kết nối: " + ex.Message);
            }
        }
    }
}