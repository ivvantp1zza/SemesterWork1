using SemWork1.Attributes.ORMAtttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Models
{
    internal class SchoolEvent
    {
        public int Id { get; set; }
        [NotIdentityValue("Name")]
        public string Name { get; set; }
        [NotIdentityValue("SchoolId")]
        public int SchoolId { get; set; }
        [NotIdentityValue("Date")]
        public DateTime Date { get; set; }
        [NotIdentityValue("Description")]
        public string Description { get; set; }

        [DbCtor]
        public SchoolEvent(int id, int schoolId, string name, string description, DateTime date) : this(name, schoolId, date, description)
        {
            Id = id;
        }

        public SchoolEvent(string name, int schoolId, DateTime date, string description)
        {
            Name = name;
            SchoolId = schoolId;
            Date = date;
            Description = description;
        }
    }
}
