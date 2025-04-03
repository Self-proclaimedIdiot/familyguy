using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTree
{
    public partial class Person
    {
        public int Id { get; set; } 
        public bool IsMale { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string? Patronim {  get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly? DeathDate { get; set; }
        public Person? Father { get; set; }
        public int? FatherId { get; set; }
        public Person? Mother { get; set; }
        public int? MotherId { get; set; }
        public List<Person> GetAncestors(int generation)
        {
            List<Person> ancestors = new List<Person>();
            if (generation > 1)
            {
                if(Father != null)
                ancestors.AddRange(Father.GetAncestors(generation - 1));
                if(Mother != null)
                ancestors.AddRange(Mother.GetAncestors(generation - 1));
            }
            else
            {
                if(Father != null)
                ancestors.Add(Father);
                if(Mother != null)
                ancestors.Add(Mother);
            }
            return ancestors;
        }
    }
}
