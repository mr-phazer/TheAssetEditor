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

#include "Logging/Logging.h"
#include "DataStructures/structs.h"

FBXWRAPPERDLL_API
class FBXSCeneContainer
{
public:
	
	bool CreateSceneFromDiskFile(const std::string& pzath);
	void CreateEmptyScene(const std::string& name);

	virtual ~FBXSCeneContainer()
	{
		_log_action("CHECK: FBXSceneContainer Destroyed");
		auto DEBUG_BREAK = 1; // TODO: remove 
	};
	
	
	FBXSCeneContainer* CreateContainer()
	{
		auto ret = new FBXSCeneContainer;
		return ret;
	};


	bool LoadSceneFile(const std::string& fileName);
	
	void readUnitsAndGeometry();


	void GetVertices(int meshindex, PackedCommonVertex** ppVertices, int* itemCount)	{

		*itemCount = m_meshes[meshindex].vertices.size();
		*ppVertices = m_meshes[meshindex].vertices.data();
	};

	void GetIndices(int meshindex, uint16_t** ppVertices, int* itemCount)	{

		*itemCount = m_meshes[meshindex].indices.size();
		*ppVertices = m_meshes[meshindex].indices.data();
	};
	

//private:
	std::vector<PackedMesh> m_meshes;
	fbxsdk::FbxScene* m_fbxScene;
};

FBXWRAPPERDLL_API_EXT
FBXSCeneContainer* CreateFBXContainer()
{
	auto ret = new FBXSCeneContainer;
	return ret;
};

FBXWRAPPERDLL_API_EXT
void CreateSceneFBX(FBXSCeneContainer* pInstance, char* path)
{
	pInstance->CreateSceneFromDiskFile(path);
};

FBXWRAPPERDLL_API_EXT 
void GetPackedVertices(FBXSCeneContainer* pInstance, int meshindex, PackedCommonVertex** ppVertices, int* itemCount)
{
	pInstance->GetVertices(meshindex, ppVertices, itemCount);
};

FBXWRAPPERDLL_API_EXT 
void GetIndices(FBXSCeneContainer* pInstance, int meshindex, uint16_t** ppVertices, int* itemCount)
{
	pInstance->GetIndices(meshindex, ppVertices, itemCount);
};

FBXWRAPPERDLL_API_EXT 
char* GetName(FBXSCeneContainer* pInstance, int meshindex)
{
	return (char*)pInstance->m_meshes[meshindex].meshName.c_str();
};

FBXWRAPPERDLL_API_EXT 
int GetMeshCount(FBXSCeneContainer* pInstance)
{
	return pInstance->m_meshes.size();
};

