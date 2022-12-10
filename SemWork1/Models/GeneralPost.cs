using SemWork1.Attributes.ORMAtttributes;
using SemWork1.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Models
{
    internal class GeneralPost
    {
        public int Id { get; set; }
        [NotIdentityValue("AuthorId")]
        public int AuthorId { get; set; }
        public string Author
        {
            get
            {
                var dao = new DAO("SemDB1");
                return dao.SelectById<User>(AuthorId).Login;
            }
        }
        [NotIdentityValue("Topic")]
        public string Topic { get; set; }
        [NotIdentityValue("Content")]
        public string Content { get; set; }
        [NotIdentityValue("Created")]
        public DateTime Created { get; set; }

        [DbCtor]
        public GeneralPost(int id, int authorId, string topic, string content, DateTime created) : this(authorId, topic, content, created)
        {
            Id = id;
        }

        
        public GeneralPost(int authorId, string topic, string content, DateTime created)
        {
            AuthorId = authorId;
            Topic = topic;
            Content = content;
            Created = created;
        }
    }
}
