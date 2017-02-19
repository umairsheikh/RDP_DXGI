﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Model.LiveControl;
using Network;
using Network.Messages.LiveControl;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Providers.LiveControl.Client
{
    public class LiveControlProvider : Provider
    {
        private Dictionary<uint, Screenshot> pendingScreenshots;
        public IList<Screenshot> liveScreenshots;

      
        public LiveControlProvider(NetworkPeer network)
            : base(network)
        {
            pendingScreenshots = new Dictionary<uint, Screenshot>(1000);
        }

      
        public int GetMTU()
        {
            return Server.LiveControllerProvider8.mtu;
        }

        public int GetImageQualityDiv()
        {
            return Server.LiveControllerProvider8.ImageQuality;
        }

        public override void RegisterMessageHandlers()
        {
            Network.RegisterMessageHandler<ResponseBeginScreenshotMessage>(OnResponseBeginScreenshotMessageReceived);
            Network.RegisterMessageHandler<ResponseScreenshotMessage>(OnResponseScreenshotMessageReceived);
            Network.RegisterMessageHandler<ResponseEndScreenshotMessage>(OnResponseEndScreenshotMessageReceived);
            Network.RegisterMessageHandler<ResponseEmptyScreenshotMessage>(OnResponseEmptyScreenshotMessageReceived);
        }

        private void OnResponseBeginScreenshotMessageReceived(MessageEventArgs<ResponseBeginScreenshotMessage> e)
        {
            Trace.WriteLine("Received ResponseBeginScreenshotMessage.");
            pendingScreenshots.Add(e.Message.Number, new Screenshot(new byte[e.Message.FinalLength], e.Message.Region, e.Message.Number,e.Message.ScreenWidth,e.Message.ScreenHeight));
        }

        private void OnResponseScreenshotMessageReceived(MessageEventArgs<ResponseScreenshotMessage> e)
        {
            Trace.WriteLine(String.Format("Received ResponseScreenshotMessage, Number: {0}, Size: {1} KB", e.Message.Number, GetKBFromBytes(e.Message.Image.Length)));
            uint num = e.Message.Number;

            // Slowly build our image bytes
            Buffer.BlockCopy(e.Message.Image, 0, pendingScreenshots[num].Image, e.Message.SendIndex * Server.LiveControllerProvider8.mtu, e.Message.Image.Length);
        }

        private void OnResponseEndScreenshotMessageReceived(MessageEventArgs<ResponseEndScreenshotMessage> e)
        {
            Trace.WriteLine("Received ResponseEndScreenshotMessage.");
            OnScreenshotReceived(this, new ScreenshotMessageEventArgs() { Screenshot = pendingScreenshots[e.Message.Number] });
            pendingScreenshots.Remove(e.Message.Number);
        }

        /// <summary>
        /// Raises the <see cref="E:ResponseEmptyScreenshotMessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Network.MessageEventArgs&lt;Network.Messages.LiveControl.ResponseEmptyScreenshotMessage&gt;"/> instance containing the event data.</param>
        private void OnResponseEmptyScreenshotMessageReceived(MessageEventArgs<ResponseEmptyScreenshotMessage> e)
        {
            Network.SendMessage(new RequestScreenshotMessage());
        }


        public async Task ChangeColorDepth(int bpp)
        {
            var RequestNewBpp = new RequestChangeColorDepth(bpp);
            Network.SendMessage(RequestNewBpp);
            Server.LiveControllerProvider8.bpp = bpp;
        }

        public async Task ChangeScreenShareDynamics(int mtu, int quality)
        {
            var RequestNewScreen = new RequestScreenshotMessage(mtu, quality);
            Network.SendMessage(RequestNewScreen);
           await Task.Delay(2000);
            Server.LiveControllerProvider8.mtu = mtu;
            Server.LiveControllerProvider8.ImageQuality = quality; 
        }

        private static float GetKBFromBytes(long bytes)
        {
            return (float)((float)bytes / (float)1024);
        }

        public event EventHandler<ScreenshotMessageEventArgs> OnScreenshotReceived;

 
        public void sendMouseKeyboardStateMessage(String message)
        {
            Network.SendMessage(new MouseKeyboardNotification() {data = message });
        }

        public override void SetQualityParameters(int MTU, int Quality, int Bpp)
        {
            throw new NotImplementedException();
        }
    }
}
