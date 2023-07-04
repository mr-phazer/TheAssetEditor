#pragma once

#include <fbxsdk.h>

#include <vector>
#include <map>

#include "..\DataStructures\VertextStructs.h"
#include "..\Helpers\FBXUniiHelper.h"
#include "..\Logging\Logging.h"
#include "..\FBXServices\FBXNodeGeometryService.h"
#include "..\FBXServices\FBXMeshGeometryService.h"


#include "..\Helpers\SimpleMath\SimpleMath.h"
#include "..\Helpers\FormatConverter.h"

namespace wrapdll
{
	class FbxMeshProcessor
	{
	public:
		static PackedCommonVertex MakePackedVertex(
			const FbxVector4& vControlPoint,
			const FbxVector4& vNormalVector,
			const FbxVector2& UVmap1,
			const ControlPointInfluences* ctrlPointInfluences,
			double m_factor_to_meters)
		{
			PackedCommonVertex destVertexRef;

			destVertexRef.position.x = static_cast<float>(vControlPoint.mData[0] * m_factor_to_meters);
			destVertexRef.position.y = static_cast<float>(vControlPoint.mData[1] * m_factor_to_meters);
			destVertexRef.position.z = static_cast<float>(vControlPoint.mData[2] * m_factor_to_meters);

			destVertexRef.normal.x = static_cast<float>(vNormalVector.mData[0]);
			destVertexRef.normal.y = static_cast<float>(vNormalVector.mData[1]);
			destVertexRef.normal.z = static_cast<float>(vNormalVector.mData[2]);

			destVertexRef.uv.x = static_cast<float>(UVmap1.mData[0]);
			destVertexRef.uv.y = static_cast <float>(UVmap1.mData[1]);

			if (ctrlPointInfluences)
			{
				destVertexRef.weightCount = ctrlPointInfluences->weightCount;

				for (size_t i = 0; i < destVertexRef.weightCount; i++)
				{
					destVertexRef.influences[i].boneIndex = ctrlPointInfluences->influences[i].boneIndex;
					destVertexRef.influences[i].weight = ctrlPointInfluences->influences[i].weight;
				}
			}
			else
				destVertexRef.weightCount = 0;


			return destVertexRef;
		};

