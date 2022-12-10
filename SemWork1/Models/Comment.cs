using SemWork1.Attributes.ORMAtttributes;
using SemWork1.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Models
{
    internal class Comment
    {
        public int Id { get; set; }
        [NotIdentityValue("PostId")]
        public int PostId { get; set; }
        [NotIdentityValue("AuthorId")]
        public int AuthorId { get; set; }
        public string AuthorName
        {
            get
            {
                var dao = new DAO("SemDB1");
                return dao.SelectById<User>(AuthorId).Login;
            }
        }

        [NotIdentityValue("Content")]
        public string Content { get; set; }
        [NotIdentityValue("Created")]
        public DateTime Created { get; set; }
    }
}
