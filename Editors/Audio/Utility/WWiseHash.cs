﻿using CommunityToolkit.Diagnostics;
using System.Text;

namespace Editors.Audio.Utility
{
    public static class WwiseHash
    {
        public static uint Compute(string name)
        {
            var lower = name.ToLower().Trim();
            var bytes = Encoding.UTF8.GetBytes(lower);

            var hashValue = 2166136261;
            for (var byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
            {
                var nameByte = bytes[byteIndex];
                hashValue = hashValue * 16777619;
                hashValue = hashValue ^ nameByte;
                hashValue = hashValue & 0xFFFFFFFF;
            }

            return hashValue;
        }

        public static uint Compute30(string name)
        {
            //v
            //uint thirtyBitMask = 0x3FFFFFFF; // 30-bit mask
            //uint thirtyBitHash = fnvHash & thirtyBitMask;

            var hash = Compute(name);

            var numBits = 30;
            var mask = (1 << numBits) - 1;
            var final = hash >> numBits ^ hash & mask;
            Guard.IsLessThan(final, 1073741824);
            return (uint)final;
        }
    }
}
