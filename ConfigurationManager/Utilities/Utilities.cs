// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Sentry;
using static ConfigurationManager.ConfigurationManager;
using Object = UnityEngine.Object;

namespace ConfigurationManager.Utilities
{
    public static class Utils
    {
        internal static void SentryInvoke(Action a) => SentryInvoke(a, _sentryOptions, _logger);

        /// <summary>
        /// Invoke code with this as a sentry wrapper. Should make a local lambda like this: 
        /// <example>
        /// <code>internal static void SentryInvoke(Action a) => SentryInvoke(a, _sentryOptions, _logger);</code>
        /// </example>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="sentryOptions"></param>
        /// <param name="logger"></param>
        public static void SentryInvoke(Action a, SentryOptions sentryOptions, ManualLogSource logger)
        {
            if (useSentry > logToSentry.Disabled)
                using (SentrySdk.Init(sentryOptions))
                {
                    try
                    {
                        a.Invoke();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e);
                        // throw e;
                    }
                }
            else
                a.Invoke();
        }

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