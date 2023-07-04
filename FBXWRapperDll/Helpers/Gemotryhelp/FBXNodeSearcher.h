#pragma once

#include "../../Helpers/Tools.h"
#include "../../FBXServices/FBXMeshGeometryService.h"
 

namespace wrapdll
{
	class FBXNodeSearcher
	{
	public:
		static std::string FetchSkeletonNameFromScene(fbxsdk::FbxScene* pScene)

		{
			std::string tempSkeletonString = "s";
			auto parent = pScene->GetRootNode();

			SearchNodesRecursiveLocal(parent, tempSkeletonString);

			return tempSkeletonString;
		}


		static void SearchNodesRecursiveLocal(fbxsdk::FbxNode* parent, std::string& skeletonString)
		{
			if (!skeletonString.empty()) // to make sure the recursive stops when string is set
				return;

			const std::string nodeTag = "skeleton//"; // a node int scenegraph starts witth these char, if skelton info is set by the export

			for (size_t nodeIndex = 0; nodeIndex < parent->GetChildCount(); nodeIndex++)
			{
				auto currentNode = parent->GetChild(nodeIndex);

				if (std::tolower(currentNode->GetName()).find(nodeTag) == 0())
				{
					skeletonString = std::string(currentNode->GetName()).erase(0, nodeTag.size());

					return;
				}
				SearchNodesRecursiveLocal(parent, skeletonString);
			}
		}

		static bool FindMeshesInScene(fbxsdk::FbxScene* poScene, std::vector<fbxsdk::FbxMesh*>& fbxMeshes)
		{
			if (!poScene)
				return false;

			auto poRootNode = poScene->GetRootNode();

			if (!poRootNode)
				return false;

			FindFbxMeshesRecursive(poRootNode, fbxMeshes);

			return true;
		}

	private:
		static void FindFbxMeshesRecursive(fbxsdk::FbxNode* poParent, std::vector<fbxsdk::FbxMesh*>& fbxMeshes)
		{
			auto DEBUG_childCount = poParent->GetChildCount(); // TODO: REMOVE;

			for (int childBoneIndex = 0; childBoneIndex < poParent->GetChildCount(); ++childBoneIndex)
			{
				fbxsdk::FbxNode* poChildItem = poParent->GetChild(childBoneIndex);

				if (poChildItem)
				{
					auto poFbxNodeAtrribute = poChildItem->GetNodeAttribute();
					if (poFbxNodeAtrribute) // valid node attribute?
					{
						if (poFbxNodeAtrribute->GetAttributeType() == fbxsdk::FbxNodeAttribute::EType::eMesh) // node has "eMesh" attribute, so should contain mesh object
						{
							fbxsdk::FbxMesh* poMeshNode = (fbxsdk::FbxMesh*)poChildItem->GetNodeAttribute(); // get mesh objec ptr

							if (poMeshNode)
							{
								fbxMeshes.push_back(poMeshNode);
							}
						}
					}
				}

				// recurse
				FindFbxMeshesRecursive(poChildItem, fbxMeshes);
			}
		}
	};
}