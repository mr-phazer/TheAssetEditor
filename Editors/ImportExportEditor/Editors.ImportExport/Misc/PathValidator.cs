using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editors.ImportExport.Misc
{
    /// <summary>
    /// Check that path does not contain invalid characters
    /// </summary>
    public static class PathValidator
    {
        public static bool IsValidPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if (!IsValidFileName(fileName))
            {
                return false;
            }

            if (!IsValidFolder(fileName))
            {
                return false;
            }

            return true;
        }

        private static bool IsValidFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var fileNamOnly = Path.GetFileName(fileName);
            
            foreach (var c in fileNamOnly)
            {
                if (Array.Exists(invalidChars, invalidChar => invalidChar == c))                
                    return false;
            }

            return true;
        }

        private static bool IsValidFolder(string fileName)
        {
            var invalidChars = Path.GetInvalidPathChars();
            var folder = Path.GetDirectoryName(fileName);
            if (folder == null)
                throw new ArgumentNullException(nameof(folder), "Fatal Eroor, cannot be null");

            foreach (var c in folder)
            {
                if (Array.Exists(invalidChars, invalidChar => invalidChar == c))                
                    return false;
            }

            return true;
        }
    }

}
