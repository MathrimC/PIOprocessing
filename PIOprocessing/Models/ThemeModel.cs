using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OxyPlot;

namespace PIOprocessing.Models
{
    static class ThemeModel
    {
        public static Dictionary<string, string[]> Themes = new Dictionary<string, string[]>
        {
            {"Light", new string[] { "#FFFFFFFF", "#FF000000", "Segoe UI", "Normal" } },
            {"Dark", new string[] { "#FF3A393C", "#FFEBEBEB", "Segoe UI", "Normal" } },
            {"Ariana Grande", new string[] { "#FFF79CDF", "#FF8A0070", "Ink Free", "Bold" } },
            {"Matrix", new string[] { "#FF1A1A1A", "#FF39D03E", "LCD", "Bold" } }
        };
        public static string BackgroundColour { get; set; }
        public static string ForegroundColour { get; set; }
        public static string Font { get; set; }
        public static string FontWeight { get; set; }
        public static OxyColor OxyBackgroundColour { get; set; }
        public static OxyColor OxyForegroundColour { get; set; }
        public static void RefreshTheme()
        {
            BackgroundColour = Themes[Properties.Settings.Default.Theme][0];
            ForegroundColour = Themes[Properties.Settings.Default.Theme][1];
            Font = Themes[Properties.Settings.Default.Theme][2];
            FontWeight = Themes[Properties.Settings.Default.Theme][3];
            OxyBackgroundColour = hexToColor(BackgroundColour);
            OxyForegroundColour = hexToColor(ForegroundColour);
            Console.WriteLine($"a: {OxyForegroundColour.A.ToString()}, r: {OxyForegroundColour.R.ToString()}, g: {OxyForegroundColour.G.ToString()}, b: {OxyForegroundColour.B.ToString()}");
        }

        private static OxyColor hexToColor(string hexString)
        {
            //replace # occurences
            if (hexString.IndexOf('#') != -1)
                hexString = hexString.Replace("#", "");

            byte a, r, g, b = 0;

            a = byte.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            r = byte.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            g = byte.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            b = byte.Parse(hexString.Substring(6, 2), NumberStyles.AllowHexSpecifier);

            // Console.WriteLine($"a: {a.ToString()}, r: {r.ToString()}, g: {g.ToString()}, b: {b.ToString()}");
            // return oxyplot color
            return OxyColor.FromArgb(a, r, g, b);
        }
    }
}
