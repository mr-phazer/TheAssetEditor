﻿using Shared.Core.PackFiles.Models;
using Shared.GameFormats.RigidModel;
using SharpGLTF.Schema2;
using Editors.ImportExport.Importing.Importers.GltfToRmv.Helper;
using System.IO;
using Shared.Core.PackFiles;
using Shared.Ui.BaseDialogs.PackFileBrowser;
using static Shared.Core.PackFiles.IPackFileService;
using Shared.Core.ErrorHandling.Exceptions;
using Shared.Core.Services;
using Shared.Core.Settings;

namespace Editors.ImportExport.Importing.Importers.GltfToRmv
{
    public record GltfImporterSettings
    (
        string InputGltfFile,
        string DestinationPackPath,
        PackFileContainer DestinationPackFileContainer,
        GameTypeEnum GameType,
        bool ConvertNormalTextureToOrangeType,
        bool ImportAnimations,
        bool MirrorMesh
    );
}
