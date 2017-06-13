using PackageBuilderOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using PackageBuilderBusiness;
using PackageBuilderBusiness.Interfaces;
using System.Diagnostics;

namespace PackageBuilderOnline.Controllers
{
    public class PackageController : Controller
    {
        public ActionResult Index()
        {
            return View("Create", new GenerationOptions());
        }

        public ActionResult Create(GenerationOptions model)
        {
            var retval = View("Create", model);

            if (!ModelState.IsValid)
            {
                return retval;
            }

            iPackage package = new Package(model.ReleaseId, model.SelectedFilesTypes);

            if (package.BuildPackage())
            {
                var results = new Results() { OutputPath = package.OutputPath, Exceptions = package.Exceptions, PageMessage = $"Package creation complete - { package.OutputPath }" };
                retval = View("Results", results);
            }
            else
            {
                model.PageErrorMessage = package.ErrorMessage;
            }

            return retval;
        }

        public ActionResult GetFilePath(string fileName)
        {
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowPackageFolder(string Path)
        {
            Process.Start(Path);
            return View("Create", new GenerationOptions());
        }
    }
}