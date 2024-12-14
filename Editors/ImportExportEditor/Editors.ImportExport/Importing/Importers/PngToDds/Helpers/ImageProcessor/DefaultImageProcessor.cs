using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DirectXTexNet;

namespace Editors.ImportExport.Importing.Importers.PngToDds.Helpers.ImageProcessor
{
    public class DefaultImageProcessor : IImageProcessor
    {
        public ScratchImage Transform(ScratchImage scratchImage)
        {          
            if (!(scratchImage.GetMetadata().Format == DXGI_FORMAT.B8G8R8A8_UNORM || scratchImage.GetMetadata().Format == DXGI_FORMAT.B8G8R8A8_UNORM_SRGB))
            {
                throw new Exception($"Error: image format is {scratchImage.GetMetadata().Format}  should be uncompressed RGBA8 (BC_B8G8R8A8_UNORM)");
            }

            var outScratchImage = scratchImage.CreateImageCopy(0, false, CP_FLAGS.NONE);

            var srcImage = scratchImage.GetImage(0, 0, 0);
            var destImage = outScratchImage.GetImage(0, 0, 0);
            const long bytesPerPixel = 4;

            // copy the pixel pointer's content to a byte array
            byte[] rgbaBytes = new byte[srcImage.SlicePitch];
            Marshal.Copy(srcImage.Pixels, rgbaBytes, 0, (int)srcImage.SlicePitch);

            //for (int x = 0; x < srcImage.Width; x++)
            //{
            //    for (var y = 0; y < srcImage.Height; y++)
            //    {
            //        long index = y * srcImage.RowPitch + x * bytesPerPixel;


            //        ColorChannels.GammaRGBA(
            //            ref rgbaBytes[index + 0],
            //            ref rgbaBytes[index + 1],
            //            ref rgbaBytes[index + 2],
            //            ref rgbaBytes[index + 3],
            //            2.2f
            //            );


            //        //// TODO: REMOVE AFTER TESTING. intentioal messsup of the image, swap R and B
            //        //float R = (float)rgbaBytes[index + 0] / 255.0f;
            //        //float G = (float)rgbaBytes[index + 1] / 255.0f;
            //        //float B = (float)rgbaBytes[index + 2] / 255.0f;
            //        //float A = (float)rgbaBytes[index + 3] / 255.0f;

            //        //R = (float)Math.Pow(R, 2.2f);
            //        //G = (float)Math.Pow(G, 2.2f);
            //        //B = (float)Math.Pow(B, 2.2f);
            //        //A = (float)Math.Pow(A, 2.2f);

            //        //rgbaBytes[index + 0] = (byte)(R * 255.0f);
            //        //rgbaBytes[index + 1] = (byte)(G * 255.0f);
            //        //rgbaBytes[index + 2] = (byte)(B * 255.0f);
            //        //rgbaBytes[index + 3] = (byte)(A * 255.0f);

            //    }
            //}
            //// copy the byte array back to the pixel pointer
            //Marshal.Copy(rgbaBytes, 0, destImage.Pixels, rgbaBytes.Length);

            return outScratchImage;
        }
    }

}
