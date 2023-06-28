#pragma once

#include <DirectXMath.h>
#include <vector>
#include <string>

#include "..\DLLDefines.h"

struct BoneAnimKey
{
	DirectX::XMFLOAT3 translation;
	DirectX::XMFLOAT4 quaternion;
	double timeStampe = 0.0;
};

struct VertexInfluence
{
	uint32_t boneIndex = 0; 
	float weight = 0.0f;
};

struct PackedCommonVertex
{
	DirectX::XMFLOAT4 position = { 0, 0, 0, 0 };
	DirectX::XMFLOAT3 normal = { 0, 0, 0 };
	DirectX::XMFLOAT3 bitangent = { 0, 0, 0 };
	DirectX::XMFLOAT3 tangent = { 0, 0, 0 };
	DirectX::XMFLOAT2 uv = { 0, 0 };
	DirectX::XMFLOAT4 color = { 1, 0, 0, 1 };
	VertexInfluence influences[4];
	int weightCount = 0;
};

struct PackedMesh
{
	std::string meshName = "Unnamed_Mesh";
	std::vector<PackedCommonVertex> vertices;
	std::vector<uint16_t> indices;
};



