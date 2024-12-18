using System.Windows.Forms;
using Shared.Core.Events;
using Shared.Core.PackFiles.Models;
using TreeNode = Shared.Ui.BaseDialogs.PackFileBrowser.TreeNode;
using Shared.Core.PackFiles;
using Editors.ImportExport.Importing.Importers.GltfToRmv;
using System.Windows.Forms.Design;
using Shared.Core.Settings;
using Shared.Core.Misc;
using Editors.ImportExport.Importing.Presentation;

namespace Editors.ImportExport.Importing
{
    public class DisplayImportFileToolCommand : IUiCommand
    {
        private readonly IAbstractFormFactory<ImportWindow> _exportWindowFactory;

        public DisplayImportFileToolCommand(IAbstractFormFactory<ImportWindow> exportWindowFactory)
        {
            _exportWindowFactory = exportWindowFactory;
        }

        public void Execute(PackFileContainer packFileContainer, string packPath)
        {
            var path = "";
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                //FileName = Path.GetFileNameWithoutExtension(_inputFile!.Name),
                //DefaultExt = SelectedImporter!.OutputExtension,
                //Filter = $"File ({SelectedImporter!.OutputExtension})|*{SelectedImporter!.OutputExtension}"
            };

            if (dlg.ShowDialog() == true)
                path = dlg.FileName;



            var window = _exportWindowFactory.Create();
            window.Initialize(packFileContainer, packPath, path);
            window.ShowDialog();
        }

    }


    //public class DisplayImportFileToolCommand : IUiCommand
    //{   
    //    // TODO: ?

    //    private readonly GltfImporter _importer;     


    //    public DisplayImportFileToolCommand(GltfImporter importer)
    //    {            
    //        // TODO: ?
    //        //_ImportWindowFactory = importWindowFactory;
    //        _importer = importer;
    //    }

    //    public void Execute(TreeNode clickedNode, GameTypeEnum gameType)
    //    {
    //        var glftFilePath = GetFileFromDiskDialog();
    //        if (string.IsNullOrEmpty(glftFilePath))
    //            return;

    //        var settings = new GltfImporterSettings(glftFilePath, clickedNode.GetFullPath(), clickedNode.FileOwner, gameType, true, true, true);
    //        _importer.Import(settings);
    //    }

    //    private static string GetFileFromDiskDialog()
    //    {
    //        string filePath = "";
    //        using (OpenFileDialog openFileDialog = new OpenFileDialog())
    //        {
    //            openFileDialog.InitialDirectory = "c:\\";
    //            openFileDialog.Filter = "All files (*.*)|*.*|GLTF model files (*.gltf)|*.gltf";
    //            openFileDialog.FilterIndex = 2;
    //            openFileDialog.RestoreDirectory = true;

    //            if (openFileDialog.ShowDialog() == DialogResult.OK)
    //            {
    //                // Get the path of specified file
    //                filePath = openFileDialog.FileName;
    //            }
    //            else
    //            {
    //                filePath = "";
    //            }
    //        }

    //        return filePath;
    //    }
    //}
}
