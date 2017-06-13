using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageBuilderBusiness.Interfaces
{
    public interface iPackage
    {
        bool BuildPackage();

        string ErrorMessage { get; set; }

        string OutputPath { get; set; }

        List<string> Exceptions { get; set; }
    }
}
