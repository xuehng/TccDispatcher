using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Themes;
using Xceed.Wpf.Themes.Glass;
using Xceed.Wpf.Themes.LiveExplorer;
using Xceed.Wpf.Themes.Media;
using Xceed.Wpf.Themes.Office2007;
using Xceed.Wpf.Themes.Windows7;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class ThemeInfo
    {
        public static readonly string Officesilver = "Office 2007 Silver";
        public static readonly string Officeblue = "Office 2007 Blue";
        public static readonly string Officeblack = "Office 2007 Black";
        public static readonly string Media = "Media";
        public static readonly string Glass = "Glass";
        public static readonly string Liveexplorer = "Live Explorer";
        public static readonly string Window7 = "Window 7";
        public static readonly string Key = "XPT20-8XYTA-5DXEN-BU3A";

        static public List<string> ThemeList
        {
            get
            {
                List<string> list = new List<string>
                                        {Officesilver, Officeblue, Officeblack, Media, Glass, Liveexplorer, Window7};
                return list;
            }
        }

        static public ResourceDictionary GetResourceDictionary(string themename, ref Brush appBackgroudBrush)
        {
            ThemeResourceDictionary resource = null;
            if (themename == Officesilver)
            {
                resource = new Office2007SilverResourceDictionary();
                appBackgroudBrush = Office2007SilverResources.ApplicationBackgroundBrush;
            }
            else if (themename == Officeblack)
            {
                resource = new Office2007BlackResourceDictionary();
                appBackgroudBrush = Office2007BlackResources.ApplicationBackgroundBrush;
            }
            else if (themename == Officeblue)
            {
                resource = new Office2007BlueResourceDictionary();
                appBackgroudBrush = Office2007BlueResources.ApplicationBackgroundBrush;
            }
            else if (themename == Media)
            {
                resource = new MediaResourceDictionary();
                appBackgroudBrush = MediaResources.ApplicationBackgroundBrush;
            }
            else if (themename == Glass)
            {
                resource = new GlassResourceDictionary();
                appBackgroudBrush = GlassResources.ApplicationBackgroundBrush;
            }
            else if (themename == Liveexplorer)
            {
                resource = new LiveExplorerResourceDictionary();
                appBackgroudBrush = LiveExplorerResources.ApplicationBackgroundBrush;
            }
            else if (themename == Window7)
            {
                resource = new Windows7ResourceDictionary();
                appBackgroudBrush = Windows7Resources.ApplicationBackgroundBrush;
            }
            
            if ( resource != null)
                resource.LicenseKey = Key;

            return resource;
        }
    }
}
