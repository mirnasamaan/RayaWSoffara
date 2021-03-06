﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RayaWSoffara.Models;
using System.Net;
using System.Configuration;
using System.IO;
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
using System.Web.Helpers;
using Newtonsoft.Json;  

namespace RayaWSoffara.Controllers
{
    public class ArticleController : SearchController
    {
        private ArticleRepository _articleRepo;
        private UserRepository _userRepo;
        private EngagementRepository _engRepo = new EngagementRepository();
        private static List<string> tempImages = new List<string>();

        public ArticleController()
        {
            _articleRepo = new ArticleRepository();
            _userRepo = new UserRepository();
        }

        [Authorize]
        public ActionResult Index()
        {
            if (User.Identity.Name != "")
            {
                UserArticleVM model = new UserArticleVM();
                return View(model);
            }
            else
            {
                return Redirect("/Login");
            }
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult Create(string Type)
        {
            if (User.Identity.Name != "")
            {
                IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
                ViewBag.tags = articlesTags.ToList();
                ViewBag.Images = GetImages(0);
                ViewBag.Type = Type;
                ImageRepository _imgrepo = new ImageRepository();
                ViewBag.AllImagesCount = _imgrepo.GetAll().Count();
                UserArticleVM model = new UserArticleVM();
                return View(model);
            }
            else
            {
                return Redirect("Login");
            }
        }

        [Authorize]
        public ActionResult Write()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            ViewBag.Images = GetImages(0);
            ImageRepository _imgrepo = new ImageRepository();
            ViewBag.AllImagesCount = _imgrepo.GetAll().Count();
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult Opinion()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            ViewBag.Images = GetImages(0);
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public ActionResult Image()
        {
            IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
            ViewBag.tags = articlesTags.ToList();
            ViewBag.Images = GetImages(0);
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
            ViewBag.Images = GetImages(0);
            UserArticleVM model = new UserArticleVM();
            return View(model);
        }

        [Authorize]
        public List<RWSDataLayer.Context.Image> GetImages(int page, string TagNames = null)
        {
            ImageRepository _imgrepo = new ImageRepository();
            List<int> tagids = new List<int>();
            if (TagNames != null && TagNames != "")
            {
                string[] Tags = TagNames.Split(',');
                foreach (var item in Tags)
                {
                    tagids.Add(_articleRepo.GetTagByName(HttpUtility.UrlDecode(item)).TagId);
                }
            }
            List<RWSDataLayer.Context.Image> imgs = _imgrepo.GetAllImages(page, tagids).ToList();
            return imgs;
        }

        [Authorize]
        public ActionResult GetImagesAjax(int page, string TagNames = null)
        {
            ImageRepository _imgrepo = new ImageRepository(); 
            List<int> tagids = new List<int>();
            if (TagNames != null && TagNames != "")
            {
                string[] Tags = TagNames.Split(',');
                foreach (var item in Tags)
                {
                    tagids.Add(_articleRepo.GetTagByName(HttpUtility.UrlDecode(item)).TagId);
                }
            }
            List<RWSDataLayer.Context.Image> imgs = _imgrepo.GetAllImages(page, tagids).ToList();
            return PartialView("_ImagesPartial", imgs);
        }

        [Authorize]
        public void UploadImage()
        {
            HttpPostedFileBase uploads = Request.Files["upload"];
            string CKEditorFuncNum = Request["CKEditorFuncNum"];
            string file = DateTime.UtcNow.ToLocalTime().Ticks + "_" + System.IO.Path.GetFileName(uploads.FileName);
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
            //HttpPostedFileBase picture = null;
            string remoteip = Request.UserHostAddress;
            string recaptcha = Request.Form["g-recaptcha-response"];
            bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
            if (valid)
            {
                //IEnumerable<Tag> articlesTags = _articleRepo.GetTags();
                //ViewBag.tags = articlesTags.ToList();
                RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
                article.newArticle.CreatedBy = currentUser.UserId;
                article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
                article.newArticle.isOriginal = true;
                article.newArticle.MetaTags = "";
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                article.newArticle.Tags = tags;
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp.Last();
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }

                    article.newArticle.HasImage = true;
                }
                else
                    if (video_url != null || video_url != string.Empty)
                    {
                        article.newArticle.HasImage = false;
                        article.newArticle.FeaturedVideo = video_url;
                    }

                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                article.newArticle.PostTypeId = 1;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);

