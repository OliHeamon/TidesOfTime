﻿// Credit to Scalie for ThemeHandler - https://github.com/ScalarVector1/DragonLens/blob/407a54e45d7a4828f660b46988feaf86092249b3/Core/Systems/ThemeSystem/ThemeHandler.cs

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using TidesOfTime.Common.UI.Themes.Providers;
using TidesOfTime.Common.UI.Themes.Futuristic;

namespace TidesOfTime.Common.UI.Themes
{
    public class ThemeSystem : ModSystem
    {
        public readonly static Dictionary<string, ThemeBoxProvider> allBoxProviders = new();
        public readonly static Dictionary<string, ThemeIconProvider> allIconProviders = new();

        private readonly static Dictionary<Type, ThemeBoxProvider> allBoxProvidersByType = new();
        private readonly static Dictionary<Type, ThemeIconProvider> allIconProvidersByType = new();

        public static ThemeBoxProvider currentBoxProvider;
        public static ThemeColorProvider currentColorProvider = new();
        public static ThemeIconProvider currentIconProvider;

        /// <summary>
        /// The color that buttons should be drawn in.
        /// </summary>
        public static Color ButtonColor => currentColorProvider.buttonColor;

        /// <summary>
        /// The color that background boxes should be drawn in.
        /// </summary>
        public static Color BackgroundColor => currentColorProvider.backgroundColor;

        /// <summary>
        /// Sets the current box provider based on a string key. The key should be the name of the ThemeBoxProvider's type.
        /// </summary>
        /// <param name="key">The type name of the ThemeBoxProvider to set</param>
        private static void SetBoxProvider(string key)
        {
            currentBoxProvider = allBoxProviders[key];
        }

        /// <summary>
        /// Sets the current box provider based on a type.
        /// </summary>
        /// <typeparam name="T">The type of the box provider to set</typeparam>
        public static void SetBoxProvider<T>() where T : ThemeBoxProvider
        {
            currentBoxProvider = allBoxProvidersByType[typeof(T)];
        }

        /// <summary>
        /// Sets the current box provider to a given box provider instance.
        /// </summary>
        /// <param name="provider">The provider to use</param>
        public static void SetBoxProvider(ThemeBoxProvider provider)
        {
            currentBoxProvider = provider;
        }

        /// <summary>
        /// Sets the current icon provider based on a string key. The key should be the name of the ThemeIconProvider's type.
        /// </summary>
        /// <param name="key">The type name of the ThemeIconProvider to set</param>
        private static void SetIconProvider(string key)
        {
            currentIconProvider = allIconProviders[key];
        }

        /// <summary>
        /// Sets the current icon provider based on a type.
        /// </summary>
        /// <typeparam name="T">The type of the icon provider to set</typeparam>
        public static void SetIconProvider<T>() where T : ThemeIconProvider
        {
            currentIconProvider = allIconProvidersByType[typeof(T)];
        }

        /// <summary>
        /// Sets the current icon provider to a given icon provider instance.
        /// </summary>
        /// <param name="provider">the provider to use</param>
        public static void SetIconProvider(ThemeIconProvider provider)
        {
            currentIconProvider = provider;
        }

        public static ThemeBoxProvider GetBoxProvider<T>() where T : ThemeBoxProvider
        {
            return allBoxProvidersByType[typeof(T)];
        }

        public static ThemeIconProvider GetIconProvider<T>() where T : ThemeIconProvider
        {
            return allIconProvidersByType[typeof(T)];
        }

        public override void Load()
        {
            foreach (Type t in GetType().Assembly.GetTypes())
            {
                if (!t.IsAbstract && t.IsSubclassOf(typeof(ThemeBoxProvider)))
                {
                    allBoxProviders.Add(t.FullName, (ThemeBoxProvider)Activator.CreateInstance(t));
                    allBoxProvidersByType.Add(t, (ThemeBoxProvider)Activator.CreateInstance(t));
                }

                if (!t.IsAbstract && t.IsSubclassOf(typeof(ThemeIconProvider)))
                {
                    allIconProviders.Add(t.FullName, (ThemeIconProvider)Activator.CreateInstance(t));
                    allIconProvidersByType.Add(t, (ThemeIconProvider)Activator.CreateInstance(t));
                }
            }
        }

        /// <summary>
        /// Shortcut to get the current icon provider's icon for a given key.
        /// </summary>
        /// <param name="key">The key of the icon to get</param>
        /// <returns>a Texture2D for the icon</returns>
        public static Texture2D GetIcon(string key)
        {
            return currentIconProvider.GetIcon(key);
        }

        public static void SaveData(TagCompound tag)
        {
            var themeTag = new TagCompound
            {
                ["BoxTheme"] = currentBoxProvider.GetType().FullName,
                ["IconTheme"] = currentIconProvider.GetType().FullName,

                ["backColor"] = currentColorProvider.backgroundColor,
                ["buttonColor"] = currentColorProvider.buttonColor
            };

            tag["Theme"] = themeTag;
        }

        public static void LoadData(TagCompound tag)
        {
            if (tag.TryGet("Theme", out TagCompound themeTag))
            {
                SetBoxProvider(themeTag.GetString("BoxTheme"));
                SetIconProvider(themeTag.GetString("IconTheme"));

                currentColorProvider.backgroundColor = themeTag.Get<Color>("backColor");
                currentColorProvider.buttonColor = themeTag.Get<Color>("buttonColor");
            }
            else
            {
                SetBoxProvider<FuturisticBoxProvider>();
                SetIconProvider<FuturisticIconProvider>();

                currentColorProvider.buttonColor = TidesOfTime.FuturisticButtonColor;
                currentColorProvider.backgroundColor = TidesOfTime.FuturisticBackgroundColor;
            }
        }
    }
}
