#include "FBXNodeGeometryService.h"
#include <vector>
#include <map>

using namespace wrapdll;

FbxVector4 FBXNodeGeometryService::multT2(FbxNode* node, FbxVector4 vector) {
	FbxAMatrix matrixGeo;
	matrixGeo.SetIdentity();
	if (node->GetNodeAttribute())
	{
		const FbxVector4 lT = node->GetGeometricTranslation(FbxNode::eSourcePivot);
		const FbxVector4 lR = node->GetGeometricRotation(FbxNode::eSourcePivot);
		const FbxVector4 lS = node->GetGeometricScaling(FbxNode::eSourcePivot);
		matrixGeo.SetT(lT);
		matrixGeo.SetR(lR);
		matrixGeo.SetS(lS);
	}
	FbxAMatrix globalMatrix = node->EvaluateGlobalTransform();

	FbxAMatrix matrix = globalMatrix * matrixGeo;
	FbxVector4 result = matrix.MultT(vector);
	return result;
}

FbxVector4 FBXNodeGeometryService::multT_normal2(FbxNode* node, FbxVector4 vector) {
	FbxAMatrix matrixGeo;
	matrixGeo.SetIdentity();
	if (node->GetNodeAttribute())
	{
		//const FbxVector4 lT = node->GetGeometricTranslation(FbxNode::eSourcePivot);
		const FbxVector4 lR = node->GetGeometricRotation(FbxNode::eSourcePivot);
		//	const FbxVector4 lS = node->GetGeometricScaling(FbxNode::eSourcePivot);
			//matrixGeo.SetT(lT);
		matrixGeo.SetROnly(lR);
		//matrixGeo.SetS(lS);
	}
	FbxAMatrix globalMatrix;
	globalMatrix.SetROnly(node->EvaluateGlobalTransform().GetROnly());

	FbxAMatrix matrix = matrixGeo;
	FbxVector4 result = matrix.MultT(vector);
	return result;
}

FbxAMatrix FBXNodeGeometryService::GetNodeGeometryTransform(FbxNode* pNode)
{
	FbxAMatrix matrixGeo;
	matrixGeo.SetIdentity();

	if (pNode->GetNodeAttribute())
	{
		const FbxVector4 lT = pNode->GetGeometricTranslation(FbxNode::eSourcePivot);
		const FbxVector4 lR = pNode->GetGeometricRotation(FbxNode::eSourcePivot);
		const FbxVector4 lS = pNode->GetGeometricScaling(FbxNode::eSourcePivot);

		matrixGeo.SetT(lT);
		matrixGeo.SetR(lR);
		matrixGeo.SetS(lS);
	}

	return matrixGeo;
}

FbxAMatrix FBXNodeGeometryService::GetNodeWorldTransform(FbxNode* pNode)
{
	FbxAMatrix matrixL2W;
	matrixL2W.SetIdentity();

	if (NULL == pNode)
	{
		return matrixL2W;
	}

	matrixL2W = pNode->EvaluateGlobalTransform();

	FbxAMatrix matrixGeo = GetNodeGeometryTransform(pNode);

	matrixL2W *= matrixGeo;

	// todo remove debugging code
	//matrixL2W.SetIdentity();

	return matrixL2W;
}

FbxAMatrix FBXNodeGeometryService::GetNodeWorldTransform_Normals(FbxNode* pNode)
{
	FbxAMatrix matrixL2W;
	matrixL2W.SetIdentity();

	if (NULL == pNode)
	{
		return matrixL2W;
	}
	matrixL2W.SetQOnly(GetNodeWorldTransform(pNode).GetQ());

	return matrixL2W;
}



/*if (load_obj)
			{
				index_count = loader.LoadedIndices.size();
				//index_count = 6; // debug code
				m_vecGroupHeader[group_counter].uiIndexCount = index_count;
				_vecIndices.resize(index_count);
				for (size_t i = 0; i < index_count; i++)
				{
					_vecIndices[i] = loader.LoadedIndices[i];
					/*_vecIndices[i*3] = loader.LoadedIndices[i*3];
					_vecIndices[i*3+2] = loader.LoadedIndices[i*3+1];
					_vecIndices[i*3+1] = loader.LoadedIndices[i*3+2];
				}
			}*/