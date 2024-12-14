using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editors.ImportExport.Importing.Importers.PngToDds.Helpers.ImageProcessor;
using Shared.Core.Settings;
using Shared.GameFormats.RigidModel.Types;

namespace Editors.ImportExport.Importing.Importers.PngToDds.Helpers.ImageProcessor
{

    public static class ImageProcessorFactory
    {
        static private Dictionary<TextureType, IImageProcessor> _textureAndGameTypeToTranformer = new Dictionary<TextureType, IImageProcessor>()
        {
             {TextureType.Diffuse, new DefaultImageProcessor() },
             {TextureType.MaterialMap, new BlenderToCAMaterialMapProcessor() },
             {TextureType.BaseColour, new DefaultImageProcessor() },
             {TextureType.Normal, new BlueToOrangeNormalMapProcessor() }
         
        };

        public static IImageProcessor CreateImageProcessor(TextureType textureType)
        {
            if (_textureAndGameTypeToTranformer.TryGetValue(textureType, out var transformer))
            {
                // TODO: log here "no converter found for texture type {textureType} and game type {gameType}"
                return transformer;

            }

            return new DefaultImageProcessor();
        }
    }
}
