﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Lidgren.Network;
using Model.LiveControl;
using Network;
using Network.Messages.LiveControl;
using Providers.Extensions;
using Rectangle = System.Drawing.Rectangle;
using DesktopDuplication;
using MirrorDriver;
using DXGI_DesktopDuplication;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WindowsInput;

namespace Providers.LiveControl.Server
{

    public class LiveControllerProvider8 :Provider
    {
        private Dictionary<uint, MouseKeyboardState> pendingMouseKeyStates;

        /*
         *Adding Dupliation Manager DXGI 
         */
        public DuplicationManager duplicationManager = null;
        public static UpdateUI RefreshUI;
        private Thread duplicateThread = null;

        private DesktopDuplicator MirrorDriver;
        public DesktopFrame frame = null;

        public Dispatcher mydispatchtoParse { get; set; }

        public event EventHandler<MouseKeyboardNotification> OnMouseKeyboardEventReceived;

        private void OnResponseMouseKeyboardMessageReceived(MessageEventArgs<MouseKeyboardNotification> obj)
        {
            string data = obj.Message.data;

            MouseKeyboardNotification newMouseKeyboardNotification = new MouseKeyboardNotification();
            newMouseKeyboardNotification.data = data;

            if (data != "")
            {
                OnMouseKeyboardEventReceived(obj,newMouseKeyboardNotification);
               
            }
            //message = System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            //parseMessage(message);
            //throw new NotImplementedException();
        }


        /// <summary>
        /// Stores a list of screen regions that have changed, to be optimized for later.
        /// </summary>
        private List<Rectangle> DesktopChanges { get; set; }

        private Stopwatch Timer { get; set; }
        public uint ScreenshotCounter = 0;
        private bool CaptureLoop = true;
  
        public LiveControllerProvider8(NetworkPeer network)
            : base(network)
        {

            /*
            DesktopChanges = new List<Rectangle>();
            Timer = new Stopwatch();
           // MirrorDriver.DesktopChange += new EventHandler<DesktopMirror.DesktopChangeEventArgs>(MirrorDriver_DesktopChange);

            try
            {
                MirrorDriver = new DesktopDuplicator(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            DesktopChanges = new List<Rectangle>();
           
          */  //MirrorDriver2.DesktopChange += new EventHandler<DesktopMirror.DesktopChangeEventArgs>(MirrorDriver_DesktopChange);
        }

        public async Task MainSendScreenThread()
        {
            while(CaptureLoop)
            {
                  CapturedChangedRects();
                  Console.WriteLine("Capture");
            }
            Console.WriteLine("Exited");
         }

        public async Task CaptureFrame()
        {
            FrameData frameData;
            duplicationManager.GetFrame(out frameData);
            //duplicationManager.GetChangedRects(ref frameData); //TODO pending
        }

        public void CapturedChangedRects()
        {
            FrameData data = null;
            duplicationManager.GetChangedRects(ref data);
        }

        private void MirrorDriver_DesktopChange(object sender, DesktopMirror.DesktopChangeEventArgs e)
        {
            var rectangle = new Rectangle(e.Region.x1, e.Region.y1, e.Region.x2 - e.Region.x1,
                                                e.Region.y2 - e.Region.y1);
            DesktopChanges.Add(rectangle);
        }

        public override void RegisterMessageHandlers()
        {
            Network.RegisterMessageHandler<RequestChangeColorDepth>(OnRequestColorDepthChanged);
            Network.RegisterMessageHandler<RequestScreenshotMessage>(OnRequestScreenshotMessageReceived2);
            Network.RegisterMessageHandler<MouseKeyboardNotification>(OnResponseMouseKeyboardMessageReceived);
        }

        private  async void OnRequestColorDepthChanged(MessageEventArgs<RequestChangeColorDepth> e)
        {
            if (e.Message.Bpp!= 0)
            {
                bpp = e.Message.Bpp;
                CaptureLoop = false;
            }

            mydispatchtoParse = Dispatcher.CurrentDispatcher;
            duplicationManager = DuplicationManager.GetInstance(mydispatchtoParse,bpp);
           
            duplicationManager.onNewFrameReady += DuplicationManager_onNewFrameReady1;
            CaptureLoop = true;
            await CaptureFrame();
            var task = Task.Factory.StartNew(() => MainSendScreenThread());
        }


        //on screen shared initiated new
        private async void OnRequestScreenshotMessageReceived2(MessageEventArgs<RequestScreenshotMessage> e)
        {

            //duplicateThread = new Thread(Demo);
            if (e.Message.MTU != 0 && e.Message.ImageQuality != 0)
            {
                mtu = e.Message.MTU;
                ImageQuality = e.Message.ImageQuality;
                CaptureLoop = false;
            }

            mydispatchtoParse = Dispatcher.CurrentDispatcher;
            duplicationManager = DuplicationManager.GetInstance(mydispatchtoParse,bpp);
            duplicationManager.onNewFrameReady += DuplicationManager_onNewFrameReady1; 
            CaptureLoop = true;
            await CaptureFrame();
            var task = Task.Factory.StartNew(()=> MainSendScreenThread());
            
        }

