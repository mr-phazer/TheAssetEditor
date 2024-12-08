using Shared.Core.PackFiles.Models;
using Shared.GameFormats.RigidModel;
using SharpGLTF.Schema2;
using Editors.ImportExport.Importing.Importers.GltfToRmv.Helper;
using System.IO;
using Shared.Core.PackFiles;
using Shared.Ui.BaseDialogs.PackFileBrowser;
using static Shared.Core.PackFiles.IPackFileService;
using Shared.Core.ErrorHandling.Exceptions;
using Shared.Core.Services;
using Serilog;
using Shared.Core.ErrorHandling;

namespace Editors.ImportExport.Importing.Importers.GltfToRmv
{

    public class GltfImporter
    {
        private readonly ILogger _logger = Logging.Create<GltfImporter>();
        private readonly IPackFileService _packFileService;
        private readonly IStandardDialogs _exceptionService;

        public GltfImporter(IPackFileService packFileSerivce, IStandardDialogs exceptionService)
        {
            _packFileService = packFileSerivce;
            _exceptionService = exceptionService;
        }

        private ModelRoot? _modelRoot;

        public void Import(GltfImporterSettings settings)
        {
            LogSettings(settings);

            try
            {
                _modelRoot = ModelRoot.Load(settings.InputGltfFile);
            }
            catch (Exception ex)
            {
                _exceptionService.ShowExceptionWindow(ex);
                return;
            }

            var importedFileName = GetImportedPackFileName(settings);

            var rmv2File = RmvMeshBuilder.Build(settings, _modelRoot);
            var bytesRmv2 = ModelFactory.Create().Save(rmv2File);

            var packFileImported = new PackFile(importedFileName, new MemorySource(bytesRmv2));

            var newFile = new NewPackFileEntry(settings.DestinationPackPath, packFileImported);
            _packFileService.AddFilesToPack(settings.DestinationPackFileContainer, [newFile]);
        }

        private static string GetImportedPackFileName(GltfImporterSettings settings)
        {
            var fileName = Path.GetFileNameWithoutExtension(settings.InputGltfFile);
            string importedFileName = $@"{fileName}.rigid_model_v2";

            return importedFileName;
        }

        void LogSettings(GltfImporterSettings settings)
        {
            var str = $"Importing using {nameof(GltfImporter)}\n";
            str += $"\tInputGltfFile:{settings?.InputGltfFile}\n";
            str += $"\tDestinationPackFileContainer:{settings?.DestinationPackFileContainer}\n";
            str += $"\tDestinationPackPath:{settings?.DestinationPackPath}\n";
            str += $"\tConvertNormalTextureToBlue:{settings?.ConvertNormalTextureToOrangeType}n";
            str += $"\tImportAnimations:{settings?.ImportAnimations}n";
            str += $"\tMirrorMesh:{settings?.MirrorMesh}\n";

            _logger.Here().Information(str);
        }

    }
}