		static bool MakeUnindexPackedMesh(
			fbxsdk::FbxScene* poFbxScene,
			fbxsdk::FbxMesh* poFbxMesh,
			PackedMesh& destMesh,
			std::vector<ControlPointInfluences> controlPointerInfluences)
		{
			auto node = poFbxMesh->GetNode();
			auto pMeshName = node->GetName();

			auto fmGloabalTransForm = FBXNodeGeometryService::GetNodeWorldTransform(poFbxMesh->GetNode());
			auto fmGloabalTransForm_Normals = FBXNodeGeometryService::GetNodeWorldTransform_Normals(poFbxMesh->GetNode());
						
			// polygon info, count + indexes to each "corner"
			int polygonVertexCount = poFbxMesh->GetPolygonVertexCount();

			// polygoncount = triangle_count, as model is triangulated
			int triangleCount = poFbxMesh->GetPolygonCount();

			// array of vertex points in 3d spack
			FbxVector4* pControlPoints = poFbxMesh->GetControlPoints();
			int C = poFbxMesh->GetControlPointsCount();

			// mesh indicies
			int* pPolyggonVertices = poFbxMesh->GetPolygonVertices();

			const int faceIndexCount = 3; // face = triangle harcoded (file is triangulated on FBX init)

			destMesh.indices.resize(triangleCount * faceIndexCount);
			destMesh.vertices.resize(triangleCount * faceIndexCount);

			poFbxMesh->GenerateTangentsDataForAllUVSets(); // not "pretty" once converted to "packed vertex"

			FbxGeometryElementNormal::EMappingMode NormalMappingMode;
			//FbxGeometryElementNormal::EMappingMode TangentMappingMode;
			//FbxGeometryElementNormal::EMappingMode BinormalMappingModing;

			auto vecNormalsTable = FBXMeshGeometryService::GetNormals(poFbxMesh, &NormalMappingMode);

			if (vecNormalsTable.empty())
				return _log_action_error("No normal vectors found!")

			auto textureUVMaps = FBXMeshGeometryService::LoadUVInformation(poFbxMesh);

			if (textureUVMaps.size() == 0)
			{
				return _log_action_error("No UV Maps founds!");
			}

			// obtain the standar scaling factor
			auto factorToMeters = MeshProcessingHelper::GetFactorToMeters(poFbxScene);

			for (int faceIndex = 0; faceIndex < triangleCount; faceIndex++)
			{
				for (int faceCorner = 0; faceCorner < 3; faceCorner++)
				{
					auto vertexIndex = (faceIndex * 3) + faceCorner; // TODO: check if this can replace "vertex_index", and if so decide if it SHOULD
					auto& destVertexRef = destMesh.vertices[vertexIndex];

					// -- FXB position ("pure" verrtex), and built-in transforms
					int controlPointIndex = poFbxMesh->GetPolygonVertex(faceIndex, faceCorner);

					FbxVector4 v4ControlPoint = GetPositionTransformedFBXX(pControlPoints, controlPointIndex, fmGloabalTransForm);
					auto vNormalVector = GetTransformedFBXNormal(NormalMappingMode, controlPointIndex, vertexIndex, vecNormalsTable, fmGloabalTransForm_Normals);

					bool mapped = false;
					FbxVector2 uvMap1 = { 0.0, 0.0 };
					FbxVector2 uvMap2 = { 0.0, 0.0 };

					if (textureUVMaps.size() == 0)
					{
						return _log_action_error("No UV Maps founds!");
					}

					/********************************************************************************
						UV MAPS
					*********************************************************************************/
					// TODO: make it possible for the user to pick what UV map to use, maybe
					if (textureUVMaps.size() > 0)
						uvMap1 = (textureUVMaps.begin())->second[vertexIndex]; // at least 1 uv map found

					if (textureUVMaps.size() == 2) // 2 uvmaps found, which is the max supported in RMV2, so stop a 2 uvmap
						uvMap1 = std::next(textureUVMaps.begin(), 1)->second[vertexIndex];

					
					ControlPointInfluences* pConctolPointInfluences = nullptr;
					if (controlPointerInfluences.size() == C)
					{
						pConctolPointInfluences = &controlPointerInfluences[controlPointIndex];
					}

					destMesh.vertices[vertexIndex] = MakePackedVertex(v4ControlPoint, vNormalVector, uvMap1, pConctolPointInfluences, factorToMeters);
					destMesh.indices[vertexIndex] = static_cast<uint16_t>(vertexIndex);

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

		

	public:
		static void doTangentAndIndexing(PackedMesh& destMesh) {

			using namespace std;
			using namespace DirectX;

			vector<SimpleMath::Vector3> vertices;
			vector<SimpleMath::Vector2> uvs;
			vector<SimpleMath::Vector3> normals;
			vector<SimpleMath::Vector3> tangents;
			vector<SimpleMath::Vector3> bitangents;

			vector<XMFLOAT4> bone_weights;
			vector<XMUINT4> bone_indices;

			vector<SimpleMath::Vector3> out_vertices;
			vector<SimpleMath::Vector2> out_uvs;
			vector<SimpleMath::Vector3> out_normals;
			vector<SimpleMath::Vector3> out_tangents;
			vector<SimpleMath::Vector3> out_bitangents;

			vector<DirectX::XMFLOAT4> out_bone_weights;
			vector<DirectX::XMUINT4> out_bone_indices;

			vector<uint16_t> out_indices;

			// fill the UN-INDEXED vertex data into vectors
			for (auto& v : destMesh.vertices)
			{
				//vertices.push_back(v.position);
				vertices.push_back({ v.position.x, v.position.y,v.position.z });
				uvs.push_back(v.uv);
				normals.push_back(v.normal);
				bone_indices.push_back({ v.influences[0].boneIndex, v.influences[1].boneIndex, v.influences[2].boneIndex, v.influences[3].boneIndex });
				bone_weights.push_back({ v.influences[0].weight, v.influences[1].weight, v.influences[2].weight, v.influences[3].weight });

			};


			computeTangentBasis_Unindexed(
				// inputs
				vertices, uvs, normals,

				// outputs	
				tangents, bitangents
			);


			// do indexing (clean up mesh), and average tangents
			indexVBO_TBN_Fast(
				vertices, uvs, normals, tangents, bitangents, bone_weights, bone_indices,
				/*remap, */
				out_indices,
				out_vertices,
				out_uvs,
				out_normals,
				out_tangents,
				out_bitangents,

				out_bone_weights,
				out_bone_indices
			);

			// copy the INDEXED vertex data (with new tangents) back into the mesh bloack vector<ModelVertex>
			destMesh.vertices.clear();
			destMesh.vertices.resize(out_vertices.size());

			for (size_t i = 0; i < out_vertices.size(); i++)
			{
				auto& v = destMesh.vertices[i];
				auto& v_src = destMesh.vertices[i];
				//v.position = out_vertices[i];
				v.position = XMFLOAT4(out_vertices[i].x, out_vertices[i].y, out_vertices[i].z, 0);

				out_normals[i].Normalize();
				v.normal = out_normals[i];

				out_tangents[i].Normalize();
				out_bitangents[i].Normalize();

				v.tangent = out_tangents[i];
				v.bitangent = out_bitangents[i];

				v.uv = out_uvs[i];

				v.influences[0].boneIndex = out_bone_indices[i].x;
				v.influences[1].boneIndex = out_bone_indices[i].y;
				v.influences[2].boneIndex = out_bone_indices[i].z;
				v.influences[3].boneIndex = out_bone_indices[i].w;

				v.influences[0].weight = out_bone_weights[i].x;
				v.influences[1].weight = out_bone_weights[i].y;
				v.influences[2].weight = out_bone_weights[i].z;
				v.influences[3].weight = out_bone_weights[i].w;

			}

			destMesh.indices = out_indices;

			/*return;

			oMeshData.oUnpackedMesh.vecIndices.resize(oMeshData.oMeshUnprocessed32.vecIndices.size());
			for (size_t i = 0; i < oMeshData.oMeshUnprocessed32.vecIndices.size(); i++)
			{
				oMeshData.oUnpackedMesh.vecIndices[i] = static_cast<uint16_t>(oMeshData.oMeshUnprocessed32.vecIndices[i]);
			}

			*/
		}

		static void computeTangentBasis_Unindexed(
			// inputs
			const std::vector<DirectX::SimpleMath::Vector3>& vertices,
			const std::vector<DirectX::SimpleMath::Vector2>& uvs,
			const std::vector<DirectX::SimpleMath::Vector3>& normals,
			// outputs
			std::vector<DirectX::SimpleMath::Vector3>& tangents,
			std::vector<DirectX::SimpleMath::Vector3>& bitangents
		) {
			for (unsigned int i = 0; i < vertices.size(); i += 3) {
				// Shortcuts for vertices
				const DirectX::SimpleMath::Vector3& v0 = vertices[i + 0];
				const DirectX::SimpleMath::Vector3& v1 = vertices[i + 1];
				const DirectX::SimpleMath::Vector3& v2 = vertices[i + 2];

				// Shortcuts for UVs
				const DirectX::SimpleMath::Vector2& uv0 = uvs[i + 0];
				const DirectX::SimpleMath::Vector2& uv1 = uvs[i + 1];
				const DirectX::SimpleMath::Vector2& uv2 = uvs[i + 2];

				// Edges of the triangle : postion delta
				DirectX::SimpleMath::Vector3 deltaPos1 = v1 - v0;
				DirectX::SimpleMath::Vector3 deltaPos2 = v2 - v0;

				// UV delta
				DirectX::SimpleMath::Vector2 deltaUV1 = uv1 - uv0;
				DirectX::SimpleMath::Vector2 deltaUV2 = uv2 - uv0;

				float r = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);
				DirectX::SimpleMath::Vector3 tangent = (deltaPos1 * deltaUV2.y - deltaPos2 * deltaUV1.y) * r;
				DirectX::SimpleMath::Vector3 bitangent = (deltaPos2 * deltaUV1.x - deltaPos1 * deltaUV2.x) * r;

				// Set the same tangent for all three vertices of the triangle.
				// They will be merged later, in vboindexer.cpp
				tangents.push_back(tangent);
				tangents.push_back(tangent);
				tangents.push_back(tangent);

				// Same thing for binormals
				bitangents.push_back(bitangent);
				bitangents.push_back(bitangent);
				bitangents.push_back(bitangent);
			}

			return;
		};


		static inline void indexVBO_TBN_Fast(
			const std::vector<DirectX::SimpleMath::Vector3>& in_vertices,
			const std::vector<DirectX::SimpleMath::Vector2>& in_uvs,
			const std::vector<DirectX::SimpleMath::Vector3>& in_normals,
			const std::vector<DirectX::SimpleMath::Vector3>& in_tangents,
			const std::vector<DirectX::SimpleMath::Vector3>& in_bitangents,

			const std::vector<DirectX::XMFLOAT4>& in_bone_weights,
			const std::vector<DirectX::XMUINT4>& in_bone_indices,

			//std::vector<uint16_t>& out_vertex_remap,
			std::vector<uint16_t>& out_indices,
			std::vector<DirectX::SimpleMath::Vector3>& out_vertices,
			std::vector<DirectX::SimpleMath::Vector2>& out_uvs,
			std::vector<DirectX::SimpleMath::Vector3>& out_normals,
			std::vector<DirectX::SimpleMath::Vector3>& out_tangents,
			std::vector<DirectX::SimpleMath::Vector3>& out_bitangents,

			std::vector<DirectX::XMFLOAT4>& out_bone_weights,
			std::vector<DirectX::XMUINT4>& out_bone_indices

		) {
			//std::map<PackedCommonVertex, unsigned short> VertexToOutIndex;

			std::vector<int> avg_count/*(in_vertices.size(), 1)*/;
			// For each input vertex
			for (unsigned int i = 0; i < in_vertices.size(); i++) {
				PackedCommonVertex packed;
				packed.position = convert::ConvertToVec3(in_vertices[i]);
				packed.uv = in_uvs[i];
				packed.normal = in_normals[i];

				// Try to find a similar vertex in out_XXXX
				uint16_t index;

				//bool found = getSimilarVertexIndex(packed, VertexToOutIndex, index);
				bool found = getSimilarVertexIndex(in_vertices[i], in_uvs[i], in_normals[i], out_vertices, out_uvs, out_normals, index);

				if (found) { // A similar vertex is already in the VBO, use it instead !
					out_indices.push_back(index);

					// Average the tangents and the bitangents
					out_tangents[index] += in_tangents[i];
					out_bitangents[index] += in_bitangents[i];

					//avg_count[index]++;
				}
				else { // If not, it needs to be added in the output data.
					out_vertices.push_back(in_vertices[i]);
					out_uvs.push_back(in_uvs[i]);
					out_normals.push_back(in_normals[i]);
					out_tangents.push_back(in_tangents[i]);
					out_bitangents.push_back(in_bitangents[i]);

					out_bone_weights.push_back(in_bone_weights[i]);
					out_bone_indices.push_back(in_bone_indices[i]);

					uint16_t newindex = (uint16_t)out_vertices.size() - 1;

					out_indices.push_back(newindex);
					//VertexToOutIndex[packed] = newindex;

					//avg_count.push_back(1);
					// the index in the INPUT, for remapping
					//out_vertex_remap.push_back(i);
				}
			}
		}

		static inline bool is_near(float v1, float v2) {
			return fabs(v1 - v2) < 0.00001f;
		}

		// Searches through all already-exported vertices
		// for a similar one.
		// Similar = same position + same UVs + same normal
		static inline bool getSimilarVertexIndex(
			const DirectX::SimpleMath::Vector3& in_vertex,
			const DirectX::SimpleMath::Vector2& in_uv,
			const DirectX::SimpleMath::Vector3& in_normal,
			std::vector<DirectX::SimpleMath::Vector3>& out_vertices,
			std::vector<DirectX::SimpleMath::Vector2>& out_uvs,
			std::vector<DirectX::SimpleMath::Vector3>& out_normals,
			uint16_t& result
		) {
			// Lame linear search
			for (unsigned int i = 0; i < out_vertices.size(); i++) {
				if (
					is_near(in_vertex.x, out_vertices[i].x) &&
					is_near(in_vertex.y, out_vertices[i].y) &&
					is_near(in_vertex.z, out_vertices[i].z) &&
					is_near(in_uv.x, out_uvs[i].x) &&
					is_near(in_uv.y, out_uvs[i].y) &&
					is_near(in_normal.x, out_normals[i].x) &&
					is_near(in_normal.y, out_normals[i].y) &&
					is_near(in_normal.z, out_normals[i].z)
					) {
					result = i;
					return true;
				}
			}
			// No other vertex could be used instead.
			// Looks like we'll have to add it to the VBO.
			return false;
		}


	public:

		static FbxVector4 GetTransformedFBXNormal(FbxGeometryElementNormal::EMappingMode NormalMappingMode, int  controlPointIndex, int vertexIndex, const std::vector < fbxsdk::FbxVector4>& m_vecNormals, FbxAMatrix transform)
		{
			FbxVector4 vNormalVector = FbxVector4(0, 0, 0, 0);
			if (NormalMappingMode == FbxGeometryElementNormal::EMappingMode::eByControlPoint)
			{
				vNormalVector = m_vecNormals[controlPointIndex];
			}
			else if (NormalMappingMode == FbxGeometryElementNormal::EMappingMode::eByPolygonVertex)
			{
				vNormalVector = m_vecNormals[vertexIndex];
			}

			vNormalVector = (transform).MultT(vNormalVector);
			vNormalVector.Normalize();
			return vNormalVector;
		}


		static FbxVector4 GetPositionTransformedFBXX(FbxVector4* pControlPoints, int controlPointIndex, FbxAMatrix& transform)
		{
			FbxVector4 v4ControlPoint = pControlPoints[controlPointIndex];
			return transform.MultT(v4ControlPoint);
		};

	};
};