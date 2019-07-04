using System;

namespace ITDocument.Models
{
    public class BaseObject
    {
        public DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public BaseObject()
        {
        }

        public void New(string username)
        {
            CreateDate = DateTime.Now;
            CreateBy = username;
        }

        public void Update(string username)
        {
            UpdateDate = DateTime.Now;
            UpdatedBy = username;
        }
    }
}