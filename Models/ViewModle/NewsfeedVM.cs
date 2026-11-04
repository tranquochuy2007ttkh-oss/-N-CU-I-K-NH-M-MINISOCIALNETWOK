using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
namespace MiniSocialMedia.Models.ViewModle
{
    public class NewsfeedVM
    {
        public IPagedList<Post> Posts { get; set; }
        public string NewPostContent { get; set; }
        public int CurrentUserId { get; set; }
    }
}