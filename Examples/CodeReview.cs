using Minimum.Connection;
using Minimum.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples
{
    class CodeReview
    {
        static void Main(string[] args)
        {
            // - This is a class that hold information on my connection to be used on the Repository
            SQLite sqliteConnection = new SQLite("App.data");

            // - A SCRUD class.
            Repository repository = new Repository(sqliteConnection);

            // - All functions from the repository use a [IMapper] class that if it isn't defined, the default [AutoMapper] is used.            
            IList<Reviewer> reviewers = repository.Select<Reviewer>();
        }
    }

    [Table("Reviewers")]
    public class Reviewer
    {
        [Identity, Column("ROWID")] public int ReviewerID { get; set; }
    }
}