        private void DuplicationManager_onNewFrameReady1(Bitmap newBitmap, Rectangle rectangle)
        {
            var screenshot = newBitmap;
            var stream = new MemoryStream();
            //screenshot.SetResolution(50.0f, 50.0f);
            var newbitmap = new Bitmap(screenshot, screenshot.Width / ImageQuality, screenshot.Height / ImageQuality);
            newbitmap.Save(stream, ImageFormat.Png);
            SendFragmentedBitmap(stream.ToArray(),rectangle);
        }
        
        //on screen shared initiated old
        private void OnRequestScreenshotMessageReceived(MessageEventArgs<RequestScreenshotMessage> e)
        {
            while (true)
            {
                Application.DoEvents();
                DesktopFrame frame = null;
                try
                {
                    frame = MirrorDriver.GetLatestFrame();
                }
                catch (Exception ex)
                {
                    MirrorDriver = new DesktopDuplicator(0);
                    continue;
                }
                if (frame != null)
                {
                    var screenshot = frame.DesktopImage;
                    var stream = new MemoryStream();
                    //screenshot.Save(stream, ImageFormat.Bmp);
                    SendFragmentedBitmap(stream.ToArray(), Screen.PrimaryScreen.Bounds);
                }
            }
               
         }

        private void SendFragmentedBitmap(byte[] bitmapBytesRecieved, Rectangle region)
        {
            // Send ResponseBeginScreenshotMessage
           byte[] bitmapBytes = Lz4Net.Lz4.CompressBytes(bitmapBytesRecieved, Lz4Net.Lz4Mode.HighCompression);

            var beginMessage = new ResponseBeginScreenshotMessage();
            beginMessage.Number = ++ScreenshotCounter;
            beginMessage.Region = region;
            beginMessage.FinalLength = bitmapBytes.Length;
            beginMessage.ScreenHeight = (uint)Screen.PrimaryScreen.Bounds.Height;
            beginMessage.ScreenWidth = (uint)Screen.PrimaryScreen.Bounds.Width;
            Network.SendMessage(beginMessage, NetDeliveryMethod.ReliableOrdered, 0);

            // Send ResponseScreenshotMessage

            // We don't want to send a 300 KB image - we want to make each packet mtu bytes
            // int numFragments = (bitmapBytes.Length / mtu) + 1;
            int numFragments = ((int)Math.Floor((decimal)bitmapBytes.Length / (decimal)mtu)) + 1;

            for (int i = 0; i < numFragments; i++)
            {
                var message = new ResponseScreenshotMessage();

                byte[] regionFragmentBuffer = null;

                if (i != numFragments - 1 && i < numFragments)
                {
                    regionFragmentBuffer = new byte[mtu];
                    Buffer.BlockCopy(bitmapBytes, i * mtu, regionFragmentBuffer, 0, mtu);
                }
                else if (i == numFragments - 1 || numFragments == 1)
                {
                    int bytesLeft = (int)(bitmapBytes.Length % mtu);
                    regionFragmentBuffer = new byte[bytesLeft];
                    Buffer.BlockCopy(bitmapBytes, i * mtu, regionFragmentBuffer, 0, bytesLeft);
                }
                else if (i == numFragments - 1)
                {
                    break;
                }

                if (regionFragmentBuffer == null)
                    Debugger.Break();

                message.Number = ScreenshotCounter;
                message.Image = regionFragmentBuffer;
                message.SendIndex = (i <= 0) ? (0) : (i);
                message.ScreenWidth = (uint)duplicationManager.width;
                message.ScreenHeight = (uint)duplicationManager.height;
                Network.SendMessage(message, NetDeliveryMethod.ReliableOrdered, 0);
                Trace.WriteLine(String.Format("Sent screenshot #{0}, fragment #{1} of {2} ({3} KB).", ScreenshotCounter, i, numFragments, message.Image.Length.ToKilobytes()));
            }

            Network.SendMessage(new ResponseEndScreenshotMessage() { Number = ScreenshotCounter });
            Trace.WriteLine(String.Format("Completed send of screenshot #{0}, Size: {1} KB", ScreenshotCounter, bitmapBytes.Length.ToKilobytes()));
        }

        private static float GetTotalScreenshotsKB(List<Screenshot> screenshots)
        {
            float total = 0f;
            screenshots.ForEach(x =>
            {
                total += x.Image.Length.ToKilobytes();
            });
            return total;
        }

        /// <summary>
        /// Combines intersecting rectangles to reduce redundant sends.
        /// </summary>
        /// <returns></returns>
        public IList<Rectangle> GetOptimizedRectangleRegions()
        {
            var desktopChangesCopy = new List<Rectangle>(DesktopChanges);
            DesktopChanges.Clear();

            desktopChangesCopy.ForEach((x) => desktopChangesCopy.ForEach((y) =>
            {
                if (x != y && x.Contains(y))
                {
                    desktopChangesCopy.Remove(y);
                }
            }));

            return desktopChangesCopy;
        }

        public event EventHandler<DesktopChangedEventArgs> OnDesktopChanged;

    }
}
