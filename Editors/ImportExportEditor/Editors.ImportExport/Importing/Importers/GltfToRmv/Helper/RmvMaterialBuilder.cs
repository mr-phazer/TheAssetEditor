using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editors.ImportExport.Importing.Importers.PngToDds;
using Shared.Core.PackFiles;
using Shared.Core.PackFiles.Models;
using Shared.Core.Services;
using Shared.Core.Settings;
using Shared.GameFormats.RigidModel;
using Shared.GameFormats.RigidModel.Types;
using SharpDX.DirectWrite;
using SharpGLTF.Schema2;
using TextureType = Shared.GameFormats.RigidModel.Types.TextureType;

namespace Editors.ImportExport.Importing.Importers.GltfToRmv.Helper
{
    // TODO: MOVE THIS TO A SHARED LOCATION
    public class TextureTypeHelper
    {
        static private readonly Dictionary<string, (TextureType, string)> _stringIdToTextureType = new Dictionary<string, (TextureType, string)>()
            {
                {"BaseColor", (TextureType.BaseColour, "base_colour")},
                {"Normal", (TextureType.Normal, "normal")},
                {"MetallicRoughness", (TextureType.MaterialMap, "material_map")},
                {"Diffuse", (TextureType.Diffuse, "diffuse")},
                {"Specular", (TextureType.Specular, "specular")},
                {"Glossiness", (TextureType.Gloss, "gloss_map") }
            };

        static public bool GetRmvTextureTypeFromGltfIdString(string textureTypeString, out TextureType outTextureType, out string postFix)
        {
            if (_stringIdToTextureType.TryGetValue(textureTypeString, out var textureType))
            {
                outTextureType = textureType.Item1;
                postFix = textureType.Item2;
                return true;
            }

            outTextureType = TextureType.Diffuse;
            postFix = "diffuse";
            return false;
        }
    }
    public class RmvMaterialBuilder
    {

        private readonly IPackFileService _packFileService;
        private readonly IStandardDialogs _exceptionService;
        //private List<string> _addedFiles = new List<string>();

        public RmvMaterialBuilder(IPackFileService packFileSerivce, IStandardDialogs exceptionService)
        {
            _packFileService = packFileSerivce;
            _exceptionService = exceptionService;
        }
        public void BuildRmvFileMaterials(GltfImporterSettings settings, SharpGLTF.Schema2.ModelRoot modelRoot, RmvFile rmvFile)
        {
            if (modelRoot == null)
                throw new ArgumentNullException(nameof(modelRoot), "Invalid Scene: ModelRoot can't be null");

            if (modelRoot.LogicalNodes == null)
                throw new ArgumentNullException(nameof(modelRoot), "Invalid Scene: root.LogicalNodes can't be null");

            if (!modelRoot.LogicalNodes.Any())
                throw new Exception("Invalid Scene: no (logical) nodes");

            if (!rmvFile.ModelList.Any())
                throw new Exception("ERROR: unexpected not meshes in rmv2 file struct");
            
            if (rmvFile.ModelList[0].Length != modelRoot.LogicalMeshes.Count)
                throw new Exception("ERROR: unexpected rmv2 mesh count mismatch");            

            for (int i = 0; i < modelRoot.LogicalMeshes.Count; i++)
            {
                BuildRmvModelMaterial(settings, modelRoot.LogicalMeshes[i], rmvFile.ModelList[0][i]);
            }
        }

        private void BuildRmvModelMaterial(GltfImporterSettings settings, SharpGLTF.Schema2.Mesh mesh, RmvModel rmvModel)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh), "Invalid Mesh: Mesh can't be null");

            if (mesh.Primitives == null || !mesh.Primitives.Any())
                throw new Exception($"Invalid Mesh: No Primitives found in mesh. Primitives.Count = {mesh.Primitives?.Count}");

            var primitive = mesh.Primitives.First();

            if (primitive == null)
                throw new Exception("Invalid Mesh: primitive[0] can't be null ");

            var gltfMaterial = primitive.Material;

            if (gltfMaterial == null)
                throw new Exception("Invalid Mesh: gltfMaterial can't be null ");

            foreach (var itText in gltfMaterial.Channels)
            {                
                if (itText.Texture == null) continue;

                var texPath = itText.Texture.PrimaryImage.Content.SourcePath;
                if (!TextureTypeHelper.GetRmvTextureTypeFromGltfIdString(
                    itText.Key,
                    out var textureType,
                    out var postFixString)) continue; // gltf string id doesn't match any of the rmv texture types
                
                var gameType = GameTypeEnum.Warhammer3; // TODO: get this from application settings                              

                // set file name
                var textureNameBase = mesh.Name.Any() ? mesh.Name : Path.GetFileNameWithoutExtension(settings.InputGltfFile);
                var textureFileName = @$"{textureNameBase}_{postFixString}.dds";

                var texturePackFolder = settings.DestinationPackPath + @"\tex";
                var textureFullPackPath = @$"{texturePackFolder}\{textureFileName}";

                // import texture PNG -> DDS
                var ddsPackFile = PngToDdsImporter.Import(texPath, textureType, gameType, textureFileName);

                // set texture in RMVmodel
                rmvModel.Material.SetTexture(textureType, textureFullPackPath);

                // make sure we don't add the same file more the once
                if (_packFileService.FindFile(textureFullPackPath.ToLower()) != null)
                    return;                

                var newFile = new NewPackFileEntry(texturePackFolder, ddsPackFile);
                _packFileService.AddFilesToPack(settings.DestinationPackFileContainer, [newFile]);               
            }
        }

        private void AddTexture(GltfImporterSettings settings, RmvModel rmvModel, TextureType textureType, PackFile ddsPackFile)
        {
        }
    }
}

