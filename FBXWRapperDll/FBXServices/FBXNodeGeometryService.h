#pragma once
#include <fbxsdk.h>
#include <vector>
#include <map>

class FBXNodeGeometryService
{
public:
	static fbxsdk::FbxAMatrix GetNodeGeometryTransform(fbxsdk::FbxNode* pNode);
	static fbxsdk::FbxAMatrix GetNodeWorldTransform(fbxsdk::FbxNode* pNode);
	static fbxsdk::FbxAMatrix GetNodeWorldTransform_Normals(fbxsdk::FbxNode* pNode);

private:
	// TODO: are these needed  ??
	static fbxsdk::FbxVector4 multT2(FbxNode* node, FbxVector4 vector);
	static fbxsdk::FbxVector4 multT_normal2(FbxNode* node, FbxVector4 vector);

};





