using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ITDocument.Models;
using ITDocument.ViewModels;

namespace ITDocument.Managers
{
    public static class DocumentManager
    {
        /// <summary>
        /// List all record of document
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private static ApplicationDbContext dbContext = new ApplicationDbContext();

        public static List<Document> ListAllDocuments(ApplicationDbContext db)
        {
            var list = db.Documents.ToList();
            return list;
        }

        /// <summary>
        /// Get one record of document by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Document GetDocumentById(Guid? id, ApplicationDbContext db)
        {
            Document document = db.Documents.Include(s => s.DocumentFiles).SingleOrDefault(x => x.Id == id);
            return document;
        }

        /// <summary>
        /// Search by Filter Date and Keyword
        /// </summary>
        /// <param name="documentIndexViewModel"></param>
        /// <param name="db"></param>
        /// <returns>DocumentList</returns>
        public static List<Document> SearchDocument(DocumentIndexViewModel documentIndexViewModel, ApplicationDbContext db)
        {
            var documents = from s in db.Documents select s;

            if (!documentIndexViewModel.FromDate.Equals(null) && !documentIndexViewModel.ToDate.Equals(null) && String.IsNullOrEmpty(documentIndexViewModel.Search))
            {
                documents = db.Documents.Where(x => x.CreateDate >= documentIndexViewModel.FromDate
                                && x.CreateDate <= documentIndexViewModel.ToDate);
            }
            else if (!documentIndexViewModel.FromDate.Equals(null) && !documentIndexViewModel.ToDate.Equals(null) && !String.IsNullOrEmpty(documentIndexViewModel.Search))
            {
                documents = db.Documents.Where(x => x.CreateDate >= documentIndexViewModel.FromDate
                                && x.CreateDate <= documentIndexViewModel.ToDate && x.Tag.Contains(documentIndexViewModel.Search));
            }
            else if (!string.IsNullOrEmpty(documentIndexViewModel.Search))
            {
                documents = documents.Where(s => s.Tag.Contains(documentIndexViewModel.Search));
            }
            return documents.ToList();
        }

        /// <summary>
        /// Get one record of document by ID to view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static DocumentViewModel ViewModelById(Guid? id, ApplicationDbContext db)
        {
            var document = GetDocumentById(id, db);
            DocumentViewModel documentViewModel = new DocumentViewModel
            {
                Id = document.Id,
                Name = document.Name,
                Summary = document.Summary,
                Tag = document.Tag,
                Content = document.Content,
                DocumentFiles = document.DocumentFiles,
                IsApproved = document.IsApproved,
                CreateBy = document.CreateBy,
                CreateDate = document.CreateDate,
                UpdateBy = document.UpdatedBy,
                UpdateDate = document.UpdateDate,
                ApprovedBy = document.ApprovedBy,
                ApprovedDate = document.ApprovedDate
            };
            return documentViewModel;
        }
    }
}