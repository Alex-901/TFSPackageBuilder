using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PackageBuilderOnline.Models
{
    public class Results
    {
        public List<string> Exceptions { get; set; }
        public string OutputPath { get; set; }
        public string PageMessage { get; set; }
    }
}