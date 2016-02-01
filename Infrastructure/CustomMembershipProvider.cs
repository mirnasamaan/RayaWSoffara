using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Linq;
using RWSDataLayer.Context;
using RWSDataLayer.Repositories;
using System.Collections.Generic;

namespace RWSInfrastructure
{
    public class CustomMembershipProvider : MembershipProvider
    {
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var md5Hash = GetMd5Hash(oldPassword);
            var _userRepo = new UserRepository();

            var requiredUser = _userRepo.GetUserByUsernameAndPassword(username, md5Hash);
            if (requiredUser == null)
                return false;
            else
            {
                _userRepo.ChangeUserPassword(username, md5Hash);
            }
            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != string.Empty)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            var user = GetUser(username, true);

            if (user == null)
            {
                var userObj = new RWSUser { UserName = username, Password = GetMd5Hash(password), Email = email, CreationDate = DateTime.Now , ConfirmationToken = new Guid().ToString(), IsConfirmed = false, IsBanned = false, BanDate = null, LastPasswordFailureDate = null, ConfirmationDate = null, PasswordChangedDate = DateTime.Now, PasswordVerificationToken = new Guid().ToString(), ProfileImagePath = null, PasswordVerificationTokenExpirationDate= null, FirstName = null, LastName = null, Country = null, PasswordSalt = null, PasswordFaliuresSinceLastSuccess = null };

                var _userRepo = new UserRepository();

                _userRepo.AddUser(userObj);

                status = MembershipCreateStatus.Success;

                return GetUser(username, true);
            }

            status = MembershipCreateStatus.DuplicateUserName;
            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var _userRepo = new UserRepository();
            if (_userRepo.CheckIfUsernameExist(username))
            {
                RWSUser user = _userRepo.GetUserByUsername(username);
                _userRepo.DeleteUser(user);
            }
            return false;
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            UserRepository _repo = new UserRepository();
            List<RWSUser> usersList = _repo.GetAllUsers(pageIndex, pageSize, "", null, null, out totalRecords).ToList();
            foreach (var item in usersList)
            {
                users.Add(UserToMembershipUser(item));
            }
            return users;
        }

        private MembershipUser UserToMembershipUser(RWSUser user)
        {
            try
            {
                MembershipUser returnedUser = new MembershipUser("", user.UserName, null, user.Email, "", "", (bool)user.IsConfirmed, (bool)user.IsBanned, (DateTime)user.CreationDate, DateTime.Now, DateTime.Now, (DateTime)user.ConfirmationDate, DateTime.Now);
                returnedUser.Comment = user.ConfirmationToken;
                return returnedUser;
            }
            catch (Exception ex) {
                return null;
            }
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            UserRepository _repo = new UserRepository();
            RWSUser user = _repo.GetUserByUsername(username);
            if (user != null)
            {
                var memUser = UserToMembershipUser(user);
                return memUser;
            }
            return null;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 6; }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            var md5Hash = GetMd5Hash(password);
            UserRepository _repo = new UserRepository();
            var requiredUser = _repo.IsValid(username, md5Hash);
            return requiredUser;
        }

        public static string GetMd5Hash(string value)
        {
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}