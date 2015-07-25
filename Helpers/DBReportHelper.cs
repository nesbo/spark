using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using OfficeOpenXml;
using System.Xml.Linq;
using frontoffice.Models;
using System.IO;

namespace frontoffice.Helpers
{
    // DBReportHelper class will parse XLS data to other data type objects
    // and perform operations with XLS data

    public static class DBReportHelper
    {
        // Store default DB Entry to database
        public static string SetDefaultDBEntry(string Report_id)
        {
            using (spark1Entities db = new spark1Entities())
            {
                string User_id = UsersHelper.GetMyUserId();

                // get user settings entry from UserSettings table in database
                IEnumerable<UserSettings> userSettings = db.UserSettings.Where(us => us.UserId == User_id);

                // if there is no entry for this user
                if (userSettings == null ||
                    userSettings.Count() <= 0)
                {
                    // add new user settings entry
                    db.UserSettings.Add(new UserSettings()
                    {
                        LastLogin = DateTime.Now,
                        UserId = User_id,
                        SelectedReport = Report_id
                    });
                }
                else
                {
                    // change value in SelectedReport field in the UserSettings table
                    userSettings.FirstOrDefault().SelectedReport = Report_id;
                }

                // save changes to the database
                try { db.SaveChanges(); }
                catch (Exception e) { return e.Message; }

                HttpContext.Current.Session["Data"] =
                    ParseDataWorkSheet(
                        GetUserDefaultDBEntry().EntryId);

                HttpContext.Current.Session["DataTitle"] =
                    DBReportHelper.GetUserDefaultDBEntry();

                return null; 
            }
        }
        
        // Gets default DB report file id from the UserSettings table in database
        public static DBEntry GetUserDefaultDBEntry()
        {
            string User_id = UsersHelper.GetMyUserId();
            using (spark1Entities db = new spark1Entities())
            {
                // we will enter default report id in this raturn variable
                string report_id = null;

                // check if there is a row entry in UserSettings table for the current user
                if (db.UserSettings.Where(us=>us.UserId == User_id).Count() == 1)
                {
                    // assign default report id from the table row
                    report_id = db
                        .UserSettings
                        .Where(us => us.UserId == User_id)
                        .FirstOrDefault()
                        .SelectedReport;
                }

                // if report_id variable still has NULL value (no row entry in UserSettings, no value in SelectedReport cell)
                // return latest uploaded report or NULL if there are no reports uploaded at all
                if (string.IsNullOrEmpty(report_id))
                {
                    IEnumerable<ReportEntries> allReports = db
                        .ReportEntries
                        .Include("AspNetUsers")
                        .Include("UserSettings");

                    if (allReports == null ||
                        allReports.Count() <= 0)
                        return null;

                    return allReports
                        .OrderByDescending(ar => ar.EntryDateTime)
                        .Select(ar => new DBEntry()
                        {
                            EntryDateTime = ar.EntryDateTime.ToString(),
                            EntryId = ar.EntryId,
                            UserName = ar.AspNetUsers.AgentFullName,
                            Title = ar.Title,
                            Description = ar.Description
                        })
                        .FirstOrDefault();
                }
                
                // this means we have report id and will return object with that id
                return db
                    .ReportEntries
                    .Include("AspNetUsers")
                    .Where(re => re.EntryId == report_id)
                    .Select(re => new DBEntry()
                    {
                        EntryDateTime = re.EntryDateTime.ToString(),
                        EntryId = re.EntryId,
                        UserName = re.AspNetUsers.AgentFullName,
                        Title = re.Title,
                        Description = re.Description
                    })
                    .FirstOrDefault();
            }
        }

        public static string UploadReport(HttpPostedFileBase File, string Title, string Description)
        {
            if (File == null ||
                File.ContentLength <= 0 ||
                string.IsNullOrEmpty(File.FileName))
                return "err_03";

            string UserId = UsersHelper.GetMyUserId();
            string fileExtension = Path.GetExtension(File.FileName).ToLower();

            // we accept only xlsx Excel files
            if (fileExtension != ".xlsx")
                return "no_xlsx";

            string dataDir = HttpContext.Current.Server.MapPath("~/xls_reports/"),
                userDir = Path.Combine(dataDir, UserId);
            
            if (!Directory.Exists(userDir))
                Directory.CreateDirectory(userDir);

            using (spark1Entities db = new spark1Entities())
            {
                // generating new report id
                bool exists = true;
                string report_id = "";
                while (exists)
                {
                    report_id = Guid.NewGuid().ToString();
                    exists = db.ReportEntries.Where(re => re.EntryId == report_id).Count() > 0;
                }
                
                // adding the report to ReportEntries database table
                db.ReportEntries.Add(new ReportEntries()
                {
                    EntryId = report_id,
                    Description = Description,
                    Title = Title,
                    EntryDateTime = DateTime.Now,
                    EntryUser = UserId
                });

                try
                {
                    // save changes to database
                    db.SaveChanges();
                }
                catch { return "err_01"; }

                try
                {
                    // save file to user directory
                    string fileName = Path.Combine(userDir, report_id + fileExtension);
                    File.SaveAs(fileName);
                }
                catch { return "err_02"; }

                // set newly uploaded file as users default
                SetDefaultDBEntry(report_id);
                return report_id;
            }
        }

        // This function will parse all data from a DB report worksheet to a DataTable object
        public static DataTable ParseDataWorkSheet(string Entry_id)
        {
            string fileName = null;
            using (spark1Entities db = new spark1Entities())
            {
                ReportEntries entry = db
                    .ReportEntries
                    .Where(re => re.EntryId == Entry_id)
                    .FirstOrDefault();
                
                fileName = Path.Combine(
                    HttpContext.Current.Server.MapPath("~/xls_reports"),
                    entry.EntryUser,
                    entry.EntryId + ".xlsx");
            }

            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets["Data"];
                DataTable tbl = new DataTable();
                bool hasHeader = true;
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                }
                var startRow = hasHeader ? 2 : 1;
                for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    var row = tbl.NewRow();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                    tbl.Rows.Add(row);
                }
                return tbl;
            }
        }

        public static List<DBEntry> GetAllReports()
        {
            using (spark1Entities db = new spark1Entities())
                return db
                    .ReportEntries
                    .Include("AspNetUsers")
                    .OrderByDescending(re => re.EntryDateTime)
                    .Select(re => new DBEntry()
                    {
                        EntryDateTime = re.EntryDateTime.ToString(),
                        EntryId = re.EntryId,
                        UserName = string.IsNullOrEmpty(re.AspNetUsers.AgentFullName) ?
                            re.AspNetUsers.UserName : 
                            re.AspNetUsers.AgentFullName,
                        Title = re.Title,
                        Description = re.Description
                    })
                    .ToList();
        }
    }
}