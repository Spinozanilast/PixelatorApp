using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixelator
{
    internal class PixelatorTool
    {
        /// <summary>
        /// Initializes block size field.
        /// </summary>
        /// <param name="blockSize"> Pixels count in pixel block of target image.</param>
        public PixelatorTool(int blockSize)
        {
            this.blockSize = blockSize;
        }

        /// <summary>
        /// Default pixel block size equals 20 sourse pixels.
        /// </summary>
        public static readonly int DefaultBlockSize = 20;

        /// <summary>
        /// Block size (count of pixels of source image in one target pixelate image).
        /// </summary>
        private int blockSize = DefaultBlockSize;

        /// <summary>
        /// Gets or sets pixel block size.
        /// </summary>
        private int BlockSize
        {
            get
            {
                return blockSize;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(value.ToString());
                }
                
                blockSize = value;
            }
        }

        /// <summary>
        /// Pixelate source image directly reading pixels color itself for finding average color for target image future pixel.
        /// </summary>
        /// <param name="sourceImage"> Source image opf Image Type </param>
        /// <param name="targetImageSource"> Target image reference of ref Image Type</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public Image DrawPixelateImage(Image sourceImage, out Bitmap targetImageSourceBitmap)
        {
            CheckForNullWithException(sourceImage, nameof(sourceImage));

            var sourceImageBitmap = (Bitmap)sourceImage;
            targetImageSourceBitmap = (Bitmap)(sourceImageBitmap.Clone()); 

            if (sourceImage.Size != targetImageSourceBitmap.Size) throw new ArgumentException("Images sizes not equals.");

            for (var w = 0; w < sourceImage.Width; w += this.blockSize)
            {
                for (var h = 0; h < sourceImage.Height; h += this.blockSize)
                {
                    BlockColor blockColor = new BlockColor();

                    for (var wTarget = 0; wTarget < this.blockSize; wTarget++)
                    {
                        for (var hTarget = 0; hTarget < this.blockSize; hTarget++)
                        {
                            if (w + wTarget >= sourceImage.Width || h + hTarget >= targetImageSourceBitmap.Height) continue;

                            var color = sourceImageBitmap.GetPixel(wTarget + w, hTarget + h);
                            
                            blockColor.R = color.R;
                            blockColor.G = color.G;
                            blockColor.B = color.B;
                            blockColor.A = color.A;
                            blockColor.PixelsCount++;
                        }
                    }

                    var averagePixel = System.Drawing.Color.FromArgb(
                        blockColor.A / blockColor.PixelsCount,
                        blockColor.R / blockColor.PixelsCount,
                        blockColor.G / blockColor.PixelsCount,
                        blockColor.B / blockColor.PixelsCount);

                    for (var i = w; i < this.blockSize + w; i++) 
                    {
                        for (var j = h; j < this.blockSize + h; j++)
                        {
                            targetImageSourceBitmap.SetPixel(i, j, averagePixel);
                        }
                    }
                }
            }

            return targetImageSourceBitmap;
        }

        /// <summary>
        /// Pixelate source image directly reading pixels color itself for finding average color for target image future pixel.
        /// </summary>
        /// <param name="sourceImage">Source image of the ImageSource Type</param>
        /// <param name="targetBitmap">Out target Bitmap image of the Bitmap type</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ImageSource DrawPixelateImage(ImageSource sourceImage, out Bitmap targetBitmap)
        {
            CheckForNullWithException(sourceImage, nameof(sourceImage));

            var sourceImageBitmap = ImageSourceToBitmap(sourceImage);
            targetBitmap = (Bitmap)sourceImageBitmap.Clone();

            if (sourceImageBitmap.Size != targetBitmap.Size) throw new ArgumentException("Images sizes not equals.");

            for (var w = 0; w < sourceImage.Width; w += this.blockSize)
            {
                for (var h = 0; h < sourceImage.Height; h += this.blockSize)
                {
                    BlockColor blockColor = new BlockColor();

                    for (var wTarget = 0; wTarget < this.blockSize; wTarget++)
                    {
                        for (var hTarget = 0; hTarget < this.blockSize; hTarget++)
                        {
                            if (w + wTarget >= sourceImage.Width || h + hTarget >= targetBitmap.Height) continue;

                            var color = sourceImageBitmap.GetPixel(wTarget + w, hTarget + h);

                            blockColor.R += color.R;
                            blockColor.G += color.G;
                            blockColor.B += color.B;
                            blockColor.A += color.A;
                            blockColor.PixelsCount++;
                        }
                    }

                    var averagePixel = System.Drawing.Color.FromArgb(
                        blockColor.A / blockColor.PixelsCount,
                        blockColor.R / blockColor.PixelsCount,
                        blockColor.G / blockColor.PixelsCount,
                        blockColor.B / blockColor.PixelsCount);

                    for (var i = w; i < this.blockSize + w && i < targetBitmap.Width; i++)
                    {
                        for (var j = h; j < this.blockSize + h && j < targetBitmap.Height; j++)
                        {
                            targetBitmap.SetPixel(i, j, averagePixel);
                        }
                    }
                }
            }

            return ImageSourceFromBitmap(targetBitmap);
        }

        public static Bitmap ImageSourceToBitmap(ImageSource image)
        {
            var memoryStream = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create((System.Windows.Media.Imaging.BitmapSource)image));
            encoder.Save(memoryStream);
            memoryStream.Flush();
            return new Bitmap(memoryStream);
        }


        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /// <summary>
        ///  Convert Bitmap to Image Source.
        /// </summary>
        /// <param name="bmp"> Image of Bitmap Type </param>
        /// <returns>Image Source value from Bitmap</returns>
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                ImageSource newSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(handle);
                return newSource;
            }
            catch (Exception)
            {
                DeleteObject(handle);
                return null;
            }
        }

        /*
         csharp
public void DrawPixelateImage(Image sourceImage, ref Image targetImageSource)
{
    CheckForNullWithException(sourceImage, nameof(sourceImage));
    CheckForNullWithException(targetImageSource, nameof(targetImageSource));
     var sourceImageBitmap = (Bitmap)sourceImage;
    var targetImageSourceBitmap = (Bitmap)targetImageSource;
     if (sourceImage.Size != targetImageSource.Size) throw new ArgumentException("Images sizes not equals.");
     var sourceData = sourceImageBitmap.LockBits(new Rectangle(0, 0, sourceImageBitmap.Width, sourceImageBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var targetData = targetImageSourceBitmap.LockBits(new Rectangle(0, 0, targetImageSourceBitmap.Width, targetImageSourceBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
     var sourceStride = sourceData.Stride;
    var targetStride = targetData.Stride;
     var sourceScan0 = sourceData.Scan0;
    var targetScan0 = targetData.Scan0;
     var blockSize = this.blockSize;
     unsafe
    {
        byte* sourcePtr = (byte*)sourceScan0;
        byte* targetPtr = (byte*)targetScan0;
         for (var w = 0; w < sourceImage.Width; w += blockSize)
        {
            for (var h = 0; h < sourceImage.Height; h += blockSize)
            {
                BlockColor blockColor = new BlockColor();
                 for (var wTarget = 0; wTarget < blockSize; wTarget++)
                {
                    for (var hTarget = 0; hTarget < blockSize; hTarget++)
                    {
                        var x = w + wTarget;
                        var y = h + hTarget;
                         if (x < sourceImage.Width && y < sourceImage.Height)
                        {
                            var offset = y * sourceStride + x * 4;
                             var colorB = sourcePtr[offset];
                            var colorG = sourcePtr[offset + 1];
                            var colorR = sourcePtr[offset + 2];
                            var colorA = sourcePtr[offset + 3];
                             blockColor.R += colorR;
                            blockColor.G += colorG;
                            blockColor.B += colorB;
                            blockColor.A += colorA;
                            blockColor.PixelsCount++;
                        }
                    }
                }
                 var pixelCount = blockColor.PixelsCount;
                var averagePixel = Color.FromArgb(
                    blockColor.A / pixelCount,
                    blockColor.R / pixelCount,
                    blockColor.G / pixelCount,
                    blockColor.B / pixelCount);
                 for (var i = w; i < blockSize + w; i++)
                {
                    for (var j = h; j < blockSize + h; j++)
                    {
                        var offset = j * targetStride + i * 4;
                         targetPtr[offset] = averagePixel.B;
                        targetPtr[offset + 1] = averagePixel.G;
                        targetPtr[offset + 2] = averagePixel.R;
                        targetPtr[offset + 3] = averagePixel.A;
                    }
                }
            }
        }
    }
     sourceImageBitmap.UnlockBits(sourceData);
    targetImageSourceBitmap.UnlockBits(targetData);
     targetImageSource = targetImageSourceBitmap;
}
         */

        private void CheckForNullWithException(Object image, string message)
        {
            if (image == null)
            {
                throw new ArgumentNullException(message);
            }
        }

        private struct BlockColor
        {
            /// <summary>
            /// Gets or sets color red channel value - RED.
            /// </summary>
            public int R
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets color green channel value - GREEN.
            /// </summary>
            public int G
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets color blue channel value - BLUE.
            /// </summary>
            public int B
            {
                get; 
                set;
            }
            
            /// <summary>
            /// Gets or sets color alpha channel value (transparency) - ALPHA.
            /// </summary>
            public int A
            {
                get; 
                set;
            }

            /// <summary>
            /// Gets or sets total count of pixels to calculate average color of pixel block
            /// </summary>
            public int PixelsCount
            {
                get;
                set;
            }
        }
    }
}
