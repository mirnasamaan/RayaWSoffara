using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using RWSDataLayer.Context;

namespace RWSDataLayer.Repositories
{
    public class ImageRepository : BaseRepository<Image>
    {
        #region Image
        /// <summary>
        /// Gets all images
        /// </summary>
        /// <returns></returns>
        public IQueryable<Image> GetAllImages(int page, int count = 20)
        {
            return Context.Images.OrderByDescending(i => i.ImageId).Skip(page*count).Take(count);
        }
        #endregion
    }
}