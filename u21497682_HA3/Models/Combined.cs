using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u21497682_HA3.Models
{
    public class Combined
    {
        public IEnumerable<students> Students { get; set; }
        public IEnumerable<books> Books { get; set; }
        public IEnumerable<authors> Authors { get; set; }
        public IEnumerable<types> Types { get; set; }
        public IEnumerable<borrows> Borrows { get; set; }
    }
}