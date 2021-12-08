﻿using CommonControls.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonControls.FileTypes.RigidModel.MaterialHeaders
{
    public class MaterialFactory
    {
        ILogger _logger = Logging.Create<MaterialFactory>();
        Dictionary<ModelMaterialEnum, IMaterialCreator> _materialCreators = new Dictionary<ModelMaterialEnum, IMaterialCreator>();

        public static MaterialFactory Create() => new MaterialFactory();

        public MaterialFactory()
        {
            // Get all the weighted materials. Different enums, but same header
            var weightedEnums = ModelMaterialEnumHelper.GetAllWeightedMaterials();
            foreach(var enumValue in weightedEnums)
                _materialCreators[enumValue] = new WeighterMaterialCreator();

            _materialCreators[ModelMaterialEnum.TerrainTiles] = new TerrainTileMaterialCreator();
            _materialCreators[ModelMaterialEnum.custom_terrain] = new CustomTerrainMaterialCreator();
        }

        public IMaterial LoadMaterial(byte[] data, int offset, RmvVersionEnum rmvType, ModelMaterialEnum modelTypeEnum, long expectedMaterialSize)
        {
            if (_materialCreators.ContainsKey(modelTypeEnum))
            {
                var material = _materialCreators[modelTypeEnum].Create(modelTypeEnum, rmvType, data, offset);
                var actualMaterialSize = material.ComputeSize();

                if (actualMaterialSize != expectedMaterialSize)
                    throw new Exception($"Part of material {modelTypeEnum} header not read");

                return material;
            }
            else
            {
                _logger.Here().Error("Could not load {modelTypeEnum} material, atttempting to load using default");
                var material = _materialCreators[ModelMaterialEnum.default_type].Create(modelTypeEnum, rmvType, data, offset);
                var actualMaterialSize = material.ComputeSize();

                if (actualMaterialSize != expectedMaterialSize)
                    throw new Exception($"Uknown material - {modelTypeEnum} header not read. Expected Size = {expectedMaterialSize} Actual Size = {actualMaterialSize}");

                return material;
            }

            throw new Exception($"Uknown material - {modelTypeEnum} Material Size = {expectedMaterialSize}");
        }

        public byte[] Save(ModelMaterialEnum modelTypeEnum, IMaterial material)
        {
            return _materialCreators[modelTypeEnum].Save(material);
        }

        public List<ModelMaterialEnum> GetSupportedMaterials() => _materialCreators.Keys.Select(x => x).ToList();
    }
}