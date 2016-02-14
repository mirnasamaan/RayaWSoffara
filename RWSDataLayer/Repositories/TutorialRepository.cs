using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Text;

namespace RWSDataLayer.Repositories
{
    public class TutorialRepository : BaseRepository<Tutorial>
    {
        #region Tutorials
        public Tutorial GetTutorialById(int TutorialId)
        {
            return Context.Tutorials.FirstOrDefault(i => i.TutId == TutorialId);
        }

        public UserTutorial GetUserTutorialByTutAndUserId(int TutorialId, int UserId)
        {
            return Context.UserTutorials.FirstOrDefault(i => i.TutId == TutorialId && i.UserId == UserId);
        }

        public bool UserTutorialExist(int TutorialId, int UserId)
        {
            UserTutorial userTut = Context.UserTutorials.FirstOrDefault(i => i.TutId == TutorialId && i.UserId == UserId);
            if (userTut != null)
            {
                return true;
            }
            return false;
        }

        public bool ViewTutorial(int TutorialId, int UserId) 
        {
           try
           {
               if (UserTutorialExist(TutorialId, UserId) == false)
               {
                   UserTutorial userTut = new UserTutorial();
                   userTut.TutId = TutorialId;
                   userTut.UserId = UserId;
                   userTut.isViewed = true;
                   Context.UserTutorials.Add(userTut);
               }
               else
               {
                   UserTutorial userTut = Context.UserTutorials.FirstOrDefault(i => i.TutId == TutorialId && i.UserId == UserId);
                   userTut.isViewed = true;
               }
               Context.SaveChanges();
               return true;
           }
           catch (Exception ex)
           {
               return false;
           }
        }
        #endregion

        #region Advertisement
        public bool AddAdvertisement(Advertisement Adv)
        {
            try
            {
                Context.Advertisements.Add(Adv);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IQueryable<Advertisement> GetAdvertisements(int? page, DateTime? from, DateTime? to, string search, int? count = 10)
        {
            IQueryable<Advertisement> advs;
            if (from != null && to != null)
            {
                DateTime fromDate = from.Value.Date;
                DateTime toDate = to.Value.Date;
                advs = Context.Advertisements.Where(i => EntityFunctions.TruncateTime(i.AdvertisementTimestamp.Value) >= fromDate && EntityFunctions.TruncateTime(i.AdvertisementTimestamp.Value) <= toDate);

            }
            else if (from != null && to == null)
            {
                DateTime fromDate = from.Value.Date;
                advs = Context.Advertisements.Where(i => EntityFunctions.TruncateTime(i.AdvertisementTimestamp.Value) >= fromDate);
            }
            else if (from == null && to != null)
            {
                DateTime toDate = to.Value.Date;
                advs = Context.Advertisements.Where(i => EntityFunctions.TruncateTime(i.AdvertisementTimestamp.Value) <= toDate);
            }
            else
            {
                advs = Context.Advertisements;
            }
            if (search != null)
            {
                advs = advs.Where(i => i.AdvertisementUserEmail.Contains(search) || i.AdvertisementText.Contains(search));
            }
            if (page != null && count != null)
            {
                int startIndex = page.Value * count.Value;
                advs = advs.OrderByDescending(i => i.AdvertisementTimestamp).Skip(startIndex).Take(count.Value);
            }
            
            return advs;
        }
        #endregion

        #region Suggestion
        public bool AddSuggestion(Suggestion suggestion)
        {
            try
            {
                Context.Suggestions.Add(suggestion);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IQueryable<Suggestion> GetSuggestions(int? page, DateTime? from, DateTime? to, string search, int? count = 10)
        {
            IQueryable<Suggestion> suggestions;
            if (from != null && to != null)
            {
                from = from.Value.Date;
                to = to.Value.Date;
                suggestions = Context.Suggestions.Where(i => EntityFunctions.TruncateTime(i.SuggestionTimestamp.Value) >= from && EntityFunctions.TruncateTime(i.SuggestionTimestamp.Value) <= to);

            }
            else if (from != null && to == null)
            {
                from = from.Value.Date;
                suggestions = Context.Suggestions.Where(i => EntityFunctions.TruncateTime(i.SuggestionTimestamp.Value) >= from);
            }
            else if (from == null && to != null)
            {
                to = to.Value.Date;
                suggestions = Context.Suggestions.Where(i => EntityFunctions.TruncateTime(i.SuggestionTimestamp.Value) <= to);
            }
            else
            {
                suggestions = Context.Suggestions;
            }
            if (search != null)
            {
                suggestions = suggestions.Where(i => i.SuggestionUserEmail.Contains(search) || i.SuggestionText.Contains(search));
            }
            if (page != null && count != null)
            {
                int startIndex = page.Value * count.Value;
                suggestions = suggestions.OrderByDescending(i => i.SuggestionTimestamp).Skip(startIndex).Take(count.Value);
            }

            return suggestions;
        }
        #endregion
    }
}
