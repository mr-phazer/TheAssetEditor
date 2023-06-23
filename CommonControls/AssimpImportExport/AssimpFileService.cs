using System.IO;
using CommonControls.FileTypes.RigidModel;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.Services;
using CommonControls.Common;
using Serilog;
using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Assimp;
using CommonControls.FileTypes.Animation;
using Filetypes.ByteParsing;
using System.Linq;
using static CommonControls.Editors.AnimationPack.Converters.AnimationBinFileToXmlConverter;

namespace CommonControls.AssimpImportExport
{
    public class AssimpFileService
    {
        ILogger _logger = Logging.Create<AssimpFileService>();

        private PackFileService _packFileService;
        private Assimp.Scene _assimpScene;
        public AssimpFileService(PackFileService pfs, Assimp.Scene scene)
        {
            _packFileService = pfs;
            _assimpScene = scene;
        }

        private AssimpFileService() { }

        public string FetchSkeletonNameFromScene()
        {
            string tempSkeletonString = "";

            var parent = _assimpScene.RootNode;

            SearchNodesRecursiveLocal(parent, ref tempSkeletonString);

            return tempSkeletonString;
        }

        private void SearchNodesRecursiveLocal(Node parent, ref string skeletonString)
        {
            foreach (var node in parent.Children)
            {
                if (node.Name.Contains("skeleton"))
                    skeletonString = node.Name.Replace("skeleton//", "");

                if (skeletonString.Length > 0)
                    return;

                SearchNodesRecursiveLocal(node, ref skeletonString);
            }
        }
        /// <summary>
        /// Loads skeleton ANIM file        
        /// </summary>
        /// <param name="inputName">Skeleton name, if empty scene is searched for a skeleton name</param>
        /// <returns></returns>
        public AnimationFile LoadSkeletonFileByIdString(string inputName = "")
        {            
            var name = inputName.Any() ? inputName : FetchSkeletonNameFromScene(); // 
            
            var skeletonFolder = @"animations\skeletons\";
            var animExt = "anim";
            var fullPath = $"{skeletonFolder}{name}.{animExt}";

            return  LoadSkeletonFileByPath(fullPath);        
        }

        public AnimationFile LoadSkeletonFileByPath(string fullPath)        {         

            var packFileSkeleton = _packFileService.FindFile(fullPath);

            if (packFileSkeleton == null)
            {
                _logger.Here().Warning($"Failed to Find skeleton '{fullPath}', it doesn't exist.");
                MessageBox.Show($"Couldn't find skeleton '{fullPath}' \rMake sure to Load All CA Packs before importing Rigged Models!\rOr add the appropiate skeleton to your project\r\rFile Will be imported as a non-rigged model.", "Skeleton Missing Warning");
                return null;
            }

            var rawByteDataSkeleton = packFileSkeleton.DataSource.ReadData();
            var skeletonFile = AnimationFile.Create(new ByteChunk(rawByteDataSkeleton));
            return skeletonFile;
        }

        public void ImportAssimpDiskFileToPack(PackFileContainer container, string parentPackPath, string filePath)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            var rigidModelExtension = ".rigid_model_v2";
            var outFileName = fileNameNoExt + rigidModelExtension;

            var assimpImporterService = new AssimpImporter(_packFileService);
            assimpImporterService.ImportScene(filePath);

            var rmv2File = assimpImporterService.MakeRMV2File();
            var factory = ModelFactory.Create();
            var buffer = factory.Save(rmv2File);

            var packFile = new PackFile(outFileName, new MemorySource(buffer));
            _packFileService.AddFileToPack(container, parentPackPath, packFile);
        }

       public void Import3dModelToPackTree(PackFileContainer owner, string parentPath)       
       {            
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = false;
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var files = dialog.FileNames;
                foreach (var file in files)
                {
                    try
                    {                        
                        ImportAssimpDiskFileToPack(owner, parentPath, file);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Failed to import model file {file}. Error : {e.Message}", "Error");
                        _logger.Here().Error($"Failed to load file {file}. Error : {e}");
                    }
                }
            }
        }

        static public string GetDialogFilterSupportedFormats()
        {
            var unmangedLibrary = Assimp.Unmanaged.AssimpLibrary.Instance;
            var suportetFileExtensions = unmangedLibrary.GetExtensionList();

            var filter = "3d Models (ALL)|";

            // All model formats in one
            foreach (var ext in suportetFileExtensions)
            {
                filter += "*" + ext + ";";
            }

            // ech model format separately
            foreach (var ext in suportetFileExtensions)
            {
                filter += "|" + ext.Remove(0, 1) + "(" + ext + ")|" + "*" + ext;
            }

            filter += "|All files(*.*) | *.*";

            return filter;
        }
    }
}
