﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Xna.Framework;
using Shared.GameFormats.AnimationMeta.Parsing;

namespace Shared.GameFormats.AnimationMeta.Definitions
{
    [MetaData("CAMERA_SHAKE_POS", 10)]
    public class CameraShakePos : DecodedMetaEntryBase
    {
        [MetaDataTag(5)]
        public Vector3 Position { get; set; }
    }
}
