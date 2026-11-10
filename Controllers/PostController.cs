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

        private int GetCurrentUserId()
        {
            return Session["UserId"] != null ? (int)Session["UserId"] : 1;
        }

        public ActionResult Index(string searchString, int? page)
        {
            return RedirectToAction("Newsfeed", new { searchString = searchString, page = page });
        }

        [HttpGet]
        public ActionResult Create()
        {
            return RedirectToAction("Newsfeed");
        }

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
                .Where(p => p.UserId == currentUserId || followingIds.Contains(p.UserId));

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                postsQuery = postsQuery.Where(p => p.Content.Contains(searchString));
            }

            postsQuery = postsQuery.OrderByDescending(p => p.CreatedAt);

            var viewModel = new NewsfeedVM
            {
                Posts = postsQuery.ToPagedList(pageNumber, pageSize),
                CurrentUserId = currentUserId
            };

            var postIds = viewModel.Posts
                .Select(p => p.PostId)
                .ToList();

            ViewBag.LikeCounts = db.Likes
                .Where(l => postIds.Contains(l.PostID))
                .GroupBy(l => l.PostID)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.LikedPostIds = db.Likes
                .Where(l => postIds.Contains(l.PostID) && l.UserID == currentUserId)
                .Select(l => l.PostID)
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

            if (string.IsNullOrWhiteSpace(Content) && (ImageFile == null || ImageFile.ContentLength == 0))
            {
                TempData["Message"] = "Vui lòng nhập nội dung hoặc chọn ảnh để đăng bài!";
                return RedirectToAction("Newsfeed");
            }

            if (!string.IsNullOrWhiteSpace(Content) && Content.Trim().Length > 2000)
            {
                TempData["Message"] = "Nội dung không được vượt quá 2000 ký tự!";
                return RedirectToAction("Newsfeed");
            }

            string imageUrl = null;

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string extension = Path.GetExtension(fileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + extension;

                    string uploadDir = Server.MapPath("~/Content/Uploads/");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string path = Path.Combine(uploadDir, uniqueFileName);
                    ImageFile.SaveAs(path);

                    imageUrl = "/Content/Uploads/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Lỗi khi tải ảnh lên: " + ex.Message;
                    return RedirectToAction("Newsfeed");
                }
            }

            var post = new Post
            {
                UserId = currentUserId,
                Content = Content != null ? Content.Trim() : "",
                ImageUrl = imageUrl,
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

            var post = db.Posts.Find(postId);

            if (post == null)
            {
                TempData["Message"] = "Bài viết không tồn tại!";
                return RedirectToAction("Newsfeed");
            }

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

            var comment = new Comment
            {
                PostID = postId,
                UserID = currentUserId,
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

            if (comment.UserID != currentUserId)
            {
                TempData["Message"] = "Bạn không có quyền xóa bình luận này!";
                return RedirectToAction("Newsfeed");
            }

            db.Comments.Remove(comment);
            db.SaveChanges();

            TempData["Message"] = "Xóa bình luận thành công!";

            return RedirectToAction("Newsfeed");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditComment(int id, string Content)
        {
            int currentUserId = GetCurrentUserId();

            var comment = db.Comments.Find(id);

            if (comment == null)
            {
                TempData["Message"] = "Bình luận không tồn tại!";
                return RedirectToAction("Newsfeed");
            }

            if (comment.UserID != currentUserId)
            {
                TempData["Message"] = "Bạn không có quyền sửa bình luận này!";
                return RedirectToAction("Newsfeed");
            }

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

            comment.Content = Content.Trim();
            db.SaveChanges();

            TempData["Message"] = "Sửa bình luận thành công!";

            return RedirectToAction("Newsfeed");
        }

        [HttpPost]
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

        [HttpGet]
        public ActionResult EditPost(int id)
        {
            int currentUserId = GetCurrentUserId();

            var post = db.Posts.Find(id);

            if (post == null)
            {
                return HttpNotFound("Bài viết không tồn tại.");
            }

            if (post.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền sửa bài viết này.");
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditPost(Post model, HttpPostedFileBase ImageFile, bool removeImage = false)
        {
            int currentUserId = GetCurrentUserId();

            var post = db.Posts.Find(model.PostId);

            if (post == null)
            {
                return HttpNotFound("Bài viết không tồn tại.");
            }

            if (post.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền sửa bài viết này.");
            }

            string updatedContent = model.Content != null ? model.Content.Trim() : "";
            bool willHaveImage = (ImageFile != null && ImageFile.ContentLength > 0) || (!removeImage && !string.IsNullOrEmpty(post.ImageUrl));

            if (string.IsNullOrWhiteSpace(updatedContent) && !willHaveImage)
            {
                ModelState.AddModelError("Content", "Bài viết phải có ít nhất nội dung hoặc hình ảnh!");
            }

            if (updatedContent.Length > 2000)
            {
                ModelState.AddModelError("Content", "Nội dung không được vượt quá 2000 ký tự!");
            }

            if (!ModelState.IsValid)
            {
                return View(post);
            }

            if (removeImage)
            {
                post.ImageUrl = null;
            }

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string extension = Path.GetExtension(fileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + extension;

                    string uploadDir = Server.MapPath("~/Content/Uploads/");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string path = Path.Combine(uploadDir, uniqueFileName);
                    ImageFile.SaveAs(path);

                    post.ImageUrl = "/Content/Uploads/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi tải ảnh mới: " + ex.Message);
                    return View(post);
                }
            }

            post.Content = updatedContent;
            db.SaveChanges();

            TempData["Message"] = "Cập nhật bài viết thành công!";

            return RedirectToAction("Newsfeed");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleLike(int postId, string type = "Like")
        {
            int currentUserId = GetCurrentUserId();

            var post = db.Posts.Find(postId);

            if (post == null)
            {
                return Json(new { success = false, message = "Bài viết không tồn tại!" });
            }

            var existingLike = db.Likes.FirstOrDefault(l => l.PostID == postId && l.UserID == currentUserId);

            string userReaction = null;

            if (existingLike != null)
            {
                if (existingLike.ReactionType == type)
                {
                    db.Likes.Remove(existingLike);
                    userReaction = null;
                }
                else
                {
                    existingLike.ReactionType = type;
                    userReaction = type;
                }
            }
            else
            {
                db.Likes.Add(new Like
                {
                    PostID = postId,
                    UserID = currentUserId,
                    ReactionType = type,
                    CreatedAt = DateTime.Now
                });

                userReaction = type;
            }

            db.SaveChanges();

            int totalLikes = db.Likes.Count(l => l.PostID == postId);

            return Json(new { success = true, userReaction = userReaction, totalLikes = totalLikes });
        }

        public ActionResult TestConnection()
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    int count = db.Posts.Count();
                    return Content("Kết nối SQL Server thành công! Số bài viết: " + count);
                }
            }
            catch (Exception ex)
            {
                return Content("Lỗi kết nối: " + ex.Message);
            }
        }
    }
}