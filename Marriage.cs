using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTree
{
    internal class Marriage
    {
        public Person? Husband { get; set; }
        public int HusbandId { get; set; }
        public Person? Wife { get; set; }
        public int WifeId { get; set;}
    }
}
