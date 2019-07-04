using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using AutoMapper;
using ITDocument.Managers;
using ITDocument.Mappers;
using ITDocument.Models;
using ITDocument.ViewModels;
using EntityState = System.Data.Entity.EntityState;

namespace ITDocument.Controllers
{
    public class DocumentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string _uploadPath = ConfigurationManager.AppSettings["UploadFolder"];

        #region Index

        //View
        [Authorize(Roles = "Admin, DocumentView")]
        public ActionResult Index()
        {
            ViewBag.Title = "Home";
            ViewBag.Home = "class = active";

            DocumentIndexViewModel documentIndexViewModel = new DocumentIndexViewModel();
            documentIndexViewModel.DocumentViewModels = Mapper.Map<IEnumerable<Document>, IEnumerable<DocumentViewModel>>(DocumentManager.ListAllDocuments(db));

            var getDoc = DocumentManager.ListAllDocuments(db);
            SelectList list = new SelectList(getDoc, "Tag", "Tag");
            ViewBag.docListName = list;
            return View(documentIndexViewModel);
        }

        [HttpPost]
        //Search
        [Authorize(Roles = "Admin, DocumentView")]
        public ActionResult Index(DocumentIndexViewModel documentIndexViewModel)
        {
            ViewBag.Title = "Home";

            
            var getDoc = DocumentManager.ListAllDocuments(db);
            SelectList list = new SelectList(getDoc, "Tag", "Tag");
            ViewBag.docListName = list;

            ViewBag.Document = DocumentManager.ListAllDocuments(db);

            documentIndexViewModel.DocumentViewModels = Mapper.Map<IEnumerable<Document>, IEnumerable<DocumentViewModel>>(DocumentManager.SearchDocument(documentIndexViewModel, db).ToList());
            return View(documentIndexViewModel);
        }

        #endregion Index

        #region Details

        [Authorize(Roles = "Admin, DocumentDetail")]
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(DocumentManager.ViewModelById(id, db));
        }

        #endregion Details

        //? Create
        [Authorize(Roles = "Admin, DocumentCreate")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, DocumentCreate")]
        public ActionResult Create(DocumentCreateViewModel documentCreateViewModel)
        {
            Document document = new Document();
            document.UpdateDocument(documentCreateViewModel);
            document.New(User.Identity.Name);

            if (ModelState.IsValid)
            {
                List<DocumentFile> fileDetails = new List<DocumentFile>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        DocumentFile fileDetail = new DocumentFile
                        {
                            Id = Guid.NewGuid(),
                            Name = fileName,
                            Type = file.ContentType
                            //DocumentId = _document.Id
                        };
                        long? size = file.ContentLength;
                        fileDetail.Size = size.ToString();
                        fileDetail.Path = Path.GetExtension(fileName);

                        fileDetails.Add(fileDetail);

                        var path = Path.Combine(Server.MapPath(_uploadPath), fileDetail.Id + fileDetail.Path);
                        file.SaveAs(path);
                    }
                }
                document.DocumentFiles = fileDetails;

                db.Documents.Add(document);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(document);
        }

        //[HttpPost]
        [Authorize(Roles = "Admin, DocumentEdit")]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var document = DocumentManager.GetDocumentById(id, db);
            DocumentUpdateViewModel documentUpdateViewModel = new DocumentUpdateViewModel
            {
                Id = document.Id,
                Name = document.Name,
                Summary = document.Summary,
                Tag = document.Tag,
                Content = document.Content,
                DocumentFiles = document.DocumentFiles
            };
            return View(documentUpdateViewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [Authorize(Roles = "Admin, DocumentEdit")]
        public ActionResult Edit(DocumentUpdateViewModel documentUpdateViewModel)
        {
            Document document = db.Documents.Find(documentUpdateViewModel.Id);
            document.UpdateDocument(documentUpdateViewModel);
            document.Update(User.Identity.Name);
            if (ModelState.IsValid)
            {
                List<DocumentFile> fileDetails = new List<DocumentFile>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        DocumentFile fileDetail = new DocumentFile()
                        {
                            Name = fileName,
                            Type = Path.GetExtension(fileName),
                            Id = Guid.NewGuid()
                        };
                        fileDetails.Add(fileDetail);
                        long? size = file.ContentLength;
                        fileDetail.Size = size.ToString();
                        fileDetail.Path = Path.GetExtension(fileName);
                        var path = Path.Combine(Server.MapPath(_uploadPath), fileDetail.Id + fileDetail.Path);

                        document.DocumentFiles = fileDetails;
                        file.SaveAs(path);
                        db.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                db.Entry(document).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(document);
        }

        [HttpPost]
        public JsonResult DeleteFile(Guid? id)
        {
            if (String.IsNullOrEmpty(id.ToString()))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                Guid guid = new Guid(id.ToString());
                DocumentFile fileDetail = db.DocumentFiles.Find(id);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                db.DocumentFiles.Remove(fileDetail);
                db.SaveChanges();

                //Delete file from the file system
                var path = Path.Combine(Server.MapPath(_uploadPath), fileDetail.Id + fileDetail.Path);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin, DocumentDelete")]
        public ActionResult Delete(Guid id)
        {
            try
            {
                Document document = db.Documents.Find(id);
                if (document == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //delete files from the file system

                if(document.DocumentFiles != null)
                {
                    foreach (var item in document.DocumentFiles)
                    {
                        String path = Path.Combine(Server.MapPath(_uploadPath), item.Id + item.Type);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                }

                db.Documents.Remove(document);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        // GET: ExportData
        public void ExportToExcel()
        {
            // Step 1 - get the data from database
            var data = db.Documents.ToList().Select(p => new
            {
                p.Name,
                p.Summary,
                p.Tag
            }
            );

            // instantiate the GridView control from System.Web.UI.WebControls namespace
            // set the data source
            GridView gridview = new GridView();
            gridview.DataSource = data;
            gridview.DataBind();

            // Clear all the content from the current response
            Response.ClearContent();
            Response.Buffer = true;
            // set the header
            Response.AddHeader("content-disposition", "attachment; filename = Document.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            // create HtmlTextWriter object with StringWriter
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    // render the GridView to the HtmlTextWriter
                    gridview.RenderControl(htw);
                    // Output the GridView content saved into StringWriter
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }
        }

        //Install Package From NuGet
        //Microsoft.ReportingServices.ReportViewerControl.WebForms
        //public void PrintList()
        //{
        //    ReportViewer reportViewer = new ReportViewer();
        //    reportViewer.ProcessingMode = ProcessingMode.Local;
        //    reportViewer.SizeToReportContent = true;
        //    reportViewer.Width = Unit.Percentage(900);
        //    reportViewer.Height = Unit.Percentage(900);
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}