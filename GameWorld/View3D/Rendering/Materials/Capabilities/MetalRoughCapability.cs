﻿using GameWorld.Core.Rendering.Materials.Capabilities.Utility;
using GameWorld.Core.Rendering.Materials.Serialization;
using GameWorld.Core.WpfWindow.ResourceHandling;
using Microsoft.Xna.Framework.Graphics;
using Shared.GameFormats.RigidModel;
using Shared.GameFormats.RigidModel.MaterialHeaders;
using Shared.GameFormats.RigidModel.Types;
using Shared.GameFormats.WsModel;

namespace GameWorld.Core.Rendering.Materials.Capabilities
{
    public class MetalRoughCapability : MaterialBaseCapability
    {
        public TextureInput BaseColour { get; set; } = new TextureInput(TextureType.BaseColour);
        public TextureInput MaterialMap { get; set; } = new TextureInput(TextureType.MaterialMap);
        public TextureInput NormalMap { get; set; } = new TextureInput(TextureType.Normal);
        public TextureInput Mask { get; set; } = new TextureInput(TextureType.Mask);
        public TextureInput Distortion { get; set; } = new TextureInput(TextureType.Distortion);
        public TextureInput DistortionNoise { get; set; } = new TextureInput(TextureType.DistortionNoise);

        public override void Apply(Effect effect, ResourceLibrary resourceLibrary)
        {
            BaseColour.Apply(effect, resourceLibrary);
            MaterialMap.Apply(effect, resourceLibrary);
            NormalMap.Apply(effect, resourceLibrary);
            Mask.Apply(effect, resourceLibrary);

            base.Apply(effect, resourceLibrary);
            //Distortion.Apply(effect, resourceLibrary);
            //DistortionNoise.Apply(effect, resourceLibrary);
        }

        public override ICapability Clone()
        {
            return new MetalRoughCapability()
            {
                ScaleMult = ScaleMult,
                UseAlpha = UseAlpha,
                BaseColour = BaseColour.Clone(),
                MaterialMap = MaterialMap.Clone(),
                NormalMap = NormalMap.Clone(),
                Mask = Mask.Clone(),
                Distortion = Distortion.Clone(),
                DistortionNoise = DistortionNoise.Clone(),
            };
        }

        public override void Initialize(WsModelMaterialFile? wsModelMaterial, IRmvMaterial rmvMaterial)
        {
            CapabilityHelper.SetTextureFromModel(rmvMaterial, wsModelMaterial, BaseColour);
            CapabilityHelper.SetTextureFromModel(rmvMaterial, wsModelMaterial, MaterialMap);
            CapabilityHelper.SetTextureFromModel(rmvMaterial, wsModelMaterial, NormalMap);
            CapabilityHelper.SetTextureFromModel(rmvMaterial, wsModelMaterial, Mask);
            CapabilityHelper.SetTextureFromModel(rmvMaterial, wsModelMaterial, Distortion);
            CapabilityHelper.SetTextureFromModel(rmvMaterial, wsModelMaterial, DistortionNoise);

            base.Initialize(wsModelMaterial, rmvMaterial);
        }

        public override void SerializeToWsModel(WsMaterialTemplateEditor templateHandler)
        {
            templateHandler.AddAttribute("TEMPLATE_ATTR_BASE_COLOUR_PATH", BaseColour);
            templateHandler.AddAttribute("TEMPLATE_ATTR_MASK_PATH", Mask);
            templateHandler.AddAttribute("TEMPLATE_ATTR_MATERIAL_MAP", MaterialMap);
            templateHandler.AddAttribute("TEMPLATE_ATTR_NORMAL_PATH", NormalMap);
            templateHandler.AddAttribute("TEMPLATE_ATTR_DISTORTION_PATH", Distortion);
            templateHandler.AddAttribute("TEMPLATE_ATTR_DISTORTIONNOISE_PATH", DistortionNoise);

            base.SerializeToWsModel(templateHandler);
        }

        public override void SerializeToRmvMaterial(IRmvMaterial rmvMaterial) 
        {
            rmvMaterial.SetTexture(BaseColour.Type, BaseColour.TexturePath);
            rmvMaterial.SetTexture(MaterialMap.Type, MaterialMap.TexturePath);
            rmvMaterial.SetTexture(NormalMap.Type, NormalMap.TexturePath);
            rmvMaterial.SetTexture(Mask.Type, Mask.TexturePath);

            base.SerializeToRmvMaterial(rmvMaterial);
        }


        public static bool AreEqual(MetalRoughCapability a, MetalRoughCapability b)
        {
            if (a.UseAlpha != b.UseAlpha)
                return false;

            string[] aTextures = [
                a.BaseColour.TexturePath,
                a.MaterialMap.TexturePath,
                a.NormalMap.TexturePath,
                a.Mask.TexturePath
            ];

            string[] bTextures = [
                b.BaseColour.TexturePath,
                b.MaterialMap.TexturePath,
                b.NormalMap.TexturePath,
                b.Mask.TexturePath
            ];

            for (var i = 0; i < aTextures.Length; i++)
            {
                if (aTextures[i] != bTextures[i])
                    return false;
            }

            return true;
        }
    }
}