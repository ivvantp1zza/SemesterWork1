using SemWork1.Attributes.ORMAtttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Models
{
    internal class School
    {
        public int Id { get; set; }
        [NotIdentityValue("Name")]
        public string Name { get; set; }
        [NotIdentityValue("City")]
        public string City { get; set; }
        [NotIdentityValue("Rating")]
        public decimal Rating { get; set; }

        [DbCtor]
        public School(int id, string name, string city, decimal rating) : this(name, city, rating)
        {
            Id = id;
        }

        public School(string name, string city, decimal rating)
        {
            Name = name;
            City = city;
            Rating = rating;
        }
    }
}
