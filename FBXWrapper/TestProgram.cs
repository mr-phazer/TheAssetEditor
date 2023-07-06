using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Collections.Generic;


namespace FBXWrapper
{ 
    public class TestProgram

    {
        static void Main(string[] args)
        {
            

            var fbxSceneContainer = FBXSCeneContainerDll.CreateFBXContainer();
            FBXSCeneContainerDll.CreateSceneFBX(fbxSceneContainer, @"H:\Fbx\emp_karl_franz_humanoid01_BindPose.fbx");

            var meshes = FBXSceneImorterService.GetAllPackedMeshes(fbxSceneContainer);

            var DEBUG_BREAK = 1;
        }

    }
}
