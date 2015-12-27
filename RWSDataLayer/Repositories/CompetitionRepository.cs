using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RWSDataLayer.Repositories
{
    public class CompetitionRepository : BaseRepository<Competition>
    {
        #region Regions
        /// <summary>
        /// Adds a new region
        /// </summary>
        /// <param name="region">Region object</param>
        public Region AddRegion(Region region)
        {
            Context.Regions.Add(region);
            Context.SaveChanges();
            return region;
        }

        /// <summary>
        /// Gets All Regions
        /// </summary>
        /// <returns></returns>
        public IQueryable<Region> GetAllRegions()
        {
            return Context.Regions.OrderByDescending(i => i.RegionName);
        }

        /// <summary>
        /// Gets All Regions
        /// </summary>
        /// <param name="pageNo">Current page starting from 0</param>
        /// <param name="count">No of items per page</param>
        /// <returns></returns>
        public IQueryable<Region> GetAllRegions(int startIndex, int count)
        {
            if (startIndex > Context.Regions.Count())
                return null;
            else
                return Context.Regions.OrderByDescending(i => i.RegionName).Skip(startIndex).Take(count);
        }

        /// <summary>
        /// Gets a region by ID
        /// </summary>
        /// <param name="id">Region Id</param>
        /// <returns></returns>
        public Region GetRegionById(int id)
        {
            if (Context.Regions.Any(i => i.RegionId == id))
                return Context.Regions.FirstOrDefault(i => i.RegionId == id);
            else
                return null;
        }

        /// <summary>
        /// Delete a region
        /// </summary>
        /// <param name="region">Region object</param>
        public void DeleteRegion(Region region)
        {
            Context.Regions.Remove(region);
            Context.SaveChanges();
        }

        /// <summary>
        /// Update a region
        /// </summary>
        /// <param name="region">Region object</param>
        public void UpdateRegion(Region region)
        {
            Context.Entry(region).State = EntityState.Modified;
            Context.SaveChanges();
        }

        public bool DeactivateRegion(int RegionId)
        {
            try
            {
                Region region = Context.Regions.Where(i => i.RegionId == RegionId).FirstOrDefault();
                region.IsActive = false;
                Context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool ActivateRegion(int RegionId)
        {
            try
            {
                Region region = Context.Regions.Where(i => i.RegionId == RegionId).FirstOrDefault();
                region.IsActive = true;
                Context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

        #region Competition
        /// <summary>
        /// Add new competition
        /// </summary>
        /// <param name="competition">Competition object</param>
        public Competition AddCompetition(Competition competition)
        {
            Context.Competitions.Add(competition);
            Context.SaveChanges();
            return competition;
        }

        /// <summary>
        /// Adds a competition to a region
        /// </summary>
        /// <param name="competition">Competition object</param>
        /// <param name="regionId">Region id</param>
        public void AddCompetitionToRegion(Competition competition, int regionId)
        {
            competition.RegionId = regionId;
            Context.Entry(competition).State = EntityState.Added;
            Context.SaveChanges();
        }

        public IQueryable<Competition> GetCompetitionsForTeam(int teamId)
        {
            return Context.Competitions.Where(i => i.Teams.Any(j => j.TeamId == teamId));
        }

        /// <summary>
        /// Gets all competitions within a region
        /// </summary>
        /// <param name="regionid">Region id</param>
        /// <returns></returns>
        public IQueryable<Competition> GetCompetitionsForRegion(int regionid)
        {
            return Context.Competitions.Where(i => i.RegionId == regionid);
        }

        public Competition GetCompetitionById(int compId)
        {
            return Context.Competitions.FirstOrDefault(i => i.CompetitionId == compId);
        }

        public void DeactivateCompetition(int compId)
        {
            Competition comp = GetCompetetionById(compId);
            comp.isActive = false;
            Context.SaveChanges();
        }

        public void ActivateCompetition(int compId)
        {
            Competition comp = GetCompetetionById(compId);
            comp.isActive = true;
            Context.SaveChanges();
        }

        /// <summary>
        /// Gets all competitions within a region
        /// </summary>
        /// <param name="regionid">Region id</param>
        /// <param name="pageNo">Page no starting from 0</param>
        /// <param name="count">No of items per page</param>
        /// <returns></returns>
        public IQueryable<Competition> GetCompetitionsForRegion(int regionid, int pageNo, int count)
        {
            int noOfItems = pageNo * count;
            if (noOfItems > Context.Competitions.Where(i => i.RegionId == regionid).Count())
                return null;
            else
                return Context.Competitions.Where(i => i.RegionId == regionid).OrderByDescending(i => i.CompetitionName).Skip(noOfItems).Take(count);
        }

        /// <summary>
        /// Get all competitions
        /// </summary>
        /// <returns></returns>
        public IQueryable<Competition> GetAllCompetetions()
        {
            return Context.Competitions;
        }

        /// <summary>
        /// Get all competitions
        /// </summary>
        /// <param name="pageNo">Page no starting from 0</param>
        /// <param name="count">No of items per page</param>
        /// <returns></returns>
        public IQueryable<Competition> GetAllCompetetions(int startIndex, int count)
        {
            if (startIndex > Context.Competitions.Count())
                return null;
            else
                return Context.Competitions.OrderByDescending(i => i.CompetitionName).Skip(startIndex).Take(count);
        }

        /// <summary>
        /// Get a competition by id
        /// </summary>
        /// <param name="id">Competition Id</param>
        /// <returns></returns>
        public Competition GetCompetetionById(int id)
        {
            return Context.Competitions.FirstOrDefault(i => i.CompetitionId == id);
        }

        /// <summary>
        /// Delete a competition
        /// </summary>
        /// <param name="c">Competition object</param>
        public void DeleteCompetition(Competition c)
        {
            Context.Competitions.Remove(c);
            Context.SaveChanges();
        }

        /// <summary>
        /// Update a competition
        /// </summary>
        /// <param name="c">Competition object</param>
        public void UpdateCompetition(Competition c)
        {
            Context.Entry(c).State = EntityState.Modified;
            Context.SaveChanges();
        }

        public void DeleteAllTeamsForCompetition(int CompId)
        {
            Context.Competitions.FirstOrDefault(i => i.CompetitionId == CompId).Teams.Clear();
            Context.SaveChanges();
        }

        #endregion

        #region Team
        /// <summary>
        /// Add new team
        /// </summary>
        /// <param name="team">Team object</param>
        /// <param name="competitionIds">List of competitions id assigned to this team</param>
        public Team AddTeam(Team team, List<int> competitionIds)
        {
            Context.Teams.Add(team);
            Context.SaveChanges();
            AddTeamToCompetitions(team, competitionIds);
            return team;
        }

        /// <summary>
        /// Adds a team to a competition
        /// </summary>
        /// <param name="team">Team object</param>
        /// <param name="competitionIds">List of competitions id assigned to this team</param>
        private void AddTeamToCompetitions(Team team, List<int> competitionIds)
        {
            if (competitionIds != null)
            {
                team.Competitions = new List<Competition>();
                foreach (var item in competitionIds)
                {
                    var comp = Context.Competitions.FirstOrDefault(i => i.CompetitionId == item);
                    team.Competitions.Add(comp);
                }
                Context.SaveChanges();
            }
        }

        /// <summary>
        /// Gets all teams in a competition
        /// </summary>
        /// <param name="competitionId">Competition Id</param>
        /// <returns></returns>
        public IQueryable<Team> GetTeamsForCompetition(int competitionId)
        {
            return Context.Teams.Where(i => i.Competitions.Any(j => j.CompetitionId == competitionId));
        }

        public IQueryable<Team> GetTeamsForPlayer(int PlayerId)
        {
            return Context.Teams.Where(i => i.Players.Any(j => j.PlayerId == PlayerId));
        }

        /// <summary>
        /// Gets all teams in a competition
        /// </summary>
        /// <param name="competitionId">Competition Id</param>
        /// <param name="pageNo">Page no starting from 0</param>
        /// <param name="count">No of items per page</param>
        /// <returns></returns>
        public IQueryable<Team> GetTeamsForCompetition(int competitionId, int pageNo, int count)
        {
            int noOfItems = pageNo * count;
            if (noOfItems > Context.Teams.Where(i => i.Competitions.Any(j => j.CompetitionId == competitionId)).Count())
                return null;
            else
                return Context.Teams.Where(i => i.Competitions.Any(j => j.CompetitionId == competitionId)).OrderByDescending(i => i.TeamName).Skip(noOfItems).Take(count);
        }

        /// <summary>
        /// Get all teams
        /// </summary>
        /// <returns></returns>
        public IQueryable<Team> GetAllTeams()
        {
            return Context.Teams;
        }

        /// <summary>
        /// Get all Teams
        /// </summary>
        /// <param name="pageNo">Page no starting from 0</param>
        /// <param name="count">No of items per page</param>
        /// <returns></returns>
        public IQueryable<Team> GetAllTeams(int startIndex, int count)
        {
            if (startIndex > Context.Teams.Count())
                return null;
            else
                return Context.Teams.OrderByDescending(i => i.TeamName).Skip(startIndex).Take(count);
        }

        /// <summary>
        /// Get a team by id
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <returns></returns>
        public Team GetTeamById(int id)
        {
            return Context.Teams.FirstOrDefault(i => i.TeamId == id);
        }

        public void DeleteAllCompetitionsForTeam(int TeamId)
        {
            Context.Teams.FirstOrDefault(i => i.TeamId == TeamId).Competitions.Clear();
            Context.SaveChanges();
        }

        public void DeleteAllPlayersForTeam(int TeamId)
        {
            Context.Teams.FirstOrDefault(i => i.TeamId == TeamId).Players.Clear();
            Context.SaveChanges();
        }

        /// <summary>
        /// Delete a team
        /// </summary>
        /// <param name="c">Team object</param>
        public void DeleteTeam(Team c)
        {
            Context.Teams.Remove(c);
            Context.SaveChanges();
        }

        public void ActivateTeam(int TeamId)
        {
            Team team = Context.Teams.FirstOrDefault(i => i.TeamId == TeamId);
            team.isActive = true;
            Context.SaveChanges();
        }

        public void DeactivateTeam(int TeamId)
        {
            Team team = Context.Teams.FirstOrDefault(i => i.TeamId == TeamId);
            team.isActive = false;
            Context.SaveChanges();
        }

        /// <summary>
        /// Update a Team
        /// </summary>
        /// <param name="c">Team object</param>
        public void UpdateTeam(Team c, List<int> competitionIds)
        {
            Context.Entry(c).State = EntityState.Modified;
            Context.SaveChanges();
            AddTeamToCompetitions(c, competitionIds);
        }

        public void AddTeamPlayers(Team team, List<int> playerIds)
        {
            if (playerIds != null)
            {
                team.Players = new List<Player>();
                foreach (var item in playerIds)
                {
                    Player player = GetPlayerById(item);
                    team.Players.Add(player);
                }
                Context.SaveChanges();
            }
        }
        #endregion

        #region Player
        /// <summary>
        /// Add a new player
        /// </summary>
        /// <param name="player">Player object</param>
        /// <param name="teamIds">List of teams that player playes for</param>
        public Player AddPlayer(Player player, List<int> teamIds)
        {
            Context.Players.Add(player);
            Context.SaveChanges();
            AddPlayerToTeams(player, teamIds);
            return player;
        }

        private void AddPlayerToTeams(Player player, List<int> teamIds)
        {
            if (teamIds != null)
            {
                player.Teams = new List<Team>();
                foreach (var item in teamIds)
                {
                    var team = Context.Teams.FirstOrDefault(i => i.TeamId == item);
                    player.Teams.Add(team);
                }
                Context.SaveChanges();
            }
        }

        /// <summary>
        /// Get player by id
        /// </summary>
        /// <returns></returns>
        public Player GetPlayerById(int id)
        {
            return Context.Players.FirstOrDefault(i => i.PlayerId == id);
        }

        public IQueryable<Player> GetPlayersForTeam(int teamId)
        {
            return Context.Players.Where(i => i.Teams.Any(j => j.TeamId == teamId));
        }

        /// <summary>
        /// Get all players
        /// </summary>
        /// <returns></returns>
        public IQueryable<Player> GetAllPlayers()
        {
            return Context.Players;
        }

        /// <summary>
        /// Get all Players
        /// </summary>
        /// <param name="pageNo">Page no starting from 0</param>
        /// <param name="count">No of items per page</param>
        /// <returns></returns>
        public IQueryable<Player> GetAllPlayers(int startIndex, int count)
        {
            if (startIndex > Context.Players.Count())
                return null;
            else
                return Context.Players.OrderByDescending(i => i.PlayerName).Skip(startIndex).Take(count);
        }

        public void DeleteAllTeamsForPlayer(int PlayerId)
        {
            Context.Players.FirstOrDefault(i => i.PlayerId == PlayerId).Teams.Clear();
            Context.SaveChanges();
        }

        /// <summary>
        /// Delete a Player
        /// </summary>
        /// <param name="c">Player object</param>
        public void DeletePlayer(Player c)
        {
            Context.Players.Remove(c);
            Context.SaveChanges();
        }

        public void DeactivatePlayer(int PlayerId)
        {
            Player player = Context.Players.FirstOrDefault(i => i.PlayerId == PlayerId);
            player.isActive = false;
            Context.SaveChanges();
        }

        public void ActivatePlayer(int PlayerId)
        {
            Player player = Context.Players.FirstOrDefault(i => i.PlayerId == PlayerId);
            player.isActive = true;
            Context.SaveChanges();
        }

        /// <summary>
        /// Update a Player
        /// </summary>
        /// <param name="c">Player object</param>
        public void UpdatePlayer(Player c, List<int> teamIds)
        {
            Context.Entry(c).State = EntityState.Modified;
            Context.SaveChanges();
            AddPlayerToTeams(c, teamIds);
        }
        #endregion

        #region Images
        public IQueryable<Image> GetAllImages()
        {
            return Context.Images;
        }

        public Image GetImageById(int ImageId)
        {
            return Context.Images.FirstOrDefault(i => i.ImageId == ImageId);
        }

        public IQueryable<Image> GetImages(int page, int count = 10)
        {
            int skipCount = (page - 1) * 10;
            IQueryable<Image> images = Context.Images.OrderByDescending(i => i.ImageId).Skip(skipCount).Take(count);
            return images;
        }

        public bool DeleteImage(Image image)
        {
            try
            {
                Context.Images.Remove(image);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Image AddImage(Image image)
        {
            Context.Images.Add(image);
            Context.SaveChanges();
            return image;
        }

        public Image UpdateImage(Image image, List<int> tagIds)
        {
            Context.SaveChanges();
            AddTagToImage(image, tagIds);
            return image;
        }

        public IQueryable<Tag> GetTagsForImage(int imageId)
        {
            return Context.Tags.Where(i => i.Images.Any(j => j.ImageId == imageId));
        }

        public void DeleteAllTagsForImage(int ImageId)
        {
            Context.Images.FirstOrDefault(i => i.ImageId == ImageId).Tags.Clear();
            Context.SaveChanges();
        }

        public void AddTagToImage(Image image, List<int> tagIds)
        {
            if (tagIds != null)
            {
                image.Tags = new List<Tag>();
                foreach (var item in tagIds)
                {
                    var tag = Context.Tags.FirstOrDefault(i => i.TagId == item);
                    image.Tags.Add(tag);
                }
                Context.SaveChanges();
            }
        }
        #endregion

        #region Engagements
        /// <summary>
        /// Gets all engagement types with engagement weight more than 0
        /// </summary>
        /// <returns></returns>
        public IQueryable<EngagementType> GetAllEngagementTypesWithWeight()
        {
            return Context.EngagementTypes.Where(i => i.EngWeight > 0);
        }
        #endregion
    }
}
