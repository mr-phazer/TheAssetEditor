using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CommonControls.FileTypes.RigidModel.LodHeader;
using CommonControls.FileTypes.RigidModel.MaterialHeaders;
using CommonControls.FileTypes.RigidModel;
using CommonControls.FileTypes.RigidModel.Vertex;
using Assimp;
using CommonControls.FileTypes.Animation;
using System.Windows.Forms;
using Filetypes.ByteParsing;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.Services;
using Serilog;
using CommonControls.Common;
using Assimp.Unmanaged;
using System.IO;


namespace CommonControls.AssimpImportExport
{
    public class AssimpUtil
    {
        public static void ImportAssimpDiskFileToPack(PackFileService pfs, PackFileContainer container, string parentPackPath, string filePath)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            var rigidModelExtension = ".rigid_model_v2";
            var outFileName = fileNameNoExt + rigidModelExtension;

            var assimpImport = new AssimpImporter(pfs);
            assimpImport.ImportScene(filePath);

            var rmv2File = assimpImport.MakeRMV2File();
            var factory = ModelFactory.Create();
            var buffer = factory.Save(rmv2File);

            var packFile = new PackFile(outFileName, new MemorySource(buffer));
            pfs.AddFileToPack(container, parentPackPath, packFile);
        }

        static public string GetDialogFilterStringSupportedFormats()
        {
            var unmangedLibrary = Assimp.Unmanaged.AssimpLibrary.Instance;
            var suportetFileExtensions = unmangedLibrary.GetExtensionList();

            var filter = "3d Models (ALL)|";
            // Example: \"Image files (*.bmp, *.jpg)|*.bmp;*.jpg|All files (*.*)|*.*\"'

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
