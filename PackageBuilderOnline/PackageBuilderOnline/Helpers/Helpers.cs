using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace PackageBuilderOnline.Helpers
{
    public static class AppHelpers
    {
        public static bool GetWebConfigValue(out string Value, string Key)
        {
            try
            {
                var appSetting = WebConfigurationManager.AppSettings[Key];

                Value = string.Empty;

                if (appSetting != null)
                {
                    Value = appSetting;
                    return true;
                }

                return false;
            }
            catch
            {
                Value = string.Empty;
                return false;
            }
        }
    }
}