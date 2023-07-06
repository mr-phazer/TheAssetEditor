using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Text;
using CommonControls.ModelFiles;


namespace CommonControls.ModelFiles.FBX
{

    public class Node
    {
        string name;
        List<Node> children;
    }

    public struct BoneInfo
    {
        public string name;
        public int id;
        public int parentId;

    }

    public struct AnimationKey
    {
        Vector3 translate;
        Quaternion rotation;
    }

    public class Animation
    {

    }

    public class FBXSceneContainer
    {
        private Node _rootNode;

        public List<PackedMesh> Meshes { get; set; } = new List<PackedMesh>();
        public List<BoneInfo> Bones { get; set; } = new List<BoneInfo>();
        public List<Animation> Animations { get; set; } = new List<Animation>();

        public Node? RootNode { get; set; }



    }
};
