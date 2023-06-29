using CommonControls.FileTypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonControls.ExtModelImport
{    
    enum FormatEnum { FBX, Stuff};

    public interface IMeshImporter
    {
        abstract RmvMesh ConvertMesh(IntPtr inputMesh);        
    }
    public interface FBXMeshImport : IMeshImporter
    {
        public RmvMesh ConvertMesh(IntPtr inputMesh);
        static public List<string> GetSupportedExtentions() { return new List<String>() { "fbx" }; }
    }

    public class MeshImporter
    {
        private Dictionary<String, IMeshImporter> _importer;

        RmvMesh DoConversion(string ext, IntPtr input)
        {          
            return _importer[ext].ConvertMesh(input);
        }
    }
}