                foreach (var item in tempImages)
                {
                    string filePath = Server.MapPath("~/Temp/" + item);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

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
                article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
                article.newArticle.isOriginal = true;
                article.newArticle.MetaTags = "";
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                article.newArticle.Tags = tags;
               
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp.Last();
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
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

                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                article.newArticle.PostTypeId = 3;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);

                foreach (var item in tempImages)
                {
                    string filePath = Server.MapPath("~/Temp/" + item);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

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
                article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
                article.newArticle.isOriginal = true;
                article.newArticle.MetaTags = "";
                if (article.newArticle.Content == null)
                {
                    article.newArticle.Content = "";
                }
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                article.newArticle.Tags = tags;
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp.Last();
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }

                    article.newArticle.HasImage = true;
                }
                else if (video_url != null || video_url != string.Empty)
                {
                    article.newArticle.HasImage = false;
                    article.newArticle.FeaturedVideo = video_url;
                    article.newArticle.PostTypeId = 5;
                }

                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                article.newArticle.PostTypeId = 4;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);

                foreach (var item in tempImages)
                {
                    string filePath = Server.MapPath("~/Temp/" + item);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

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
                article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
                article.newArticle.isOriginal = true;
                article.newArticle.MetaTags = "";
                if (article.newArticle.Content == null)
                {
                    article.newArticle.Content = "";
                }
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                article.newArticle.Tags = tags;
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp.Last();
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }

                    article.newArticle.HasImage = true;
                }
                else if (video_url != null || video_url != string.Empty)
                {
                    article.newArticle.HasImage = false;
                    article.newArticle.FeaturedVideo = video_url;
                    article.newArticle.PostTypeId = 5;
                }

                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.SharesCount = 0;
                article.newArticle.PostTypeId = 5;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);

                foreach (var item in tempImages)
                {
                    string filePath = Server.MapPath("~/Temp/" + item);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

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
                article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
                article.newArticle.isOriginal = true;
                article.newArticle.MetaTags = "";
                List<Tag> tags = _articleRepo.getSelectedTags(article.SelectedTags).ToList();
                article.newArticle.Tags = tags;

                int count = 0;
                foreach (string img_path in article_picture_path)
                {
                    if (count > 0)
                    {
                        if (img_path != "")
                        {
                            string path = AppDomain.CurrentDomain.BaseDirectory + img_path;
                            if (System.IO.File.Exists(path))
                            {
                                string[] separator = new string[] { "/" };
                                string[] temp = img_path.Split(separator, StringSplitOptions.None);
                                string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp.Last();
                                System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                                article.newArticle.ArticleTopXes.ElementAt(count - 1).TopXImage = imgName;
                            }
                        }
                        else if (video_url != null || video_url[count] != string.Empty)
                        {
                            article.newArticle.ArticleTopXes.ElementAt(count - 1).TopXVideo = video_url[count];
                        }
                    }
                    else
                    {
                        if (img_path != "")
                        {
                            string path = AppDomain.CurrentDomain.BaseDirectory + img_path;
                            if (System.IO.File.Exists(path))
                            {
                                string[] separator = new string[] { "/" };
                                string[] temp = img_path.Split(separator, StringSplitOptions.None);
                                string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp.Last();
                                System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                                article.newArticle.FeaturedImage = imgName;
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

                article.newArticle.MetaTags = "";
                article.newArticle.ViewsCount = 0;
                article.newArticle.PostTypeId = 2;
                Post addedArticle = _articleRepo.AddPost(article.newArticle);

                foreach (var item in tempImages)
                {
                    string filePath = Server.MapPath("~/Temp/" + item);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

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

        [Authorize]
        public ActionResult ReportComment(int CommentId, string Username)
        {
            try
            {
                int userId = _userRepo.GetUserByUsername(Username).UserId;
                _articleRepo.ReportComment(CommentId, userId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) 
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
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
            //Dictionary<string, string> result = CropImage(data, left, top, imageWidth, imageHeight, imageOriginalWidth, imageOriginalHeight); 
            //var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //string response = jsonSerializer.Serialize(result);
            //string response = JsonConvert.SerializeObject(result);
            string path = AppDomain.CurrentDomain.BaseDirectory + data;
            byte[] imageBytes;
            byte[] croppedImage;
            byte[] newbytes;
            MemoryStream stream;
            if (data.Length > 260)
            {
                String imageDataParsed = data.Substring(data.IndexOf(',') + 1);
                imageBytes = Convert.FromBase64String(imageDataParsed);
                stream = new MemoryStream(imageBytes);
            }
            else
            {
                imageBytes = System.IO.File.ReadAllBytes(path);
                int len2 = imageBytes.Length;
                stream = new MemoryStream(imageBytes);
            }

            Bitmap sourceBitmap = new Bitmap(stream);
            croppedImage = ImageHelper.GetBitmapBytes(sourceBitmap);
            string tempFolderName = Server.MapPath("~/" + ConfigurationManager.AppSettings["Image.TempFolderName"]);
            string fileName = Path.GetFileNameWithoutExtension(data);
            string newName = fileName + new Random().Next().ToString() + ".jpg";

            try
            {
                FileHelper.SaveFile(croppedImage, Path.Combine(tempFolderName, newName));
                tempImages.Add(newName);
                return Json(newName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
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
            _engRepo.AddShareCount(PostId, UserId);
        }

        public void AddLikeCount(int PostId)
        {
            int UserId = _userRepo.GetUserByUsername(User.Identity.Name).UserId;
            _engRepo.AddLikeCount(PostId, UserId);
        }

        [HttpPost]
        public ActionResult GetWritePartial(string Type)
        {
            UserArticleVM model = new UserArticleVM();
            if (Type == "article")
            {
                return PartialView("_WriteArticlePartial", model);
            }
            else if (Type == "lists")
            {
                return PartialView("_WriteTopXPartial", model);
            }
            else if (Type == "opinion")
            {
                return PartialView("_WriteOpinionPartial", model);
            }
            else if (Type == "image")
            {
                return PartialView("_WriteImagePartial", model);
            }
            else if (Type == "video")
            {
                return PartialView("_WriteVideoPartial", model);
            }
            return null;
        }

        [HttpPost]
        public ActionResult GetTopXAddItemPartial(int Count)
        {
            int order = Count + 1;
            ViewBag.Order = order.ToString();
            return PartialView("_AddTopXItemPartial");
        }

        public ActionResult GetComments(int Index, int PostId)
        {
            List<Comment> comments = _articleRepo.GetComments(Index, PostId, null, null, null, null).ToList();
            UserArticleVM result = new UserArticleVM();
            result.Comments = comments;
            if (Request.IsAjaxRequest())
            {
                result.Comments.Reverse();
                return PartialView("_CommentPartial", result);
            }
            return View("ArticleDisplay", comments);
        }

        public string isPostLikedByUser(int PostId)
        {
            if (User.Identity.Name != null && User.Identity.Name != "")
            {
                int UserId = _userRepo.GetUserByUsername(User.Identity.Name).UserId;
                bool liked = _engRepo.isPostLikedByUser(PostId, UserId);
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
                _engRepo.AddViewsCount(id, User.Identity.Name);
            }
            // end of checking if the current user is the author of the post //

            UserArticleVM articleData = new UserArticleVM();
            articleData.newArticle = _articleRepo.GetPostById(id);
            articleData.userArticles = _articleRepo.GetAllUserPostsButOne(articleData.newArticle.CreatedBy, id, 5).ToList();
            articleData.newArticle.ViewsCount = _engRepo.GetEngCountByPostId(id, 3);
            articleData.newArticle.LikesCount = _engRepo.GetEngCountByPostId(id, 2);
            articleData.newArticle.SharesCount = _engRepo.GetEngCountByPostId(id, 1);
            ViewBag.userViewsCount = _engRepo.GetEngCountByUserId(articleData.newArticle.CreatedBy, 3);
            List<Post> simillarArticles = _articleRepo.GetPostsWithTagIDs(articleData.newArticle.Tags.Select(i => i.TagId).ToList(), 5).ToList();
            ViewBag.simillarArticles = simillarArticles;

            ViewBag.AllCommentsCount = _articleRepo.GetAllComments(id).Count();
            articleData.Comments = _articleRepo.GetComments(0, id, null, null, null, null).OrderBy(i => i.CommentCreationDate).ToList();

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

        [Authorize]
        public bool DeletePost(int PostId)
        {
            Post post = _articleRepo.GetPostById(PostId);
            try
            {
                _articleRepo.DeletePost(post);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
