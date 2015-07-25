using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace frontoffice.Models
{
    public class UserHelper
    {
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public DateTime LastLogin { get; set; }

        public UserHelper()
        {

        }
    }

    public class DBEntry
    {
        public string EntryId { get; set; }
        public string UserName { get; set; }
        public string EntryDateTime { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class AddDBEntryViewModel
    {
        [Required]
        [Display(Name="Report title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Upload XLSX file")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File { get; set; }
    }

    public class SetDefaultDBReportHelper
    {
        public string Report_id { get; set; }
        public bool RedirectToDashboard { get; set; }
    }
}