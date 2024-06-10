// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using Object = UnityEngine.Object;

namespace ConfigurationManager.Utilities
{
    public static class Utils
    {
        // Additionally search for BaseUnityPlugin to find dynamically loaded plugins
        public static BaseUnityPlugin[] FindPlugins()
        {
            return Chainloader.Plugins
                .Concat(Object.FindObjectsOfType(typeof(BaseUnityPlugin)).Cast<BaseUnityPlugin>()).Distinct().ToArray();
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}