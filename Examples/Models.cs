using Minimum.DataAccess;
using System.Collections.Generic;

namespace Examples
{
    [Table("Users")]
    public class User
    {
        [Identity, Column("ROWID")] public int UserID { get; set; }
        public string Name { get; set; }

        public int CompanyID { get; set; }
        [On("CompanyID", "ROWID")] public Company Company { get; set; }

        [On("ROWID", "UserID"), Cascade] public virtual IList<Comment> Comments { get; set; }
    }

    [Table("Companies")]
    public class Company
    {
        [Identity, Column("ROWID")] public int CompanyID { get; set; }
        public string Name { get; set; }
    }


    [Table("Comments")]
    public class Comment
    {
        [Identity, Column("ROWID")] public int CommentID { get; set; }
        public string Text { get; set; }
        public int UserID { get; set; }
    }
}