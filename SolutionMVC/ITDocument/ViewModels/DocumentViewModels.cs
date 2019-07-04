using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ITDocument.Models;

namespace ITDocument.ViewModels
{
    public class DocumentIndexViewModel
    {
        public string Search { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FromDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ToDate { get; set; }

        public IEnumerable<DocumentViewModel> DocumentViewModels { get; set; }

        public DocumentIndexViewModel()
        {
            DocumentViewModels = new List<DocumentViewModel>();
            //FromDate = DateTime.Now;
            //ToDate= DateTime.Now;
        }
    }

    public class DocumentViewModel
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
            public string Summary { get; set; }

            [AllowHtml] public string Content { get; set; }

            public string Tag { get; set; }
            public DateTime? CreateDate { get; set; }
            public string CreateBy { get; set; }
            public DateTime? UpdateDate { get; set; }
            public string UpdateBy { get; set; }
            public bool IsApproved { get; set; }
            public string ApprovedBy { get; set; }
            public DateTime? ApprovedDate { get; set; }
            public virtual ICollection<DocumentFile> DocumentFiles { get; set; }

            public DocumentViewModel()
            {
                Id = new Document().Id;
            }
        }

        public class DocumentCreateViewModel
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
            public string Summary { get; set; }

            [AllowHtml] public string Content { get; set; }

            public string Tag { get; set; }

            public virtual ICollection<DocumentFile> DocumentFiles { get; set; }

            public DocumentCreateViewModel()
            {
                Id = Guid.NewGuid();
            }
        }

        public class DocumentUpdateViewModel
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
            public string Summary { get; set; }

            [AllowHtml] public string Content { get; set; }

            public string Tag { get; set; }
            public virtual ICollection<DocumentFile> DocumentFiles { get; set; }
        }
    }
