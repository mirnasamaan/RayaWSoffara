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
        public IQueryable<Image> GetAllImages(int page, List<int> TagIds = null, int count = 20)
        {
            if (TagIds != null && TagIds.Count() != 0)
            {
                return Context.Images.OrderByDescending(i => i.ImageId).Where(i => TagIds.Intersect(i.Tags.Select(j => j.TagId)).Count() > 0).Skip(page * count).Take(count);
            }
            else
            {
                return Context.Images.OrderByDescending(i => i.ImageId).Skip(page * count).Take(count);
            }
        }
        #endregion
    }
}