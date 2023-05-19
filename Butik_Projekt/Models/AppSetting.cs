using System.Configuration;

namespace Butik_Projekt.Models
{
    internal class AppSetting
    {
        public static string Title { get; set; } = GetConfigValue("Title");
        public static int Height { get; set; } = int.Parse(GetConfigValue("Height"));
        public static int Width { get; set; } = int.Parse(GetConfigValue("Width"));
        public static string SavedBasketPath { get; set; } = GetConfigValue("SavedBasketPath");
        public static string BackgroundImagePath { get; set; } = GetConfigValue("BackgroundImagePath");
        public static string ProductsFilePath { get; set; } = GetConfigValue("ProductsFilePath");
        public static string DiscountFilePath { get; set; } = GetConfigValue("DiscountFilePath");

        private static string GetConfigValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}

