﻿using ReLogic.Content.Sources;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.Core;
using Terraria;

namespace TidesOfTime
{
    public class TidesOfTimeContentLoader : ContentSource
    {
        private readonly TmodFile file;

        public TidesOfTimeContentLoader(TmodFile file)
        {
            ArgumentNullException.ThrowIfNull(file);

            this.file = file;

            if (Main.dedServ)
                return;

            var assetReaderCollection = Main.instance.Services.GetService(typeof(AssetReaderCollection)) as AssetReaderCollection;

            var files = file.Select(static fileEntry => fileEntry.Name);
            var replacedFileNames = file.Where(static fileEntry => fileEntry.Name.StartsWith("Assets/"))
                .Select(static fileEntry => fileEntry.Name.Replace("Assets", "Content"));

            SetAssetNames(files.Concat(replacedFileNames)
                .Where(name => assetReaderCollection.TryGetReader(Path.GetExtension(name), out _)));
        }

        public override Stream OpenStream(string fullAssetName)
        {
            if (file.HasFile(fullAssetName))
                return file.GetStream(fullAssetName, newFileStream: true);

            if (!fullAssetName.StartsWith("Assets/") && file.HasFile(fullAssetName.Replace("Content", "Assets")))
                return file.GetStream(fullAssetName.Replace("Content", "Assets"), newFileStream: true);

            throw new KeyNotFoundException(fullAssetName);
        }
    }
}
