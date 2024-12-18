using CommunityToolkit.Mvvm.ComponentModel;
using Editors.ImportExport.Exporting.Exporters.RmvToGltf;
using Editors.ImportExport.Exporting.Presentation.RmvToGltf;
using Editors.ImportExport.Importing.Importers;
using Editors.ImportExport.Importing.Importers.GltfToRmv;
using Editors.ImportExport.Misc;
using Editors.ImportExport.Importing.Presentation;
using Shared.Core.PackFiles.Models;
using Shared.Core.Settings;
using Shared.Ui.Common.DataTemplates;
using Editors.ImportExport.Importing.Presentation.RmvToGltf;

namespace Editors.ImportImport.Importing.Presentation.RmvToGltf
{
    internal partial class RmvToGltfImporterViewModel : ObservableObject, IImporterViewModel, IViewProvider<RmvToGltfImporterView>
    {
        private readonly GltfImporter _Importer;

        public string DisplayName => "Gltf_To_Rmv2";
        public string OutputExtension => ".rigid_model_v2";

        [ObservableProperty] bool _importMaterials = true;
        [ObservableProperty] bool _convertFromBlenderMaterialMap = true;
        [ObservableProperty] bool _convertNormalTextureToOrange = true;
        [ObservableProperty] bool _ImportAnimations = true;
        [ObservableProperty] float _AnmiationKeysPerSecond = 20.0f;

        public RmvToGltfImporterViewModel(GltfImporter Importer)
        {
            _Importer = Importer;
        }

        public ImportSupportEnum CanImportFile(PackFile file) => _Importer.CanImportFile(file);

        public void Execute(PackFile ImportSource, string outputPath, PackFileContainer packFileContainer, GameTypeEnum gameType)
        {
            var settings = new GltfImporterSettings(ImportSource.Name, outputPath, packFileContainer, gameType, true, true, true);
            
            _Importer.Import(settings);
        }
    }
}
