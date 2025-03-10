using System.Windows.Forms;
using Editors.ImportExport.Importing.Importers.GltfToRmv;
using Shared.Core.Events;
using Shared.Core.Settings;
using TreeNode = Shared.Ui.BaseDialogs.PackFileTree.TreeNode;

namespace Editors.ImportExport.Importing
{
    public class DisplayImportFileToolCommand : IUiCommand
    {   
        // TODO: ?
   
        private readonly GltfImporter _importer;
        private readonly ApplicationSettingsService _settingsService;

        public DisplayImportFileToolCommand(GltfImporter importer, ApplicationSettingsService settingsService)
 
        {            
            // TODO: ?
            //_ImportWindowFactory = importWindowFactory;
            _importer = importer;
            _settingsService = settingsService;
        }


        public void Execute(TreeNode clickedNode)
        {
            var glftFilePath = GetFileFromDiskDialog();
            if (string.IsNullOrEmpty(glftFilePath))
                return;


            var selectGaame = _settingsService.CurrentSettings.CurrentGame;

            var settings = new GltfImporterSettings(glftFilePath, clickedNode.GetFullPath(), selectGaame, clickedNode.FileOwner, true, true, true, true, true);
            _importer.Import(settings);
        }

        private static string GetFileFromDiskDialog()
        {
            string filePath = "";
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All files (*.*)|*.*|GLTF model files (*.gltf)|*.gltf";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
                else
                {
                    filePath = "";
                }
            }

            return filePath;
        }
    }
}
