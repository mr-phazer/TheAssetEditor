#include "MeshProcessor.h"
#include "..\libs\meshopt\meshoptimizer.h"

//TODO: use meshopt, and add VertexRemapper variant of their code, that takes "CommonPackedVertex*"
// And processes the the tangents like in "DoMeshIndexingWithTangentSmoothing"
// Their hashing method is proper 10% faster, and it it MIT license 


// TODO: maybe also use this code, for meshes before Saving them as FBX?

void wrapdll::MeshProcessor::DoFinalMeshProcessing(PackedMesh& mesh)
{
    ComputeTangentBasisUnindexed(mesh.vertices);
    DoMeshIndexingWithTangentSmoothing(mesh);


    DoTangentBasisAndIndexing(mesh);
}

void wrapdll::MeshProcessor::DoTangentBasisAndIndexing(PackedMesh& destMesh)
{
    using namespace std;
    using namespace DirectX;
    
    
    ComputeTangentBasisForUnindexedMeshWithRemp(/*vertexRemap, */destMesh.vertices);   
    
    //size_t index_count = destMesh.vertices.size() * 3;
    //size_t unindexed_vertex_count = face_count * 3;

    //"TODO"
        //"TODO; , this is not reducing indexes meshes because, it is use the whole PackedCommonVertex"
        //"only use pos, normal, uv", "need to make temp struct()" "" and fille it upt

      //  struct SimpleVertex { sm::Vector3 pos, sm::Vector3 normal, sm::Vector2 uv };

    vector<uint32_t> vertexRemap(destMesh.vertices.size());
    auto newVertexCount = meshopt_generateVertexRemap(
        vertexRemap.data(),
        &destMesh.indices[0],
        destMesh.indices.size(),
        destMesh.vertices.data(),
        destMesh.vertices.size(),
        sizeof(PackedCommonVertex));


    std::vector<uint32_t> indexRemap(newVertexCount);
    meshopt_remapIndexBuffer(destMesh.indices.data(), NULL, destMesh.indices.size(), indexRemap.data());

    meshopt_remapVertexBuffer(destMesh.vertices.data(), destMesh.vertices.data(), destMesh.vertices.size(), sizeof(PackedCommonVertex), vertexRemap.data());



    // -- fill the mesh with the proceessed data
    
    for (auto& v : destMesh.vertices)
    {
        v.position.w = 0;

        
        v.normal = sm::Vector3::Normalize(v.normal);
        

        std::swap(v.tangent, v.bitangent);        

        v.tangent = sm::Vector3::Normalize(v.tangent);
        v.bitangent = sm::Vector3::Normalize(v.bitangent);
    }



    RemapVertexWeights<uint32_t, ~0u> (destMesh.vertexWeights, destMesh.vertexWeights, vertexRemap);

    //for (size_t i = 0; i < destMesh.vertices.size(); i++)
    //{
    //    auto& v = destMesh.vertices[i];
    //    auto& v_src = destMesh.vertices[i];
    //    v.position = XMFLOAT4(outVertices[i].x, outVertices[i].y, outVertices[i].z, 0);

    //    outNormals[i].Normalize();
    //    v.normal = outNormals[i];

    //    outTangents[i].Normalize();
    //    outBitangents[i].Normalize();

    //    v.tangent = outTangents[i];
    //    v.bitangent = outBitangents[i];

    //    v.uv = outUVs[i];
    //}

    //destMesh.indices = outIndices;
}

template <typename T, T discard_value>
void wrapdll::MeshProcessor::RemapVertexWeights(const std::vector<VertexWeight>& inVertexWeights, std::vector<VertexWeight>& outVertexWeights, const std::vector<T>& outVertexIndexRemap)
{
    for (size_t i = 0; i < inVertexWeights.size(); i++) // run through all old vertexweights
    {
        if (outVertexIndexRemap[inVertexWeights[i].vertexIndex] != discard_value)
        {
            auto tempVertexWeight = inVertexWeights[i];

            // remap the index, 
            tempVertexWeight.vertexIndex = outVertexIndexRemap[inVertexWeights[i].vertexIndex];

            outVertexWeights.push_back(tempVertexWeight);
        }
    }
}