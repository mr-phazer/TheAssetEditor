// T-he following ifdef block is the standard way of creating macros which make exporting
// from a DLL simpler. All files within this DLL are compiled with the FBXWRAPPERDLL_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see
// FBXWRAPPERDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#pragma once

#include <fbxsdk.h>
#include <DirectXMath.h>
#include <d3d.h>
#include <Vector>
#include <memory>
#include <oaidl.h>

#include "../Logging/Logging.h"
#include "../DataStructures/VertextStructs.h"
#include "../Helpers/FBXHelperFileUtil.h"
#include "../Helpers/Gemotryhelp/FBXNodeSearcher.h"

//FBXWRAPPERDLL_API
namespace wrapdll
{
	class FBXSCeneContainer
	{
	public:
		bool CreateSceneFromDiskFile(const std::string& pzath);
		void CreateEmptyScene(const std::string& name);


		//bool LoadSceneFromFBXFile(const std::string& path);

		char* GetSkeletonNameFromSceneNodes()
		{
			return (char*)FBXNodeSearcher::FetchSkeletonNameFromScene(m_poFbxScene).c_str();
		}

		virtual ~FBXSCeneContainer()
		{
			_log_action("CHECK: FBXSceneContainer Destroyed");
			auto DEBUG_BREAK = 1; // TODO: remove 
		};

		static FBXSCeneContainer* CreateContainer()
		{
			return new FBXSCeneContainer();
		}

		void readUnitsAndGeometry();

		void AddBoneInfo(char* boneName)
		{						
			m_animFileBoneNames.push_back(boneName[i]);
			
		}

		void GetVertices(int meshindex, PackedCommonVertex** ppVertices, int* itemCount)
		{
			*itemCount = static_cast<int>(m_packedMeshes[meshindex].vertices.size());
			*ppVertices = m_packedMeshes[meshindex].vertices.data();
		};

		void GetIndices(int meshindex, uint16_t** ppVertices, int* itemCount)
		{

			*itemCount = static_cast<int>(m_packedMeshes[meshindex].indices.size());
			*ppVertices = m_packedMeshes[meshindex].indices.data();
		};


		std::vector <PackedMesh>& GetMeshes()
		{
			return m_packedMeshes;
		}

	private:
		std::vector<PackedMesh> m_packedMeshes;
		std::vector<std::string> m_animFileBoneNames; // ordered as the .ANIM file, so can be used for bonename -> index lookups
		fbxsdk::FbxScene* m_poFbxScene;
	};

}


FBXWRAPPERDLL_API_EXT
wrapdll::FBXSCeneContainer* CreateFBXContainer()
{
	auto ret = new wrapdll::FBXSCeneContainer;
	return ret;
};

FBXWRAPPERDLL_API_EXT
void DeleteFBXCScenLoadser(wrapdll::FBXSCeneContainer* pInstance)
{
	delete pInstance;
};

FBXWRAPPERDLL_API_EXT
void AddBoneInfo(wrapdll::FBXSCeneContainer* pInstance, char* boneName)
{
	pInstance->AddBoneInfo(boneName);
};

FBXWRAPPERDLL_API_EXT
void CreateSceneFBX(wrapdll::FBXSCeneContainer* pInstance, char* path)
{
	pInstance->CreateSceneFromDiskFile(path);
};

FBXWRAPPERDLL_API_EXT
void GetPackedVertices(wrapdll::FBXSCeneContainer* pInstance, int meshindex, PackedCommonVertex** ppVertices, int* itemCount)
{
	pInstance->GetVertices(meshindex, ppVertices, itemCount);
};

FBXWRAPPERDLL_API_EXT
void GetIndices(wrapdll::FBXSCeneContainer* pInstance, int meshindex, uint16_t** ppVertices, int* itemCount)
{
	pInstance->GetIndices(meshindex, ppVertices, itemCount);
};

FBXWRAPPERDLL_API_EXT
char* GetName(wrapdll::FBXSCeneContainer* pInstance, int meshindex)
{
	return (char*)pInstance->GetMeshes()[meshindex].meshName.c_str();
};

FBXWRAPPERDLL_API_EXT
char* GetSkeletonNameFromSceneNodes(wrapdll::FBXSCeneContainer* pInstance, int meshindex)
{
	return (char*)pInstance->GetSkeletonNameFromSceneNodes();
};

FBXWRAPPERDLL_API_EXT
int GetMeshCount(wrapdll::FBXSCeneContainer* pInstance)
{
	return static_cast<int>(pInstance->GetMeshes().size());
};

