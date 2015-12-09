using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Linq;
using RWSDataLayer.Repositories;
using System.Globalization;

namespace RWSInfrastructure
{
    public class CustomRoleProvider : RoleProvider
    {
        public override bool IsUserInRole(string username, string roleName)
        {
            UserRepository _repo = new UserRepository();
            var user = _repo.GetUserByUsername(username);
            return _repo.IsUserInRole(user, roleName);
        }

        public override string[] GetRolesForUser(string username)
        {
            UserRepository _repo = new UserRepository();
            var roles = _repo.GetRolesForUser(username);
            if (roles != null)
            {
                var mroles = roles.ToList();
                return mroles.ToArray();
            }
            
            return null;
        }

        public override string[] GetAllRoles()
        {
            UserRepository _repo = new UserRepository();
            var roles = _repo.GetAllRoles();
            if (roles != null)
                return roles.ToArray();
            return null;
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            UserRepository _repo = new UserRepository();
            _repo.AddUserToRoles(usernames, roleNames);
        }

        public override void CreateRole(string roleName)
        {
            UserRepository _repo = new UserRepository();
            _repo.AddRole(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            UserRepository _repo = new UserRepository();
            _repo.DeleteRole(roleName);
            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            UserRepository _repo = new UserRepository();
            return _repo.RoleExists(roleName);
        }

        public override string ApplicationName { get; set; }
    }
}