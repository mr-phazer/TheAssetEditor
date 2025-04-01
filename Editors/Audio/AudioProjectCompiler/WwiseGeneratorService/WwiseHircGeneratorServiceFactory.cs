﻿using System;
using System.Collections.Generic;
using Editors.Audio.AudioEditor.AudioProjectData;
using Editors.Audio.AudioProjectCompiler.WwiseGeneratorService.WwiseGenerators.Hirc.V136;
using Shared.GameFormats.Wwise.Enums;
using Shared.GameFormats.Wwise.Hirc;

namespace Editors.Audio.AudioProjectCompiler.WwiseGeneratorService
{
    public class WwiseHircGeneratorServiceFactory
    {
        private readonly Dictionary<AkBkHircType, Func<IWwiseHircGeneratorService>> _hircGenerators = [];

        public void RegisterGenerator(AkBkHircType type, Func<IWwiseHircGeneratorService> creator)
        {
            _hircGenerators[type] = creator;
        }

        public IWwiseHircGeneratorService CreateInstance(AkBkHircType type)
        {
            if (_hircGenerators.TryGetValue(type, out var creator))
                return creator();

            return null;
        }

        public HircItem GenerateHirc(AudioProjectHircItem audioProjectHircItem, SoundBank soundBank)
        {
            var generator = CreateInstance(audioProjectHircItem.HircType);
            if (generator == null)
                throw new Exception($"No Hirc generator registered for type {audioProjectHircItem.HircType}");
            return generator.GenerateHirc(audioProjectHircItem, soundBank);
        }

        public static WwiseHircGeneratorServiceFactory CreateFactory(uint bankGeneratorVersion)
        {
            switch (bankGeneratorVersion)
            {
                case 2147483784: return CreateFactory_V136();
            }

            throw new Exception($"Unknown Bank Generator Version: {bankGeneratorVersion}");
        }

        private static WwiseHircGeneratorServiceFactory CreateFactory_V136()
        {
            var instance = new WwiseHircGeneratorServiceFactory();
            instance.RegisterGenerator(AkBkHircType.Sound, () => new SoundHircGenerator_V136());
            instance.RegisterGenerator(AkBkHircType.RandomSequenceContainer, () => new RanSeqCntrHircGenerator_V136());
            instance.RegisterGenerator(AkBkHircType.Action, () => new ActionHircGenerator_V136());
            instance.RegisterGenerator(AkBkHircType.Event, () => new ActionEventHircGenerator_V136());
            instance.RegisterGenerator(AkBkHircType.Dialogue_Event, () => new DialogueEventHircGenerator_V136());
            return instance;
        }
    }
}
