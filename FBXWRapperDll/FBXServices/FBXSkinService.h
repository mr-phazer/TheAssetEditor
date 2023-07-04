#pragma once

#include <fbxsdk.h>
#include "..\DataStructures\VertextStructs.h"
#include "..\Helpers\tools.h"
#include "..\Logging\Logging.h"

namespace wrapdll
{
	class FBXSkinProcessorService
	{
	public:
		static bool ProcessSkin(
			FbxMesh* _poSourceFbxMesh,
			PackedMesh& destPackedMesh,
			const std::vector < std::string >& boneTable,
			std::vector<ControlPointInfluences>& controlPointInfluences);

	private:
		static bool ExtractInfluencesFromSkin(
			fbxsdk::FbxSkin* poSkin,
			fbxsdk::FbxMesh* poFbxMeshNode,
			PackedMesh& destPackedMesh,
			const std::vector<std::string>& boneTable,
			std::vector<ControlPointInfluences>& controlPointInfluences);
	};

}
