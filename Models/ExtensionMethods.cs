using System;

namespace DXLinkFormatter { 
    public static class ExtensionMethods {
        public static string[] Lines(this string str) {
            return str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
