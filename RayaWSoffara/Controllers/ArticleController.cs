using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RayaWSoffara.Models;
using System.Net;
using System.Configuration;
using System.IO;
using TestImageCrop;
using RayaWSoffara.Filters;
using RWSDataLayer.Repositories;
using RWSDataLayer.Context;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using RayaWSoffara.Helpers;
using System.Web.Script.Serialization;
using Newtonsoft.Json;  

namespace RayaWSoffara.Controllers
{
    public class ArticleController : Controller
    {
        private ArticleRepository _articleRepo;
        private UserRepository _userRepo;
        public ArticleController()
        {
            _articleRepo = new ArticleRepository();
            _userRepo = new UserRepository();
        }

        [Authorize]
        public ActionResult Index()
        {
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult Write()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult Opinion()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult Image()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult Video()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult WriteTopX()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult GetImages()
        {
            ImageRepository _imgrepo = new ImageRepository();
            List<string> imgs = _imgrepo.GetAllImages().Select(i => i.ImageURL).ToList();

            return Json(new { Images = imgs }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public void UploadImage()
        {
            HttpPostedFileBase uploads = Request.Files["upload"];
            string CKEditorFuncNum = Request["CKEditorFuncNum"];
            string file = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(uploads.FileName);
            string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), file);
            uploads.SaveAs(path);
            string url = "/Content/Article_Images/" + file;
            Response.Write("<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + url + "\");</script>");
            Response.End();
        }
 
        public bool IsReusable {
            get {
                return false;
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Write(UserArticleVM article, string article_picture_path, string video_url)
        {
            string remoteip = Request.UserHostAddress;
            string recaptcha = Request.Form["g-recaptcha-response"];
            bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
            if (valid)
            {
                IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
                ViewBag.tags = articlesTags.ToList();
                RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
                article.newArticle.CreatedBy = currentUser.UserId;
                article.newArticle.CreationDate = DateTime.Now;
                article.newArticle.MetaTags = "";
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                HttpPostedFileBase picture = Request.Files[0];
                if (picture.FileName != "" || article_picture_path != "")
                {
                    if (article_picture_path != "")
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.Now.Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            article.newArticle.FeaturedImage = imgName;
                        }
                    }
                    else if (picture.FileName != "")
                    {
                        string picName = System.IO.Path.GetFileName(picture.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                        picture.SaveAs(path);
                        article.newArticle.FeaturedImage = picName;
                    }

                    article.newArticle.HasImage = true;
                }
                else
                    if (video_url != null || video_url != string.Empty)
                    {
                        article.newArticle.HasImage = false;
                        article.newArticle.FeaturedVideo = video_url;
                    }


                article.newArticle.Tags = null;
                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                article.newArticle.PostTypeId = 1;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);
                _articleRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
                if (addedArticle != null)
                {
                    ViewBag.ErrorMsg = 0;
                    return RedirectToAction("ArticleDisplay", new { id = addedArticle.PostId });
                }
                else
                {
                    ViewBag.ErrorMsg = 1;
                    return View();
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Opinion(UserArticleVM article, string article_picture_path, string video_url)
        {
            string remoteip = Request.UserHostAddress;
            string recaptcha = Request.Form["g-recaptcha-response"];
            bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
            if (valid)
            {
                IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
                ViewBag.tags = articlesTags.ToList();
                RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
                article.newArticle.CreatedBy = currentUser.UserId;
                article.newArticle.CreationDate = DateTime.Now;
                article.newArticle.MetaTags = "";
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                HttpPostedFileBase picture = Request.Files[0];
                if (picture.FileName != "" || article_picture_path != "")
                {
                    if (article_picture_path != "")
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.Now.Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            article.newArticle.FeaturedImage = imgName;
                        }
                    }
                    else if (picture.FileName != "")
                    {
                        string picName = System.IO.Path.GetFileName(picture.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                        picture.SaveAs(path);
                        article.newArticle.FeaturedImage = picName;
                    }

                    article.newArticle.HasImage = true;
                }
                else if (video_url != null && video_url != "")
                {
                    article.newArticle.HasImage = false;
                    article.newArticle.FeaturedVideo = video_url;
                }
                else
                {
                    article.newArticle.HasImage = false;
                }

                article.newArticle.Tags = null;
                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                article.newArticle.PostTypeId = 3;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);
                _articleRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
                if (addedArticle != null)
                {
                    ViewBag.ErrorMsg = 0;
                    return RedirectToAction("ArticleDisplay", new { id = addedArticle.PostId });
                }
                else
                {
                    ViewBag.ErrorMsg = 1;
                    return View();
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Image(UserArticleVM article, string article_picture_path, string video_url)
        {
            string remoteip = Request.UserHostAddress;
            string recaptcha = Request.Form["g-recaptcha-response"];
            bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
            if (valid)
            {
                IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
                ViewBag.tags = articlesTags.ToList();
                RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
                article.newArticle.CreatedBy = currentUser.UserId;
                article.newArticle.CreationDate = DateTime.Now;
                article.newArticle.MetaTags = "";
                if (article.newArticle.Content == null)
                {
                    article.newArticle.Content = "";
                }
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                HttpPostedFileBase picture = Request.Files[0];
                if (picture.FileName != "" || article_picture_path != "")
                {
                    if (article_picture_path != "")
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.Now.Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            article.newArticle.FeaturedImage = imgName;
                            article.newArticle.PostTypeId = 4;
                        }
                    }
                    else if (picture.FileName != "")
                    {
                        string picName = System.IO.Path.GetFileName(picture.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                        picture.SaveAs(path);
                        article.newArticle.FeaturedImage = picName;
                        article.newArticle.PostTypeId = 4;
                    }

                    article.newArticle.HasImage = true;
                }
                else if (video_url != null || video_url != string.Empty)
                {
                    article.newArticle.HasImage = false;
                    article.newArticle.FeaturedVideo = video_url;
                    article.newArticle.PostTypeId = 5;
                }

                article.newArticle.Tags = null;
                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);
                _articleRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
                if (addedArticle != null)
                {
                    ViewBag.ErrorMsg = 0;
                    return RedirectToAction("ArticleDisplay", new { id = addedArticle.PostId });
                }
                else
                {
                    ViewBag.ErrorMsg = 1;
                    return View();
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Video(UserArticleVM article, string article_picture_path, string video_url)
        {
            string remoteip = Request.UserHostAddress;
            string recaptcha = Request.Form["g-recaptcha-response"];
            bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
            if (valid)
            {
                IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
                ViewBag.tags = articlesTags.ToList();
                RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
                article.newArticle.CreatedBy = currentUser.UserId;
                article.newArticle.CreationDate = DateTime.Now;
                article.newArticle.MetaTags = "";
                if (article.newArticle.Content == null)
                {
                    article.newArticle.Content = "";
                }
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                HttpPostedFileBase picture = Request.Files[0];
                if (picture.FileName != "" || article_picture_path != "")
                {
                    if (article_picture_path != "")
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.Now.Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            article.newArticle.FeaturedImage = imgName;
                            article.newArticle.PostTypeId = 4;
                        }
                    }
                    else if (picture.FileName != "")
                    {
                        string picName = System.IO.Path.GetFileName(picture.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                        picture.SaveAs(path);
                        article.newArticle.FeaturedImage = picName;
                        article.newArticle.PostTypeId = 4;
                    }

                    article.newArticle.HasImage = true;
                }
                else if (video_url != null || video_url != string.Empty)
                {
                    article.newArticle.HasImage = false;
                    article.newArticle.FeaturedVideo = video_url;
                    article.newArticle.PostTypeId = 5;
                }

                article.newArticle.Tags = null;
                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);
                _articleRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
                if (addedArticle != null)
                {
                    ViewBag.ErrorMsg = 0;
                    return RedirectToAction("ArticleDisplay", new { id = addedArticle.PostId });
                }
                else
                {
                    ViewBag.ErrorMsg = 1;
                    return View();
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult WriteTopX(UserArticleVM article, string[] article_picture_path, string[] video_url)
        {
            string remoteip = Request.UserHostAddress;
            string recaptcha = Request.Form["g-recaptcha-response"];
            bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
            if (valid)
            {
                IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
                ViewBag.tags = articlesTags.ToList();
                RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
                article.newArticle.CreatedBy = currentUser.UserId;
                article.newArticle.CreationDate = DateTime.Now;
                article.newArticle.MetaTags = "";
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();

                int count = 0;
                foreach (string img_path in article_picture_path)
                {
                    if (count > 0)
                    {
                        HttpPostedFileBase picture = Request.Files[count];
                        if (picture.FileName != "" || img_path != "")
                        {
                            if (img_path != "")
                            {
                                string path = AppDomain.CurrentDomain.BaseDirectory + img_path;
                                if (System.IO.File.Exists(path))
                                {
                                    string[] separator = new string[] { "Temp/" };
                                    string[] temp = img_path.Split(separator, StringSplitOptions.None);
                                    string imgName = DateTime.Now.Ticks + "_" + temp[1];
                                    System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                                    article.newArticle.ArticleTopXes.ElementAt(count - 1).TopXImage = imgName;
                                }
                            }
                            else if (picture.FileName != "")
                            {
                                string picName = System.IO.Path.GetFileName(picture.FileName);
                                // string newName = new Random().ne System.IO.Path.GetExtension(picture.FileName);
                                string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                                picture.SaveAs(path);
                                article.newArticle.ArticleTopXes.ElementAt(count).TopXImage = picName;
                            }
                        }
                        else if (video_url != null || video_url[count] != string.Empty)
                        {
                            article.newArticle.ArticleTopXes.ElementAt(count).TopXVideo = video_url[count];
                        }
                    }
                    else
                    {
                        HttpPostedFileBase picture = Request.Files[0];
                        if (picture.FileName != "" || img_path != "")
                        {
                            if (img_path != "")
                            {
                                string path = AppDomain.CurrentDomain.BaseDirectory + img_path;
                                if (System.IO.File.Exists(path))
                                {
                                    string[] separator = new string[] { "Temp/" };
                                    string[] temp = img_path.Split(separator, StringSplitOptions.None);
                                    string imgName = DateTime.Now.Ticks + "_" + temp[1];
                                    System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                                    article.newArticle.FeaturedImage = imgName;
                                }
                            }
                            else if (picture.FileName != "")
                            {
                                string picName = System.IO.Path.GetFileName(picture.FileName);
                                string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                                picture.SaveAs(path);
                                article.newArticle.FeaturedImage = picName;
                            }

                            article.newArticle.HasImage = true;
                        }
                        else if (video_url != null || video_url[count] != string.Empty)
                        {
                            article.newArticle.HasImage = false;
                            article.newArticle.FeaturedVideo = video_url[count];
                            article.newArticle.FeaturedImage = img_path;
                        }
                    }
                    count++;
                }

                article.newArticle.Tags = null;
                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.PostTypeId = 2;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);
                _articleRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
                if (addedArticle != null)
                {
                    //_articleRepo.UpdatedArticleTags(addedArticle.ArticleId, tags);
                    ViewBag.ErrorMsg = 0;
                    return RedirectToAction("ArticleDisplay", new { id = addedArticle.PostId });
                }
                else
                {
                    ViewBag.ErrorMsg = 1;
                    return View();
                }
            }
            return View();
        }

        private string EncodeTo64(string toEncode)
        {
            var toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
            var returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private static byte[] PadLines(byte[] bytes, int rows, int columns)
        {
            //The old and new offsets could be passed through parameters,
            //but I hardcoded them here as a sample.
            var currentStride = columns * 3;
            var newStride = columns * 4;
            var newBytes = new byte[newStride * rows];
            for (var i = 0; i < rows; i++)
                Buffer.BlockCopy(bytes, currentStride * i, newBytes, newStride * i, currentStride);
            return newBytes;
        }

        [HttpPost]
        public ActionResult TestCrop(String data, double left, double top, double imageWidth, double imageHeight, double imageOriginalWidth, double imageOriginalHeight)
        {
            Dictionary<string, string> result = CropImage(data, left, top, imageWidth, imageHeight, imageOriginalWidth, imageOriginalHeight);
            string response = JsonConvert.SerializeObject(result);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        protected static System.Drawing.Image ByteArrayToBitmap(byte[] blob)
        {
            using (var mStream = new MemoryStream())
            {
                MemoryStream memoryStream = new MemoryStream();
                foreach (var b in blob) memoryStream.WriteByte(b);
                memoryStream.Position = 0;
                System.Drawing.Image image = System.Drawing.Image.FromStream(memoryStream);
                return image;
            }
        }

        [HttpPost]
        //public virtual ActionResult CropImage(String imagePath, double? cropPointX, double? cropPointY, double? imageCropWidth, double? imageCropHeight, double? originalWidth, double? originalHeight)
        public Dictionary<string, string> CropImage(String imagePath, double? cropPointX, double? cropPointY, double? imageCropWidth, double? imageCropHeight, double? originalWidth, double? originalHeight)
        {
            if (string.IsNullOrEmpty(imagePath) || !cropPointX.HasValue || !cropPointY.HasValue || !imageCropWidth.HasValue || !imageCropHeight.HasValue)
            {
                //return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
                return null;
            }
            string path = AppDomain.CurrentDomain.BaseDirectory + imagePath;
            byte[] imageBytes;
            byte[] croppedImage;
            byte[] newbytes;
            if (imagePath.Length > 260)
            {
                String imageDataParsed = imagePath.Substring(imagePath.IndexOf(',') + 1);
                imageBytes = Convert.FromBase64String(imageDataParsed);

                var columns = (int)originalWidth;
                var rows = (int)originalHeight;
                var stride = columns * 4;
                MemoryStream stream = new MemoryStream(imageBytes);
                int len = imageBytes.Length;

                croppedImage = ImageHelper.CropImage(stream, (int)cropPointX, (int)cropPointY, Convert.ToInt16(imageCropWidth.Value), Convert.ToInt16(imageCropHeight.Value));
            }
            else
            {
                imageBytes = System.IO.File.ReadAllBytes(path);
                int len2 = imageBytes.Length;
                MemoryStream stream = new MemoryStream(imageBytes);
                croppedImage = ImageHelper.CropImage(stream, Convert.ToInt16(cropPointX.Value), Convert.ToInt16(cropPointY.Value), Convert.ToInt16(imageCropWidth.Value), Convert.ToInt16(imageCropHeight.Value));
            }


            string tempFolderName = Server.MapPath("~/" + ConfigurationManager.AppSettings["Image.TempFolderName"]);
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string newName = fileName + new Random().Next().ToString() + ".jpg";

            try
            {
                FileHelper.SaveFile(croppedImage, Path.Combine(tempFolderName, newName));
            }
            catch (Exception ex)
            {
                //Log an error     
                //return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
                return null;
            }

            string photoPath = string.Concat("/", ConfigurationManager.AppSettings["Image.TempFolderName"], "/", newName);
            //return Json(new { photoPath = photoPath }, JsonRequestBehavior.AllowGet);
            var response = new Dictionary<string, string>();
            response["success"] = "success";
            response["url"] = photoPath;
            response["filename"] = newName;
            return response;
        }

        public void AddShareCount(int PostId)
        {
            int UserId = _userRepo.GetUserByUsername(User.Identity.Name).UserId;
            _articleRepo.AddShareCount(PostId, UserId);
        }

        public void AddLikeCount(int PostId)
        {
            int UserId = _userRepo.GetUserByUsername(User.Identity.Name).UserId;
            _articleRepo.AddLikeCount(PostId, UserId);
        }

        public ActionResult GetComments(int Index, int PostId)
        {
            List<Comment> comments = _articleRepo.GetComments(Index, PostId);
            UserArticleVM result = new UserArticleVM();
            result.Comments = comments;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_CommentPartial", result);
            }
            return View("ArticleDisplay", comments);
        }

        public string isPostLikedByUser(int PostId)
        {
            if (User.Identity.Name != null && User.Identity.Name != "")
            {
                int UserId = _userRepo.GetUserByUsername(User.Identity.Name).UserId;
                bool liked = _articleRepo.isPostLikedByUser(PostId, UserId);
                if (liked)
                {
                    return "liked";
                }
                else
                {
                    return "not liked";
                }
            }
            return "not a member";
        }

        public ActionResult ArticleDisplay(int id)
        {
            // start of getting current user profile picture //
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;
                ViewBag.CurrentUserPicture = _userRepo.GetUserByUsername(username).ProfileImagePath;
            }
            // end o getting current user profile picture //

            // start of checking if the current user is the author of the post //
            if (User.Identity.Name != _articleRepo.GetPostById(id).RWSUser.UserName)
            {
                _articleRepo.AddViewsCount(id, User.Identity.Name);
            }
            // end of checking if the current user is the author of the post //

            UserArticleVM articleData = new UserArticleVM();
            articleData.newArticle = _articleRepo.GetPostById(id);
            articleData.userArticles = _articleRepo.GetAllUserPosts(articleData.newArticle.CreatedBy, 5).ToList();
            //articleData.newArticle.article_content = _articleRepo.RemoveHTMLTags(articleData.newArticle.article_content);
            _articleRepo.UpdatedArticleViewsCounter(id);
            ViewBag.userViewsCount = _articleRepo.GetViewsCountByUserId(articleData.newArticle.CreatedBy);
            List<Post> simillarArticles = _articleRepo.GetPostsWithTagIDs(articleData.newArticle.Tags.Select(i => i.TagId).ToList(), 5).ToList();
            ViewBag.simillarArticles = simillarArticles;

            ViewBag.AllCommentsCount = _articleRepo.GetAllComments(id).Count();
            articleData.Comments = _articleRepo.GetComments(0, id).OrderBy(i => i.CommentCreationDate).ToList();

            ViewBag.ArticleTopXes = articleData.newArticle.ArticleTopXes;

            CompetitionRepository _compRepo = new CompetitionRepository();
            if (articleData.newArticle.RWSUser.FavTeam != null)
            {
                ViewBag.FavTeamName = _compRepo.GetTeamById(articleData.newArticle.RWSUser.FavTeam.Value).TeamName;
            }
            else
            {
                ViewBag.FavTeamName = "N/A";
            }

            if (articleData.newArticle.RWSUser.FavComp != null)
            {
                ViewBag.FavCompName = _compRepo.GetCompetetionById(articleData.newArticle.RWSUser.FavComp.Value).CompetitionName;
            }
            else
            {
                ViewBag.FavCompName = "N/A";
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_CommentPartial", articleData);
            }
            return View(articleData);
        }

        public void AddComment(string PostId, string CommentText, string Username)
        {
            int post_id = Int32.Parse(PostId);
            _articleRepo.AddComment(post_id, CommentText, Username);
        }

        public JsonResult GetTagIdbyName(string tagName)
        {
            int tagId = _articleRepo.GetTagByName(tagName).TagId;
            return Json(tagId, JsonRequestBehavior.AllowGet);
        }

    }
}
