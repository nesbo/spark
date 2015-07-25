using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using frontoffice.Helpers;
using frontoffice.Models;

namespace frontoffice.Helpers
{
    // static class UsersHelper does some work with user accounts
    public static class UsersHelper
    {
        // GetAllUsers() will return the list of all users in database.
        // We put the user data in UserHelper class defined in Models/AccountModels file
        public static List<UserHelper> GetAllUsers()
        {
            // Call the EntityFramework to retreive data from AspNetUsers database table.
            // EntityFramework class spark1Entities inherits IDisposable interface and should be disposed
            // after usage. It is done by using statement.
            using (spark1Entities db = new spark1Entities())
            {
                // This is basic LINQ to Entities example

                return db
                    // First call db.AspNetUsers table,
                    .AspNetUsers
                    // then include AspNetRoles table in the search query (similar to inner join in SQL),
                    .Include("AspNetRoles")
                    // then for every return row we create one UserHelper object with data from that row,
                    .Select(u => new UserHelper()
                    {
                        UserName = string.IsNullOrEmpty(u.AgentFullName) ?
                            u.UserName :
                            u.AgentFullName,
                        UserRole = u.AspNetRoles.FirstOrDefault().Name,

                    })
                    // and finally convert the search result to List<UserHelper>
                    .ToList();
            }
        }
        
        public static string GetMyUserName()
        {
            string result = "",
                username = HttpContext.Current.User.Identity.Name;
            using (spark1Entities db = new spark1Entities())
                result = db
                    .AspNetUsers
                    .Where(u => u.UserName == username)
                    .Count() > 0 ?
                        db
                        .AspNetUsers
                        .Where(u => u.UserName == username)
                        .FirstOrDefault()
                        .AgentFullName : 
                        username;
            return result;
        }
        public static string GetMyUserId()
        {
            string result = "",
                username = HttpContext.Current.User.Identity.Name;

            using (spark1Entities db = new spark1Entities())
                result = db
                    .AspNetUsers
                    .Where(u => u.UserName == username)
                    .Count() > 0 ?
                        db
                        .AspNetUsers
                        .Where(u => u.UserName == username)
                        .FirstOrDefault()
                        .Id :
                        null;
            return result;
        }

        public static string UpdateLoginStats(string Username)
        {
            using (spark1Entities db = new spark1Entities())
            {
                string reportId = null;
                IEnumerable<UserSettings> settings = db
                    .UserSettings
                    .Include("AspNetUsers")
                    .Where(us => us.AspNetUsers.UserName == Username);

                if (settings != null &&
                    settings.Count() == 1)
                {
                    settings.FirstOrDefault().LastLogin = DateTime.Now;
                    reportId = settings.FirstOrDefault().SelectedReport;
                }
                else
                {
                    string userId = db
                        .AspNetUsers
                        .Where(us => us.UserName == Username)
                        .FirstOrDefault()
                        .Id;
                    reportId = db
                        .ReportEntries
                        .OrderByDescending(r => r.EntryDateTime)
                        .FirstOrDefault()
                        .EntryId;
                    db
                        .UserSettings
                        .Add(new UserSettings()
                        {
                            UserId = userId,
                            LastLogin = DateTime.Now,
                            SelectedReport = reportId
                        });
                }

                try { db.SaveChanges(); }
                catch (Exception e) { return e.Message; }

                HttpContext.Current.Session["Data"] =
                    DBReportHelper.ParseDataWorkSheet(
                        DBReportHelper.GetUserDefaultDBEntry().EntryId);

                HttpContext.Current.Session["DataTitle"] =
                    DBReportHelper.GetUserDefaultDBEntry();

                return null;
            }
        }
    }
}