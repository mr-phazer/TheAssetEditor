using CsvHelper.Expressions;
using Shared.Core.PackFiles.Models;
using Shared.Core.Settings;

namespace Editors.ImportExport.Importing.Importers.GltfToRmv
{
    public record GltfImporterSettings
    (
        string InputGltfFile,
        string DestinationPackPath,
        GameTypeEnum SelectedGame,
        PackFileContainer DestinationPackFileContainer,
        bool ImportMeshes, 
        bool ImportMaterials, 
        bool ConvertNormalTextureToOrangeType,
        bool ImportAnimations,
        bool MirrorMesh
    );
}
