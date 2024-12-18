namespace Shared.Ui.BaseDialogs.PackFileTree.ContextMenu.External
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
