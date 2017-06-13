using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using PackageBuilderOnline.Helpers;

namespace PackageBuilderOnline.Models
{
    public class GenerationOptions
    {
        [DisplayName("TFS Release ID")]
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0")]
        [Required]
        public int ReleaseId { get; set; }

        [DisplayName("Files Types")]
        [Required(ErrorMessage = "Select a file type")]
        public List<string> SelectedFilesTypes { get; set; }

        public string PageErrorMessage { get; set; }

        public MultiSelectList GetFileTypes()
        {
            var configFileTypes = new List<string>();
            string value = null;

            if (AppHelpers.GetWebConfigValue(out value, "FileTypes"))
            {
                configFileTypes.AddRange(value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x));

                if (configFileTypes.Any())
                {
                    return new MultiSelectList(configFileTypes, SelectedFilesTypes);
                }
            }

            return new MultiSelectList(new string[] { ".SQL", ".PRC", ".RPT", ".VIW", ".CS", ".VB", ".CSHTML", ".VBHTML", ".ASPX", ".ASPX.CS", ".ASPX.VB" }, SelectedFilesTypes);
        }
        
    }
}