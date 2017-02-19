using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Rectangle = SharpDX.Rectangle;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;
using Model.LiveControl;
using System.Linq;
using BitmapConversion;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace DXGI_DesktopDuplication
{
    public class DuplicationManager
    {
        /// <summary>
        ///     If the caller specifies a zero time-out interval in the TimeoutInMilliseconds parameter, AcquireNextFrame verifies
        ///     whether there is a new desktop image available, returns immediately, and indicates its outcome with the return
        ///     value. If the caller specifies an INFINITE time-out interval in the TimeoutInMilliseconds parameter, the time-out
        ///     interval never elapses
        /// </summary>
        private const int TIME_OUT = 130;

        private static int counter;
        private static readonly DuplicationManager instance = new DuplicationManager();

        private Device device;
        public Dispatcher dispatcher { get; set; }
        private OutputDuplication duplicatedOutput;
        public int height;
        private OutputDescription outputDescription;
        private Texture2D screenTexture;
        private Texture2DDescription textureDesc;
        public int width;
        //private Texture2D acquiredDesktopImage = null;

        ImageCodecInfo myImageCodecInfo;
        Encoder myEncoder;
        EncoderParameter myEncoderParameter;
        EncoderParameters myEncoderParameters;
        public static int ColorDepth  { get; set; }

        private DuplicationManager()
        {
            Init();
        }

        public static DuplicationManager GetInstance(Dispatcher dispatcher,int cbpp)
        {
            instance.SetDispatcher(dispatcher);
            ColorDepth = cbpp;
            return instance;
        }
        public delegate void NewFrameReady(Bitmap newBitmap,System.Drawing.Rectangle rectangle);
        public event NewFrameReady onNewFrameReady;
     
          private void Init()
        {
            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            const int numOutput = 0;

            // Create DXGI Factory1
            var factory = new Factory1();
            Adapter1 adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            Output output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            width = output.Description.DesktopBounds.Width;
            height = output.Description.DesktopBounds.Height;

            // Create Staging screenTexture CPU-accessible
            textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = {Count = 1, Quality = 0},
                Usage = ResourceUsage.Staging
            };

            outputDescription = output.Description;

            // Demo the output
            duplicatedOutput = output1.DuplicateOutput(device);

            screenTexture = new Texture2D(device, textureDesc);
        }

        public OutputDescription GetOutputDescription()
        {
            return outputDescription;
        }

        private void GetDirtyAndMoveRects(ref FrameData data, Resource screenResource,
            OutputDuplicateFrameInformation duplicateFrameInformation)
        {
            //copy resource into memory that can be accessed by the CPU
            using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
            screenResource.Dispose();

            int bufSize = duplicateFrameInformation.TotalMetadataBufferSize;

            if (bufSize <= 0) return;

            var moveRectangles =
                new OutputDuplicateMoveRectangle
                    [
                    (int)
                        Math.Ceiling((double) bufSize/
                                     Marshal.SizeOf(typeof (OutputDuplicateMoveRectangle)))
                    ];

            Console.WriteLine("Move : {0}  {1}  {2}  {3}", moveRectangles.Length, bufSize,
                Marshal.SizeOf(typeof (OutputDuplicateMoveRectangle)),
                bufSize/Marshal.SizeOf(typeof (OutputDuplicateMoveRectangle)));

            //get move rects
            if (moveRectangles.Length > 0)
                duplicatedOutput.GetFrameMoveRects(bufSize, moveRectangles, out bufSize);

            data.MoveRectangles = moveRectangles;
            data.MoveCount = bufSize;

            bufSize = duplicateFrameInformation.TotalMetadataBufferSize - bufSize;
            var dirtyRectangles = new Rectangle[bufSize/Marshal.SizeOf(typeof (Rectangle))];
            Console.WriteLine("Dirty : {0}  {1}  {2}  {3}", dirtyRectangles.Length, bufSize,
                Marshal.SizeOf(typeof (Rectangle)), bufSize/Marshal.SizeOf(typeof (Rectangle)));
            //get dirty rects
            if (dirtyRectangles.Length > 0)
                duplicatedOutput.GetFrameDirtyRects(bufSize, dirtyRectangles, out bufSize);
            data.DirtyRectangles = dirtyRectangles;
            data.DirtyCount = bufSize;

            data.Frame = screenTexture;

            data.FrameInfo = duplicateFrameInformation;
        }

        public void FireNewFrameEvent(Bitmap newBitmap,System.Drawing.Rectangle rect)
        {
            try
            {
                
               onNewFrameReady(newBitmap,rect);   
            }
            catch { }
        }
        public void SetDispatcher(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public void GetChangedRects(ref FrameData data)
        {
            if (data == null)
                data = new FrameData();

            bool captured = false;
            //var bitmapTosend = Texture2DToBitmap();
            //FireNewFrameEvent(bitmapTosend, new System.Drawing.Rectangle(0, 0, bitmapTosend.Width, bitmapTosend.Height));

            lock (duplicatedOutput)
                for (int i = 0; !captured; i++)
                {
                    bool dirtyCapture = false;  
                    try
                    {
                        Resource screenResource;
                        OutputDuplicateFrameInformation duplicateFrameInformation;

                        //try to get duplicated frame within given time
                        duplicatedOutput.AcquireNextFrame(TIME_OUT, out duplicateFrameInformation, out screenResource);

                        if (i > 0)
                        {
                            GetDirtyAndMoveRects(ref data, screenResource, duplicateFrameInformation);


                            //display results
                            if (data.MoveRectangles != null)
                                foreach (OutputDuplicateMoveRectangle moveRectangle in data.MoveRectangles)
                                {
                                    Console.WriteLine("MoveRectangle : {0}", (moveRectangle.DestinationRect));
                                }

                            int subCounter = 0;

                            if (data.DirtyRectangles != null)
                            {
                                foreach (Rectangle dirtyRectangle in data.DirtyRectangles)
                                {
                                    Console.WriteLine("DirtyRectangle : {0}    Size : {1}    {2}，{3}",
                                        dirtyRectangle, dirtyRectangle.Size.Height * dirtyRectangle.Size.Width,
                                        counter, subCounter);

                                    Bitmap exactrectangle = ExtractRect(dirtyRectangle.X, dirtyRectangle.Y, dirtyRectangle.Width,
                                           dirtyRectangle.Height);

                                    //exactrectangle.Save("dirty" + (counter) + "-" + (subCounter++) + ".jpg");
                                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(dirtyRectangle.X, dirtyRectangle.Y, dirtyRectangle.Width, dirtyRectangle.Height);
                                    // Size 8bit-16Bit-32Bit
                                    if (dirtyRectangle.Width < width  && dirtyRectangle.Height < height )
                                    {
                                        //Grayscale(clone);  
                                        //Bitmap compressed = exactrectangle.Clone(new System.Drawing.Rectangle(0, 0, exactrectangle.Width, exactrectangle.Height), System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
                                        //FireNewFrameEvent(compressed, rect);
                                        dirtyCapture = true;

                                        if (ColorDepth == 4)
                                        {
                                            Bitmap target = exactrectangle;
                                            Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
                                            //Grayscale(target);
                                            FireNewFrameEvent(NewPicture, rect);

                                        }
                                        else if (ColorDepth == 8)
                                        {
                                            Bitmap target = exactrectangle;
                                            Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                                            //Grayscale(target);
                                            FireNewFrameEvent(NewPicture, rect);
                                        }
                                        else if (ColorDepth == 16)
                                        {
                                            Bitmap target = exactrectangle;
                                            Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                                            //Grayscale(target);
                                            FireNewFrameEvent(NewPicture, rect);

                                        }
                                        else if (ColorDepth == 24)
                                        {
                                            Bitmap target = exactrectangle;
                                            Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                                            //Grayscale(target);
                                            FireNewFrameEvent(NewPicture, rect);
                                        }
                                        else if (ColorDepth == 32)
                                        {
                                            Bitmap target = exactrectangle;
                                            Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                            //Grayscale(target);
                                            FireNewFrameEvent(NewPicture, rect);

                                        }
                                    }
                                    //else
                                    //{
                                    //    Bitmap target = Texture2DToBitmap();
                                    //    Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
                                    //    //Grayscale(target);
                                    //    FireNewFrameEvent(NewPicture, new System.Drawing.Rectangle(0, 0, width, height));
                                    //}
                                }
                            }
                            else 
                            {
                                if (ColorDepth == 4)
                                {
                                    Bitmap target = Texture2DToBitmap();
                                    Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
                                    //Grayscale(target);
                                    FireNewFrameEvent(NewPicture, new System.Drawing.Rectangle(0, 0, width, height));
                                }
                                else if (ColorDepth == 8)
                                {
                                    Bitmap target = Texture2DToBitmap();
                                    Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                                    //Grayscale(target);
                                    FireNewFrameEvent(NewPicture, new System.Drawing.Rectangle(0, 0, width, height));
                                }
                                else if (ColorDepth == 16)
                                {
                                    Bitmap target = Texture2DToBitmap();
                                    Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                                    //Grayscale(target);
                                    FireNewFrameEvent(NewPicture, new System.Drawing.Rectangle(0, 0, width, height));

                                }
                                else if (ColorDepth == 24)
                                {
                                    Bitmap target = Texture2DToBitmap();
                                    Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                                    //Grayscale(target);
                                    FireNewFrameEvent(NewPicture, new System.Drawing.Rectangle(0, 0, width, height));

                                }
                                else if (ColorDepth == 32)
                                {
                                    Bitmap target = Texture2DToBitmap();
                                    Bitmap NewPicture = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                    //Grayscale(target);
                                    FireNewFrameEvent(NewPicture, new System.Drawing.Rectangle(0, 0, width, height));
                                }
                            }
                            counter++;
                            dirtyCapture = false;
                            captured = true;
                        }
                        duplicatedOutput.ReleaseFrame();
                    }
                    catch (SharpDXException e)
                    {
                        if (e.ResultCode.Code != ResultCode.WaitTimeout.Result.Code)
                            Console.WriteLine("GetChangedRects : {0}", e.Message);
                    }
                }
        }
        private static void Grayscale(Bitmap source)
        {
            using (var bmp = new LockBitmap(source))
            {
                for (var y = 0; y < bmp.Height; y++)
                {
                    for (var x = 0; x < bmp.Width; x++)
                    {
                        var color = bmp.GetPixel(x, y);
                        var minComponent = (color.R + color.G + color.B) / 3;

                        if (minComponent > 255)
                        {
                            minComponent = 255;
                        }

                        var newColor = System.Drawing.Color.FromArgb(color.A, minComponent, minComponent, minComponent);
                        bmp.SetPixel(x, y, newColor);
                    }
                }
            }
        }
        private Bitmap ChangePixelFormat(Bitmap inputImage, System.Drawing.Imaging.PixelFormat newFormat)
        {
            Bitmap bmp = new Bitmap(inputImage.Width, inputImage.Height, newFormat);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(inputImage, 0, 0);
            }
            return bmp;
        }
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        public void GetFrame(out FrameData data)
        {
            data = new FrameData();
            bool captureDone = false;
            lock (duplicatedOutput)
                for (int i = 0; !captureDone; i++)
                    try
                    {
                        Resource screenResource;
                        OutputDuplicateFrameInformation duplicateFrameInformation;

                        // Try to get duplicated frame within given time
                        duplicatedOutput.AcquireNextFrame(TIME_OUT, out duplicateFrameInformation, out screenResource);

                        if (i > 0)
                        {
                            // copy resource into memory that can be accessed by the CPU
                            using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                                device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
                            screenResource.Dispose();

                            Bitmap bitmap;
                            if (ColorDepth == 4)
                            {
                                Bitmap target = Texture2DToBitmap();
                                 bitmap = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
                                //bitmap.Save("save" + (counter++) + ".bmp");
                                data.Frame = screenTexture;
                                // Capture done
                                captureDone = true;
                                FireNewFrameEvent(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                            }else if (ColorDepth == 8)
                            {
                                Bitmap target = Texture2DToBitmap();
                                bitmap = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                                //bitmap.Save("save" + (counter++) + ".bmp");
                                data.Frame = screenTexture;
                                // Capture done
                                captureDone = true;
                                FireNewFrameEvent(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                            }
                             else if (ColorDepth == 16)
                            {
                                Bitmap target = Texture2DToBitmap();
                                bitmap = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                                //bitmap.Save("save" + (counter++) + ".bmp");
                                data.Frame = screenTexture;
                                // Capture done
                                captureDone = true;
                                FireNewFrameEvent(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                            }else if (ColorDepth == 24)
                            {
                                Bitmap target = Texture2DToBitmap();
                                bitmap = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                                //bitmap.Save("save" + (counter++) + ".bmp");
                                data.Frame = screenTexture;
                                // Capture done
                                captureDone = true;
                                FireNewFrameEvent(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                            }
                            else if (ColorDepth == 32)
                            {
                                Bitmap target = Texture2DToBitmap();
                                bitmap = target.Clone(new System.Drawing.Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                //bitmap.Save("save" + (counter++) + ".bmp");
                                data.Frame = screenTexture;
                                // Capture done
                                captureDone = true;
                                FireNewFrameEvent(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                            }


                        }

                        duplicatedOutput.ReleaseFrame();
                    }
                    catch (SharpDXException e)
                    {
                        if (e.ResultCode.Code != ResultCode.WaitTimeout.Result.Code)
                        {
                            Console.WriteLine("GetFrame : {0}", e.Descriptor);
                        }
                    }

            // Display the screenTexture using system associated viewer
            //Process.Start(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "save.bmp")));
        }

        public Bitmap Texture2DToBitmap()
        {

            //return ExtractRect(0, 0, width, height);

            // Get the desktop capture screenTexture
            DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read,
              MapFlags.None);

            //// Create Drawing.Bitmap

            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb); //不能是ARGB
            var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);

            //// Copy pixels from screen capture Texture to GDI bitmap
            BitmapData mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            IntPtr sourcePtr = mapSource.DataPointer;
            IntPtr destPtr = mapDest.Scan0;
            for (int y = 0; y < height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            bitmap.UnlockBits(mapDest);
            device.ImmediateContext.UnmapSubresource(screenTexture, 0);

            return bitmap;
        }
        //TODO copy bitmap of the designated area
        public Bitmap ExtractRect(int originX, int originY, int width, int height)
        {
            // Get the desktop capture screenTexture
            DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read,
                MapFlags.None);

            // Create Drawing.Bitmap
            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb); //不能是ARGB
            var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);

            // Copy pixels from screen capture Texture to GDI bitmap
            BitmapData mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            IntPtr sourcePtr = mapSource.DataPointer;
            IntPtr destPtr = mapDest.Scan0;

            sourcePtr = IntPtr.Add(sourcePtr, originY * mapSource.RowPitch + originX * 4);
            for (int y = 0; y < height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, width*4);

                // Advance pointers
                if(y != height - 1)
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            bitmap.UnlockBits(mapDest);
            device.ImmediateContext.UnmapSubresource(screenTexture, 0);

            return bitmap;
        }
    }
}