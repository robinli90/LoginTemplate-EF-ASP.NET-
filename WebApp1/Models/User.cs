using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using WebApp1.Data;

namespace WebApp1.Models
{
    [Table("loginUsers")]
    public class User
    {
        public int InternalId { get; set; }

        [Key]
        [Required]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }

        public AccountType AccountType { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password, ErrorMessage = "Password invalid")]
        public string Password { get; set; }

        public string Viewable { get; set; }

        public DateTime LastLoginTime { get; set; }

        public int LoginCount { get; set; }
        
        public byte[] Image { get; set; }

        public User()
        {
        }

        /// <summary>
        /// Set User Credentials
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="accountType"></param>
        public static void CreateNewUser(User user)
        {
            //Encrypt password here


            using (var db = new AccountContext())
            {
                // Cast to lower for comparison
                user.Email = user.Email.ToLower();

                // Set internal id if not set already
                if (user.InternalId < 5000)
                    user.InternalId = GetInternalId();

                // If new user
                if (!db.Users.Any(x => x.Email == user.Email))
                {
                    if (user.LoginCount <= 1) user.LoginCount = 1;
                    if (user.LastLoginTime.Year < DateTime.Now.Year) user.LastLoginTime = DateTime.Now;
                    db.Users.Add(user);
                }
                else
                {
                    // Remove old entry
                    db.Users.Remove(db.Users.First(x => x.Email == user.Email));

                    // Append new entry
                    db.Users.Add(user);
                }
                db.SaveChanges();
            }
        }

        public static AccountType GetAccountType(string email)
        {
            using (var db = new AccountContext())
            {
                return db.Users.FirstOrDefault(x => x.Email == email).AccountType;
            }
        }

        /// <summary>
        /// Get User List (specific users is not null, focus only on specifics)
        /// Providing an email returns only users in viewable
        /// </summary>
        /// <param name="specificUsers"></param>
        /// <returns></returns>
        public static List<User> GetUsers(string email="")
        {
            List<User> userList = new List<User>();

            using (var db = new AccountContext())
            {
                // Display all users from the database 
                var query = from b in db.Users
                    select b;

                foreach (var item in query)
                {
                    userList.Add(item);
                }
            }

            if (email.Length > 0 && GetAccountType(email) != AccountType.Administrator)
            {
                User refUser = userList.First(x => x.Email == email);

                // If no viewable, return empty list
                if (refUser.Viewable == "") return new List<User>();

                // Get only specific users from viewable user list
                userList = userList.Where(x => refUser.Viewable.Contains(x.InternalId.ToString())).ToList();
            }

            return userList;
        }

        /// <summary>
        /// Delete user based on email
        /// </summary>
        /// <param name="email"></param>
        public static void DeleteUser(string email)
        {
            using (var db = new AccountContext())
            {
                if (db.Users.Any(x => x.Email == email))
                {
                    db.Users.Remove(db.Users.First(x => x.Email == email));
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Returns whether or not user can login based on credentials; if so, set login parameters
        /// </summary>
        /// <param name="loginEmail"></param>
        /// <param name="loginPassword"></param>
        /// <returns></returns>
        public static bool Login(string loginEmail, string loginPassword)
        {
            loginEmail = loginEmail.ToLower();

            // Validate login
            if (ValidatePassword(loginEmail, loginPassword))
            {

                using (var db = new AccountContext())
                {
                    db.Users.First(x => x.Email == loginEmail).LastLoginTime = DateTime.Now;
                    db.Users.First(x => x.Email == loginEmail).LoginCount++;
                    db.SaveChanges();
                }
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Debug tool for displaying users
        /// </summary>
        public static void DisplayUsers()
        {
            using (var db = new AccountContext())
            {
                // Display all users from the database 
                var query = from b in db.Users
                    select b;
                
                foreach (var item in query)
                {
                    // Demo purposed accounts (force accountType)
                    if (item.Email == "lirobin9@gmail.com")
                    {
                        item.AccountType = AccountType.Administrator;
                    }
                    if (item.Email == "jsmith@gmail.com")
                    {
                        item.AccountType = AccountType.Manager;
                    }

                    Debug.WriteLine(item.Email + ", " + item.Password + ", views: " + (item.Viewable != null ? String.Join("|", item.Viewable) : ""));
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Save image into database
        /// </summary>
        /// <param name="imageIn"></param>
        public static void SaveImage(User user, MemoryStream ms)
        {

            using (var db = new AccountContext())
            {
                // Display all users from the database 
                var query = from b in db.Users
                            where b.Email.ToLower() == user.Email.ToLower()
                    select b;

                foreach (var item in query)
                {
                    item.Image = ms.ToArray();
                }
                
                db.SaveChanges();
                
            }
        }

        /// <summary>
        /// Save image into database
        /// </summary>
        /// <param name="imageIn"></param>
        public static void SaveViewables(User user, string viewableListById)
        {

            using (var db = new AccountContext())
            {

                db.Users.First(x => x.InternalId == user.InternalId).Viewable = viewableListById;
                
                db.SaveChanges();
                
            }
        }

        public string GetImageString()
        {
            if (Image == null) return "";

            return string.Format("data:image/png;base64,{0}", Convert.ToBase64String(Image));
        }

        /// <summary>
        /// Validate password by comparing email and password with EF DB
        /// </summary>
        /// <param name="comparisonEmail"></param>
        /// <param name="comparisonPassword"></param>
        /// <returns></returns>
        private static bool ValidatePassword(string comparisonEmail, string comparisonPassword)
        {
            //Decrypt comparisonPassword here

            using (var db = new AccountContext())
            {
                // Display all users from the database 
                var query = from b in db.Users
                    where b.Email.Equals(comparisonEmail) && 
                    b.Password.Equals(comparisonPassword)
                    select b;

                // If query returns anything, matching password
                return query.Any();
            }
        }

        /// <summary>
        /// Return bool for whether or not email exists
        /// </summary>
        /// <param name="comparisonEmail"></param>
        /// <param name="comparisonPassword"></param>
        /// <returns></returns>
        public static bool DoesEmailExist(string comparisonEmail)
        {
            using (var db = new AccountContext())
            {
                // Display all users from the database 
                var query = from b in db.Users
                    where b.Email.ToLower() == comparisonEmail.ToLower()
                    select b;

                // If query returns anything, matching password
                return query.Any();
            }
        }

        /// <summary>
        /// Return user object (assumes email exists already)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static User GetUserByEmail(string email)
        {
            using (var db = new AccountContext())
            {
                return db.Users.FirstOrDefault(x => x.Email == email);
            }
        }
        /// <summary>
        /// Return user object (assumes id exists already)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static User GetUserById(string id)
        {
            using (var db = new AccountContext())
            {
                return db.Users.FirstOrDefault(x => x.InternalId.ToString() == id);
            }
        }

        /// <summary>
        /// Generate unique random Id
        /// </summary>
        /// <returns></returns>
        public static int GetInternalId()
        {
            using (var db = new AccountContext())
            {
                Random r = new Random();
                int randomNum = r.Next(100000000, 999999999);
                while (db.Users.Any(x => x.InternalId == randomNum))
                    randomNum = r.Next(100000000, 999999999);

                return randomNum;
            }
        }
    }
}