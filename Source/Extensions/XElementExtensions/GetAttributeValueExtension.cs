using Geranium.Reflection;
using Godot;
using System;
using System.Xml.Linq;

namespace ioi
{
    public static class GetAttributeValueExtension
    {
        /// <summary>
        /// Integer attribute value
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="attrName"></param>
        /// <returns></returns>
        public static int GetTagAttrInteger(this XElement xElement, string attrName)
        {
            var attr = xElement.Attribute(attrName);
            if (attr == null)
                return default;

            int.TryParse(attr.Value, out int value);
            return value;
        }
        
        public static int GetTagAttrFloatRound(this XElement xElement, string attr)
        {
            double.TryParse(xElement.Attribute(attr).Value.Replace(".",","), out var value);

            return (int)Math.Round(value);
        }

        public static string GetTagAttrString(this XElement xElement, string attr)
        {
            return xElement.Attribute(attr).Value;
        }
        
        public static Color GetTagAttrColor(this XElement xElement, string attr)
        {
            var @string = xElement.Attribute(attr).Value;

            var conv = new System.Drawing.ColorConverter();
            var sysDrawColor = conv.ConvertFromString(@string).As<System.Drawing.Color>();

            return new Color(sysDrawColor.R, sysDrawColor.G, sysDrawColor.B, sysDrawColor.A);
        }
    }
}
