#pragma once

#include <fbxsdk.h>
#include "..\DataStructures\structs.h"
#include "..\Helpers\tools.h"
#include "..\Logging\Logging.h"

class FBXSkinProcessorService
{
public:
	static bool ProcessSkin(FbxMesh* _poSourceFbxMesh, PackedMesh& destPackedMesh, const std::vector < std::string >& boneTable, const std::vector<int>& vertexToControlPoint);

private:
	static bool ExtractInfluencesFromSkin(fbxsdk::FbxSkin* poSkin, fbxsdk::FbxMesh* poFbxMeshNode, PackedMesh& destPackedMesh, const std::vector<std::string>& boneTable, const std::vector<int>& vertexToControlPoint);
};

