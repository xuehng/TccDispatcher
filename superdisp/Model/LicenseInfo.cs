using System;
using System.Management;
using System.Reflection;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class LicenseInfo
    {
        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }   

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }   

        private static string GetProcessorId()
        {
            string sProcessorId = null;
            const string sQuery = "SELECT ProcessorId FROM Win32_Processor";

            ManagementObjectSearcher oManagementObjectSearcher = new ManagementObjectSearcher(sQuery);
            
            ManagementObjectCollection oCollection = oManagementObjectSearcher.Get();

            foreach (ManagementObject oManagementObject in oCollection)
            {
                sProcessorId = (string)oManagementObject["ProcessorId"];
            }

            return sProcessorId;
        }

        public static string GenerateSerial()
        {
            if (string.IsNullOrEmpty(AssemblyProduct))
                return null;

            string processId = GetProcessorId();
            if (string.IsNullOrEmpty(processId))
                return null;

            string deviceSerialNumber = GetDeviceSerialNumber();
            if (string.IsNullOrEmpty(deviceSerialNumber))
                return null;

            string serial = string.Format("{0}, {1}, {2}", AssemblyProduct, processId, deviceSerialNumber);
            string encoded = Encrypt.GetMD5Hash(serial);
            return encoded;
        }

        private static string GetDeviceSerialNumber()
        {
            HardDiskInfo hdd = AtapiDevice.GetHddInfo(0); // µÚÒ»¸öÓ²ÅÌ
            return hdd.SerialNumber;
        }

        public string LicenseCode { get; set; }
        private string Serial { get; set; }
        private string ProductName { get; set; }
        private string Version { get; set; }
        public int FeatureCode { get; private set; }
        public DateTime ExpireDate { get; private set; }

        public bool Initialize()
        {
            string license = RegistryInfo.GetLicense();
            if (string.IsNullOrEmpty(license))
                return false;

            if (!Parse(license))
                return false;

            return true;
        }

        public bool Parse(string license)
        {
            string plain = Encrypt.TripleDESDecrypt(license);

            string[] items = plain.Split(',');
            if (items.Length != 5)
                return false;

            Serial = items[0];
            ProductName = items[1];
            Version = items[2];
            FeatureCode = int.Parse(items[3]);

            DateTime expire = DateTime.Now;
            DateTime.TryParse(items[4], out expire);
            ExpireDate = expire;

            LicenseCode = license;
            return true;
        }

        public bool IsLicensed()
        {
            if (string.IsNullOrEmpty(LicenseCode))
                return false;

            if (!ValidateSerial())
                return false;

            if (ProductName != AssemblyProduct)
                return false;

            if (Version != AssemblyVersion)
                return false;

            if (ExpireDate.CompareTo(DateTime.Now) < 0)
                return false;

            return true;
        }

        private bool ValidateSerial()
        {
            if (string.IsNullOrEmpty(Serial))
                return false;

            string local = GenerateSerial();
            if (string.IsNullOrEmpty(local))
                return false;

            if (Serial != local)
                return false;

            return true;
        }

        public bool SaveLicense()
        {
            if (string.IsNullOrEmpty(LicenseCode))
                return false;

            if (!IsLicensed())
                return false;

            if (!RegistryInfo.SaveLicense(LicenseCode))
                return false;

            return true;
        }
    }
}
