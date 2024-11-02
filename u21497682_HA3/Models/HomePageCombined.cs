using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u21497682_HA3.Models
{
    public class HomePageCombined
    {
        public IEnumerable<students> Students { get; set; }
        public IEnumerable<books> Books { get; set; }
    }
}