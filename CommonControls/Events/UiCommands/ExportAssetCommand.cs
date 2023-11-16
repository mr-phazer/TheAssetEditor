using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CommonControls.FileTypes;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.Interfaces.AssetManagement;
using CommonControls.Services;

namespace CommonControls.Events.UiCommands
{
    public class ExportAssetCommand : IUiCommand
    {        
        private readonly PackFileService _packFileService;
        private readonly IAssetExporterProvider _assetManagementFactory;

        public ExportAssetCommand(PackFileService packFileService, IAssetExporterProvider assetExportProvider)
        {
            _packFileService = packFileService;
            _assetManagementFactory = assetExportProvider;
        }

        /// <summary>
        /// Exports complete asset from packfile, input path can be RMV2, WSMODE, VMD
        /// </summary>        
        public void Execute(PackFileContainer fileOwner, string pathModel, string pathAnimationClip = "")
        {
            var exportData = AssetExportHelper.FetchParsedInputFiles(_packFileService, pathModel);
            var fileNameSuffix = (exportData.skeletonFile != null ? $"__{exportData.skeletonFile.Header.SkeletonName}" : "");
            var fileName = $"{Path.GetFileNameWithoutExtension(pathModel)}" + fileNameSuffix;

            // TODO: renable
            //exportData.DestinationPath = GetOpenSaveFile(fileName);
            var dateTime = new DateTime();
            
            string time = dateTime.TimeOfDay.Ticks.ToString();


            exportData.DestinationPath = $"H:\\Documents\\{fileName}__{time}.fbx";


            if (!exportData.DestinationPath.Any())            
                return;            

            try
            {
                var exporter = _assetManagementFactory.GetExporter(".fbx");
                var binaryFileData = exporter.ExportAsset(exportData);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to export geomeotry to file {exportData.SourcePath}. Error : {e.Message}", "Error");

            }
        }

        private string GetOpenSaveFile(string initialPath = "")
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = initialPath;
                saveFileDialog.Filter = "FBX Files (*.fbx)|*.fbx|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = initialPath;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = saveFileDialog.FileName;

                    return filePath;
                }
            }

            return "";
        }
    }


}



