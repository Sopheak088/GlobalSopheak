using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ITDocument.Models
{
    public class Document : BaseObject
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Summary { get; set; }

        [AllowHtml]
        public string Content { get; set; }

        public string Tag { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public virtual ICollection<DocumentFile> DocumentFiles { get; set; }

        public Document()
        {
            Id = Guid.NewGuid();
            IsApproved = false;
        }

        public void Approve(string username)
        {
            IsApproved = true;
            ApprovedBy = username;
            ApprovedDate = DateTime.Now;
        }
    }
}