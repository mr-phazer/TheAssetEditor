﻿using Shared.Core.PackFiles.Models;
using Shared.GameFormats.RigidModel;
using SharpGLTF.Schema2;
using Editors.ImportExport.Importing.Importers.GltfToRmv.Helper;
using System.IO;
using Shared.Core.PackFiles;
using Shared.Ui.BaseDialogs.PackFileBrowser;
using static Shared.Core.PackFiles.IPackFileService;
using Shared.Core.Services;
using Shared.Core.ErrorHandling.Exceptions;
using Editors.Shared.Core.Services;
using System.Windows;
using Shared.GameFormats.Animation;
using Shared.Core.ErrorHandling;
using CommonControls.BaseDialogs.ErrorListDialog;
using SharpGLTF.Materials;
using Editors.ImportExport.Misc;

namespace Editors.ImportExport.Importing.Importers.GltfToRmv
{
    public class GltfImporter
    {
        private readonly IPackFileService _packFileService;
        private readonly IStandardDialogs _exceptionService;
        private readonly SkeletonAnimationLookUpHelper _skeletonLookUpHelper;
        private readonly RmvMaterialBuilder _materialBuilder;

        public GltfImporter(IPackFileService packFileService, IStandardDialogs exceptionService, SkeletonAnimationLookUpHelper skeletonLookUpHelper, RmvMaterialBuilder materialBuilder)
        {
            _packFileService = packFileService;
            _exceptionService = exceptionService;
            _skeletonLookUpHelper = skeletonLookUpHelper;
            _materialBuilder = materialBuilder;
        }

        public void Import(GltfImporterSettings settings)
        {
            ModelRoot? modelRoot = null;
            try
            {
                modelRoot = ModelRoot.Load(settings.InputGltfFile);
            }
            catch (Exception ex)
            {
                _exceptionService.ShowExceptionWindow(ex);
                return;
            }

            var importedFileName = GetImportedPackFileName(settings);

            var skeletonName = FetchSkeletonIdStringFromScene(modelRoot);
            if (skeletonName == null)
                throw new ArgumentNullException(nameof(skeletonName), "Fatal eroro: This shouldn't be null");

            AnimationFile? skeletonAnimFile = null;
            if (skeletonName.Any())
            {
                skeletonAnimFile = _skeletonLookUpHelper.GetSkeletonFileFromName(skeletonName);

                if (skeletonAnimFile == null)
                {
                    var errorList = new ErrorList();
                    errorList.Error("Skeleton Not Found", $"Skeleton named '{skeletonName}' could not be found\nHave you selected the correct game AND loaded all CA Pack Files?");

                    ErrorListWindow.ShowDialog("Skeleton Error", errorList);

                    return;
                }
            }

            var rmv2File = RmvMeshBuilder.Build(settings, modelRoot, skeletonAnimFile, skeletonName);

            _materialBuilder.BuildRmvFileMaterials(settings, modelRoot, rmv2File);

            rmv2File.RecalculateOffsets();

            var bytesRmv2 = ModelFactory.Create().Save(rmv2File);
            var packFileImported = new PackFile(importedFileName, new MemorySource(bytesRmv2));
            var newFile = new NewPackFileEntry(settings.DestinationPackPath, packFileImported);
            _packFileService.AddFilesToPack(settings.DestinationPackFileContainer, [newFile]);
        }

        internal ImportSupportEnum CanImportFile(PackFile file)
        {
            if (FileExtensionHelper.IsGltfGile(file.Name))
                return ImportSupportEnum.HighPriority;
            
            return ImportSupportEnum.NotSupported;
        }


        private static string GetImportedPackFileName(GltfImporterSettings settings)
        {
            var fileName = Path.GetFileNameWithoutExtension(settings.InputGltfFile);
            string importedFileName = $@"{fileName}.rigid_model_v2";

            return importedFileName;
        }

        private static string FetchSkeletonIdStringFromScene(ModelRoot modelRoot)
        {
            var nodeSearchResult = modelRoot.LogicalNodes.Where(node => node.Name.StartsWith("//skeleton//"));

            if (nodeSearchResult == null || !nodeSearchResult.Any())
                return "";

            var skeletonName = nodeSearchResult.First().Name.TrimStart("//skeleton//".ToCharArray());

            return skeletonName.ToLower();
        }
    }
}
