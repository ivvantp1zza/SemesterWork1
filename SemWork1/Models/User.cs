using SemWork1.Attributes.ORMAtttributes;
using SemWork1.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Models
{
    internal class User
    {
        public int Id { get; set; }
        [NotIdentityValue("Login")]
        public string Login { get; set; }
        [NotIdentityValue("Email")]
        public string Email { get; set; }
        [NotIdentityValue("Password")]
        public string Password { get; set; }
        [NotIdentityValue("City")]
        public string City { get; set; }
        [NotIdentityValue("Sex")]
        public string Sex { get; set; }
        public int PostsAmount { get 
            {
                var dao = new DAO("SemDB1");
                var posts = dao.Select<GeneralPost>().Where(p => p.AuthorId == Id);
                return posts.Count();
            } }

        public User(string login, string email, string password, string city, string sex)
        {
            Login = login;
            Email = email;
            Password = password;
            City = city;
            Sex = sex;
        }

        [DbCtor]
        public User(int id, string login, string email, string password, string city, string sex) : this(login, email, password, city, sex)
        {
            Id = id;
        }
    }
}
