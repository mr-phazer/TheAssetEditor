#pragma once

#include <fbxsdk.h>
#include <vector>

#include "MeshProcessingHelper.h"
#include "..\Logging\Logging.h"
#include "..\FBXServices\FBXMeshGeomtryService.h"
#include "..\FBXServices\FBXNodeGeometryService.h"
#include "..\DataStructures\structs.h"



class FbxMeshProcessorService {

public:
	static bool MakeUnindexPackedMesh(fbxsdk::FbxScene* poFbxScene, fbxsdk::FbxMesh* poFbxMesh, PackedMesh& destMesh)
	{
		auto node = poFbxMesh->GetNode();
		auto pMeshName = node->GetName();


		auto fmGloabalTransForm = FBXNodeGeometryService::GetNodeWorldTransform(poFbxMesh->GetNode());
		auto fmGloabalTransForm_Normals = FBXNodeGeometryService::GetNodeWorldTransform_Normals(poFbxMesh->GetNode());

		// TODO: REMOVE THESE 2 LINES, DEBUGGIN...
		//fmGloabalTransForm.SetIdentity();
		//fmGloabalTransForm_Normals.SetIdentity();


		// polygon info, count + indexes to each "corner"
		int polygonVertexCount = poFbxMesh->GetPolygonVertexCount();

		// polygoncount = triangle_count, as model is triangulated
		int triangleCount = poFbxMesh->GetPolygonCount();

		// array of vertex points in 3d spack
		FbxVector4* pControlPoints = poFbxMesh->GetControlPoints();

		int controlPoinCount = poFbxMesh->GetControlPointsCount();

		// mesh indicies
		int* pPolyggonVertices = poFbxMesh->GetPolygonVertices();

		const int faceIndexCount = 3; // face = triangle harcoded (file is triangulated on FBX init)

		destMesh.indices.resize(triangleCount * faceIndexCount);
		destMesh.vertices.resize(triangleCount * faceIndexCount);

		poFbxMesh->GenerateTangentsDataForAllUVSets(); // not "pretty" once converted to "packed vertex"

		FbxGeometryElementNormal::EMappingMode NormalMappingMode;
		FbxGeometryElementNormal::EMappingMode TangentMappingMode;
		FbxGeometryElementNormal::EMappingMode BinormalMappingModing;

		auto m_vecNormals = FBXMeshGeometryService::GetNormals(poFbxMesh, &NormalMappingMode);
		auto m_vecTangents = FBXMeshGeometryService::GetTangents(poFbxMesh, &BinormalMappingModing);
		auto m_vecBitangents = FBXMeshGeometryService::GetBitangents(poFbxMesh, &TangentMappingMode);

		auto textureUVMaps = FBXMeshGeometryService::LoadUVInformation(poFbxMesh);

		if (textureUVMaps.size() == 0)
		{
			return _log_action_error("No UV Maps founds!");
		}

		// obtain the standar scaling factor
		auto m_factor_to_meters = MeshProcessingHelper::GetFactorToMeters(poFbxScene);

		size_t vertex_index = 0;
		for (size_t triangle_index = 0; triangle_index < triangleCount; triangle_index++)
		{
			auto test = triangle_index * 3;
			for (size_t edge = 0; edge < 3; edge++)
			{
				auto& destVertexRef = destMesh.vertices[vertex_index];

				int control_point_index = poFbxMesh->GetPolygonVertex(triangle_index, edge);
				FbxVector4 vControlPoint = pControlPoints[control_point_index];
				vControlPoint = fmGloabalTransForm.MultT(vControlPoint);

				/********************************************************************************
					POSITION
				**********************************************************************************/
				// TODO: apply transform?
				destVertexRef.position.x = vControlPoint.mData[0] * m_factor_to_meters/* * m_scale_factor*/;
				destVertexRef.position.y = vControlPoint.mData[1] * m_factor_to_meters/* * m_scale_factor*/;
				destVertexRef.position.z = vControlPoint.mData[2] * m_factor_to_meters/* * m_scale_factor*/;

				/********************************************************************************
					NORMALS
				*********************************************************************************/
				// transformation  + assign normals

				FbxVector4 vNormalVector = FbxVector4(0, 0, 0, 0);
				if (NormalMappingMode == FbxGeometryElementNormal::EMappingMode::eByControlPoint)
				{
					vNormalVector = m_vecNormals[control_point_index];;
				}
				else if (NormalMappingMode == FbxGeometryElementNormal::EMappingMode::eByPolygonVertex)
				{
					vNormalVector = m_vecNormals[vertex_index];;
				}

				//_poFbxMesh->GetPolygonVertexNormal(triangle_index, edge, vNormalVector);
				//vNormalVector =(vecMeshes[m_group].normal_transformation_matrix).MultT(vNormalVector);
				vNormalVector = (fmGloabalTransForm_Normals).MultT(vNormalVector);
				vNormalVector.Normalize();

				double sign = 1.0;

				//if (vecMeshes[m_group].m_bNegativeScaling)
				//sign = -1.0;

				destVertexRef.normal.x = vNormalVector.mData[0] * sign;
				destVertexRef.normal.y = vNormalVector.mData[1] * sign;
				destVertexRef.normal.z = vNormalVector.mData[2] * sign;

				/********************************************************************************
					TANGENTS
				*********************************************************************************/

				//#define READ_FBX_TANGENTS

#ifdef  READ_FBX_TANGENTS
				FbxVector4 vTangentVector;
				vTangentVector = m_vecTangents[vertex_index];
				vTangentVector = (fmGloabalTransForm_Normals).MultT(vTangentVector);
				vTangentVector.Normalize();

				sign = 1.0f;

				destVertex_.tangent.x = vTangentVector.mData[0];// * x;// *vGlobalScale[0];
				destVertex_.tangent.y = vTangentVector.mData[1];// * y;// *vGlobalScale[1];
				destVertex_.tangent.z = vTangentVector.mData[2];// * z;// *vGlobalScale[2];

				/********************************************************************************
					BITANGENTS
				*********************************************************************************/
				FbxVector4 vBitangentVector;
				vBitangentVector = m_vecBitangents[vertex_index];
				vBitangentVector = fmGloabalTransForm_Normals.MultT(vBitangentVector);
				vBitangentVector.Normalize();

				destVertex_.bitangent.x = vBitangentVector.mData[0];// * x;// *vGlobalScale[0];
				destVertex_.bitangent.y = vBitangentVector.mData[1];// * y;// *vGlobalScale[1];
				destVertex_.bitangent.z = vBitangentVector.mData[2];// * z;// *vGlobalScale[2];
#endif
				bool mapped = false;
				FbxVector2 UVmap1 = { 0.0, 0.0 };
				FbxVector2 UVmap2 = { 0.0, 0.0 };

				if (textureUVMaps.size() == 0)
				{
					return _log_action_error("No UV Maps founds!");
				}

				/********************************************************************************
					UV MAPS
				*********************************************************************************/
				// TODO: make it possible for the user to pick what UV map to use, maybe
				if (textureUVMaps.size() > 0)
					UVmap1 = (textureUVMaps.begin())->second[vertex_index]; // at least 1 uv map found

				if (textureUVMaps.size() == 2) // 2 uvmaps found, which is the max supported in RMV2, so stop a 2 uvmap
					UVmap2 = std::next(textureUVMaps.begin(), 1)->second[vertex_index];

				FbxVector2 v2UVPoly;
				poFbxMesh->GetPolygonVertexUV(triangle_index, edge, textureUVMaps.begin()->first.c_str(), v2UVPoly, mapped);

				float u = v2UVPoly[0];
				float v = v2UVPoly[1];

				destVertexRef.uv.x = UVmap1.mData[0];
				destVertexRef.uv.y = 1.0 - UVmap1.mData[1];

				// TODO: add support for 2 uv channels, as RMV2 "default vertex format" supports this, and it is used in some models
				/*destVertex_.uv2.x = UVmap2.mData[0];
				destVertex_.uv2.y = 1.0 - UVmap2.mData[1];*/

				/********************************************************************************
					Vertex Weighting / Skin
				*********************************************************************************/

				// --- reset the indices and weight to a default which will not deform, nor act weigrd
				/*destVertex_.influences[0] bone_indices.x = 0;
				destVertex_.bone_indices.y = 0;
				destVertex_.bone_indices.z = 0;
				destVertex_.bone_indices.w = 0;*/

				destVertexRef.influences[0].weight = 1.0f;

				/*destVertex_.bone_weights.y = 0.f;
				destVertex_.bone_weights.z = 0.f;
				destVertex_.bone_weights.w = 0.f;*/

				//if (!m_vecvecTempControlPointInfluences.empty())
				//{
				//	// --- get per-vertex, normalized weights, and indices:
				//	pair<XMUINT4, XMFLOAT4> riggingIndicesAndWeights =
				//		this->getVertexWeightingNormalized(m_vecvecTempControlPointInfluences[control_point_index], 4);

				//	destVertex_.bone_indices = riggingIndicesAndWeights.first;
				//	destVertex_.bone_weights = riggingIndicesAndWeights.second;
				//}
				/********************************************************************************
					Indices
				*********************************************************************************/
				// indices are just the vertex index, as the buffer is not yet optimized, nor indexed
				destMesh.indices[vertex_index] = vertex_index;

				vertex_index++;
			}
						
			destMesh.meshName = pMeshName;
		}

#if 0 // TODO: should this be here?

		// -- check error in rigging,
		for (auto& v : destMesh.vecVertices)
		{
			auto sum =
				v.bone_weights.x +
				v.bone_weights.y +
				v.bone_weights.z +
				v.bone_weights.w;

			if (sum < 0.0f) // sum < 0 is invalid
				return _log_action_error("Invalid: sum of weights of this is < 0.0!");

			if (sum == 0.0f) // sum == 0 is invalid
				return _log_action_error("Invalid: Null-weighting, sum of weights of vertex is 0.0!");

			// sum > 1.0 is invalid
			// (in this case the eventual 8 bit resolution is taken into account, so 1 resolution step over 1.0 is not considered an error
			if (sum > (1.0f + (1.f / 255.f)))
				return _log_action_error("Invalid: sum of weights of vertex is: " + to_string(sum));

			// sum < 1.0 is invalid
			// (in this case, again, the eventual 8 bit resolution is taken into account, so 1 resolution step under 1.0 is not considered an error
			if (sum < (1.0f - (1.f / 255.f)))
				return _log_action_error("Invalid: sum of weights of vertex is: " + to_string(sum));
		}

#endif // 0 // TODO: should this be here?

		return true;
	}

	static bool FindMeshesInScene(fbxsdk::FbxScene* poScene, std::vector<fbxsdk::FbxMesh*>& fbxMeshes)
	{
		if (!poScene)
			return false;
				
		auto poRootNode = poScene->GetRootNode();
		
		if (!poRootNode)
			return false;

		FindFbxMeshesRecursive(poRootNode, fbxMeshes);
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
	};

	

};