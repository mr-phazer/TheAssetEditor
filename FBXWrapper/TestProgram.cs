using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FBXWrapper.Structs;
using System.Collections.Generic;
using FBXWrapper.Importer;

namespace FBXWrapper
{ 
    public class TestProgram

    {
        static void Main(string[] args)
        {
            

            var fbxSceneContainer = DLLFunctionsFBXSDK.CreateFBXContainer();
            DLLFunctionsFBXSDK.CreateSceneFBX(fbxSceneContainer, @"H:\Fbx\emp_karl_franz_humanoid01_BindPose.fbx");

            var meshes = FBXSceneImorterService.GetAllPackedMeshes(fbxSceneContainer);

            var DEBUG_BREAK = 1;
        }

    }
}
