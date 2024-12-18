using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Core.Events.Global;
using Shared.Core.PackFiles.Models;

namespace Shared.Ui.BaseDialogs.PackFileBrowser.ContextMenu.External
{
    public interface IImportFileContextMenuHelper
    {
        /// <summary>
        /// Implemeation checks if the can be file imported
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool CanImportFile(PackFile packFile);

        // implemenation returns packfile container, so it can create new foldders
        public void ShowDialog(TreeNode nodeClickedOn);
    }
}
