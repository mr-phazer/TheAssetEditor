using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Editors.ImportExport.Exporting.Exporters.RmvToGltf.Helpers;
using GameWorld.Core.SceneNodes;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;
using GLTF = SharpGLTF.Schema2;
using Shared.Core.PackFiles.Models;
using Shared.GameFormats.RigidModel;
using Shared.GameFormats.RigidModel.Vertex;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Transforms;
using SharpGLTF.Schema2;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using Editors.ImportExport.Importing.Importers.GltfToRmv.Helper;
using System.IO;
using GameWorld.Core.Animation;
using GameWorld.Core.Rendering.Geometry;



namespace Editors.ImportExport.Importing.Importers.GltfToRmv
{
    public record GltfImporterSettings(
       string InputGltfFile,
       bool ConvertNormalTextureToOrnge,
       PackFileContainer dest
   );

    public class GltfImporter
    {
        private ModelRoot? _modelRoot;
        public void Import(GltfImporterSettings settings)
        {
            _modelRoot = ModelRoot.Load(settings.InputGltfFile);

            var rmv2File = RmvMeshBuilder.Build(settings, _modelRoot);                        
            var bytesRmv2 = ModelFactory.Create().Save(rmv2File);            

            TESTING_SaveTestFileToDisk(settings, bytesRmv2);
        }

        private static void TESTING_SaveTestFileToDisk(GltfImporterSettings settings, byte[] bytes)
        {
            var docsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileNameOnly = Path.GetFileNameWithoutExtension(settings.InputGltfFile);
            var modelStream = File.Open($@"{docsFolder}\{fileNameOnly}.rigid_model_v2", FileMode.Create);
            var binaryWriter = new BinaryWriter(modelStream);
            binaryWriter.Write(bytes);
        }
    }
}
