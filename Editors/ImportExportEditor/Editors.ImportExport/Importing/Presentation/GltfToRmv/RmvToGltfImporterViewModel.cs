﻿using CommunityToolkit.Mvvm.ComponentModel;
using Editors.ImportExport.Exporting.Exporters.RmvToGltf;
using Editors.ImportExport.Exporting.Presentation.RmvToGltf;
using Editors.ImportExport.Importing.Importers;
using Editors.ImportExport.Importing.Importers.GltfToRmv;
using Editors.ImportExport.Importing.Presentation;
using Shared.Core.PackFiles.Models;
using Shared.Core.Settings;
using Shared.Ui.Common.DataTemplates;
using Editors.ImportExport.Importing.Presentation.RmvToGltf;
using Editors.ImportExport.Common;
using Editors.ImportExport.Misc;

namespace Editors.ImportImport.Importing.Presentation.RmvToGltf
{
    public partial class RmvToGltfImporterViewModel : ObservableObject, IImporterViewModel, IViewProvider<RmvToGltfImporterView>
    {
        private readonly GltfImporter _Importer;

        public string DisplayName => "Gltf Importer";
        public string OutputExtension => ".rigid_model_v2";
        public string[] InputExtensions => new string[] { ".gltf", ".glb" };

        [ObservableProperty] bool _importMeshes = true;        
        [ObservableProperty] bool _importMaterials = true;
        [ObservableProperty] bool _convertFromBlenderMaterialMap = true;
        [ObservableProperty] bool _convertNormalTextureToOrange = true;
        [ObservableProperty] bool _importAnimations = true;
        [ObservableProperty] float _animationKeysPerSecond = 20.0f;

        public RmvToGltfImporterViewModel(GltfImporter Importer)
        {
            _Importer = Importer;
        }

        public ImportSupportEnum CanImportFile(PackFile file) => _Importer.CanImportFile(file);

        public void Execute(PackFile importSource, string outputPath, PackFileContainer packFileContainer, GameTypeEnum gameType)
        {
            var settings = new GltfImporterSettings(
                InputGltfFile: importSource.Name,
                DestinationPackPath: outputPath,
                SelectedGame: gameType,
                DestinationPackFileContainer: packFileContainer,
                ImportMeshes: this.ImportMeshes,
                ImportMaterials: this.ImportMaterials,
                ConvertNormalTextureToOrangeType: this.ConvertNormalTextureToOrange,
                ImportAnimations: this.ImportAnimations,            
                MirrorMesh: true); 
                        
            _Importer.Import(settings);
        }
    }
}
