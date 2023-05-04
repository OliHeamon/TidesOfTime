using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Terraria.ModLoader.Core.TmodFile;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.Core;
using Terraria.ModLoader;
using Terraria;

namespace TidesOfTime
{
    public class TidesOfTimeShaderLoader : ILoadable
    {
        public void Load(Mod mod)
        {
            if (Main.dedServ)
            {
                return;
            }

            MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);

            TmodFile file = (TmodFile)info.Invoke(mod, null);

            IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Assets/Effects/") && n.Name.EndsWith(".xnb"));

            foreach (FileEntry entry in shaders)
            {
                string name = entry.Name.Replace(".xnb", "").Replace("Assets/Effects/", "");
                string path = entry.Name.Replace(".xnb", "");

                LoadShader(mod, name, path);
            }
        }

        public void Unload()
        {
        }

        private void LoadShader(Mod mod, string name, string path)
        {
            Ref<Effect> shader = new(mod.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);

            Filters.Scene[name] = new Filter(new ScreenShaderData(shader, name + "Pass"), EffectPriority.High);
            Filters.Scene[name].Load();
        }
    }
}
