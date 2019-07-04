using System;
using System.ComponentModel.DataAnnotations;

namespace ITDocument.Models
{
    public class DocumentFile
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }

        //public Guid DocumentId { get; set; }
        public virtual Document ApplicationDocumentViewModel { get; set; }

        public DocumentFile()
        {
            Id = Guid.NewGuid();
        }
    }
}