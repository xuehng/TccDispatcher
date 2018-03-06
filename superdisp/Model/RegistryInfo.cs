using System.Diagnostics;
using System.Security.Permissions;
using Microsoft.Win32;

namespace renstech.NET.SupernovaDispatcher.Model
{
    class RegistryInfo
    {
        private const string CompanyKey = "renstech";
        private const string ProductKey = "superdisp";
        private const string LoginKey = "last_login";
        private const string LicenseKey = "license";
        private const string AdminPasswd = "admin_password";

        private static RegistryKey GetProductDir()
        {
            try
            {
                RegistryKey hkml = Registry.LocalMachine;
                RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
                if (software != null)
                {
                    RegistryKey companydir = software.CreateSubKey(CompanyKey);
                    if (companydir != null)
                    {
                        RegistryKey productdir = companydir.CreateSubKey(ProductKey);
                        return productdir;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            return null;
        }

        public static string GetAdminUserPassword()
        {
            RegistryKey productdir = GetProductDir();
            if (productdir == null)
                return null;

            var value = productdir.GetValue(AdminPasswd);
            if (value != null)
            {
                return value.ToString();
            }
            return null;
        }

        public static bool SetAdminUserPassword(string password)
        {
            RegistryKey productdir = GetProductDir();
            if (productdir == null)
                return false;

            try
            {
                productdir.SetValue(AdminPasswd, password);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public static bool SaveLastLoginUser(string user)
        {
            try
            {
                RegistryKey productdir = GetProductDir();
                productdir.SetValue(LoginKey, user);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public static string GetLastLoginUser()
        {
            try
            {
                RegistryKey productdir = GetProductDir();
                return productdir.GetValue(LoginKey).ToString();

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;            	
            }
        }

        public static string GetLicense()
        {
            RegistryKey productdir = GetProductDir();
            if (productdir == null)
                return null;

            var license = productdir.GetValue(LicenseKey);
            if ( license != null)
                return license.ToString();

            return null;
        }

        public static bool SaveLicense(string code)
        {
            RegistryKey productdir = GetProductDir();
            if (productdir == null)
                return false;

            try
            {
                productdir.SetValue(LicenseKey, code);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        [RegistryPermission(SecurityAction.LinkDemand, Write = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run")]
        public static bool SetAutoRun(bool enable)
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (rkApp == null)
                return false;

            if (enable)
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                rkApp.SetValue(LicenseInfo.AssemblyProduct, "\"" + path + "\"");
            }
            else
            {
                rkApp.DeleteValue(LicenseInfo.AssemblyProduct, false);
            }

            return true;
        }

        public static bool IsAutoRunEnabled()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (rkApp == null)
                return false;

            if (rkApp.GetValue(LicenseInfo.AssemblyProduct) == null)
                return false;
            return true;
        }
    }
}
