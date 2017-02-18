using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Providers.Nova.Modules;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Managers.Nova.Server;
using System.Diagnostics;
using Managers.LiveControl.Client;
using Providers.LiveControl.Client;
using Network;
using Network.Messages.Nova;
using System.IO;
using System.Drawing.Imaging;
using RamGecTools;
using HookerClient;
using System.ComponentModel;
using SharpDX;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WindowsInput;
using System.Windows.Threading;

namespace RDPApplication
{
    
    public partial class Form1 : Form
    {

        private Thread duplicateThread = null;

        public Managers.Nova.Client.NovaManager NovaManagerClient;
        public Managers.LiveControl.Client.LiveControlManager LiveControlManagerClient;

        public NovaManager NovaManagerServer;
        public Managers.LiveControl.Server.LiveControlManager LiveControlManagerServer;


        //Hook Servers
        private InputSimulator inputSimulator;
        private Task updateImageThread;
        private Queue<Model.LiveControl.Screenshot> LiveShots;

        //Client Screen share 
        private WriteableBitmap BGWritable;
        private int ImageDivisor = 1;
        private int mtu = 1;
        private RenderTargetBitmap buffer;
        private DrawingVisual drawingVisual;
        private double screenImagePositionX;
        private double screenImagePositionY;
        private int hostScreenWidth;
        private int hostScreenHeight;


        //VPN
        private DotRas.RasPhoneBook myRasPhonebook;
        private static DotRas.RasDialer myRasDialer;


        public Form1()
        {
            InitializeComponent();
        }

        public async Task InitNetworkManagerClient()
        {
            NovaManagerClient = Managers.NovaClient.Instance.NovaManager;
            LiveControlManagerClient = Managers.NovaClient.Instance.LiveControlManager;

            LiveControlManagerClient.OnScreenshotReceived += new EventHandler<ScreenshotMessageEventArgs>(LiveControlManager_OnScreenshotReceived);

            NovaManagerClient.OnIntroductionCompleted += new EventHandler<IntroducerIntroductionCompletedEventArgs>(ClientManager_OnIntroductionCompleted);
            NovaManagerClient.OnNatTraversalSucceeded += new EventHandler<Network.NatTraversedEventArgs>(ClientManager_OnNatTraversalSucceeded);
            NovaManagerClient.OnConnected += new EventHandler<ConnectedEventArgs>(ClientManager_OnConnected);

            ImageDivisor = LiveControlManagerClient.Provider.GetImageQualityDiv();
            mtu = LiveControlManagerClient.Provider.GetMTU();
            //LiveShots = new Queue<Model.LiveControl.Screenshot>();
            drawingVisual = new DrawingVisual();

        }

        public async Task InitNetworkManagerServer()
        {

            NovaManagerServer = Managers.NovaServer.Instance.NovaManager;
            LiveControlManagerServer = Managers.NovaServer.Instance.LiveControlManager;
            inputSimulator = new InputSimulator();
            NovaManagerServer.OnIntroducerRegistrationResponded += NovaManager_OnIntroducerRegistrationResponded;
            NovaManagerServer.OnNewPasswordGenerated += new EventHandler<PasswordGeneratedEventArgs>(ServerManager_OnNewPasswordGenerated);
            NovaManagerServer.Network.OnConnected += new EventHandler<Network.ConnectedEventArgs>(Network_OnConnected);

            PasswordGeneratedEventArgs passArgs = await NovaManagerServer.GeneratePassword();
            //LabelPassword.Content = passArgs.NewPassword;
            IntroducerRegistrationResponsedEventArgs regArgs = await NovaManagerServer.RegisterWithIntroducerAsTask();
            //LabelNovaId.Content = regArgs.NovaId;

            //xaml Status.Content = "Ready to Connect";
        }

        void ClientManager_OnConnected(object sender, ConnectedEventArgs e)
        {
            //  ButtonConnect.Set(() => ButtonConnect.Text, "Connected.");
            //xaml remoteConnection.Content = "Connected to remote";
            textBox1.Text = "connected";
            LiveControlManagerClient.RequestScreenshot();

            // this.Dispose();
        }

        private void ClientManager_OnIntroductionCompleted(object sender, IntroducerIntroductionCompletedEventArgs e)
        {
            switch (e.Result)
            {
                case Network.Messages.Nova.ResponseIntroducerIntroductionCompletedMessage.Result.Allowed:
                    // Do nothing, expect OnNatTraversalSucceeded() to be raised shortly
                    break;

                case Network.Messages.Nova.ResponseIntroducerIntroductionCompletedMessage.Result.Denied:
                    switch (e.DenyReason)
                    {
                        case ResponseIntroducerIntroductionCompletedMessage.Reason.WrongPassword:
                            //TextBox_Password.Set(() => TextBox_Password.Text, String.Empty); // clear the password box for re-entry
                            //xaml remoteConnection.Content = "Enter correct password/login";
                            MessageBox.Show("Please enter the correct password.");
                            break;
                        case ResponseIntroducerIntroductionCompletedMessage.Reason.Banned:
                            MessageBox.Show("You have been banned for trying to connect too many times.");
                            break;
                    }
                    break;
            }
        }

        private void ClientManager_OnNatTraversalSucceeded(object sender, NatTraversedEventArgs e)
        {
            // ButtonConnect.Set(() => ButtonConnect.Text, "Connecting to " + TextBox_Id.Text + " ...");
           //xaml remoteConnection.Content = "Connecting to machine..";

        }

        public async void SetImage(Bitmap bitmap)
        {
            IntPtr pointer = bitmap.GetHbitmap();

           //xaml BGImage.Source = Imaging.CreateBitmapSourceFromHBitmap(pointer, IntPtr.Zero, Int32Rect.Empty,
               // BitmapSizeOptions.FromEmptyOptions());
            //DeleteObject(pointer);
        }
        // Network handshakes for HOST
        void Network_OnConnected(object sender, Network.ConnectedEventArgs e)
        {
            //xaml Status.Content = "Connected";

        }

        void ServerManager_OnNewPasswordGenerated(object sender, Providers.Nova.Modules.PasswordGeneratedEventArgs e)
        {
            //xaml
            //Dispatcher.Invoke(new Action(() =>
            //{
            //    LabelPassword.Content = e.NewPassword;
            //}));

        }

        private void NovaManager_OnIntroducerRegistrationResponded(object sender, Providers.Nova.Modules.IntroducerRegistrationResponsedEventArgs e)
        {
            //xaml
            //Dispatcher.Invoke(new Action(() =>
            //{
            //    LabelNovaId.Content = e.NovaId;
            //}));

        }

        private async void startCapture_Click(object sender, RoutedEventArgs e)
        {
            //Start Server Network Registration
            await InitNetworkManagerServer();
            //LiveControlManagerServer.OnMouseKeyboardEventReceived += LiveControlManagerServer_OnMouseKeyboardEventReceived;


        }

    
        // Network handshakes for CLIENT
        private async void ConnectRemote_Click(object sender, RoutedEventArgs e)
        {
            await InitNetworkManagerClient();
            //xaml
            await NovaManagerClient.RequestIntroductionAsTask("wr4", "22");


            //if (duplicateThread.ThreadState == System.Threading.ThreadState.Unstarted)
            //{
            //    duplicateThread.Start();
            //    Console.WriteLine("Start");
            //}
            //CapturedChangedRects();
            //Console.WriteLine("Click");
        }

        private async Task updateRegionContinue()
        {
            while (LiveShots.Count != 0)
            {
                UpdateRegion(LiveShots.Dequeue());
            }


        }

        private async void LiveControlManager_OnScreenshotReceived(object sender, ScreenshotMessageEventArgs e)
        {

            var screenshot = e.Screenshot;

            using (var stream = new MemoryStream(screenshot.Image))
            {

                System.Drawing.Image image = Image.FromStream(stream);
                Application.DoEvents();
                //this.BackgroundImage = image;
                gdiScreen1.Size = new System.Drawing.Size(1280, 1024);
                gdiScreen1.Draw(image, screenshot.Region);
            }
            //var screenshot = e.Screenshot;
            //if (hostScreenWidth == 0 && hostScreenHeight == 0)
            //{
            //    hostScreenWidth = (int)e.Screenshot.ScreenWidth;
            //    hostScreenHeight = (int)e.Screenshot.ScreenHeight;

            //}
            ////LiveShots.Enqueue(screenshot);
            ////if(LiveShots.Count == 0)
            ////{
            ////await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => UpdateRegion(screenshot))));

            ////   Task.Run(() => updateRegionContinue());
            ////}
            //UpdateRegion(screenshot);
            ////if(hostScreenWidth == 1280 && hostSctreenHeight == 960)
            //// Task.Factory.StartNew(()=> updateImageThread.)
            ////LiveControlManagerClient.RequestScreenshot();
        }

        private async Task UpdateRegion(Model.LiveControl.Screenshot screenshot)
        {
            using (var stream = new System.IO.MemoryStream(screenshot.Image))
            {

                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                MemoryStream memoryStream = new MemoryStream();
                //// Save to a memory stream...
                image.Save(memoryStream, ImageFormat.Png);
                //// Rewind the stream...
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();

                using (Graphics g = Graphics.FromImage(image))
                {
                    g.DrawImage(image, new PointF(screenshot.Region.X, screenshot.Region.Y));
                }

                //await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => BGImage.Source = BGWritable)));
                //Debug.WriteLine("bitmap.height = " + bitmap.Height.ToString() + " bitmap.widht =" + bitmap.Width.ToString());

                //if ((int)bitmap.Height== hostScreenHeight / ImageDivisor && (int)bitmap.Width == hostScreenWidth / ImageDivisor)
                //{
                //    BGImage.Width = hostScreenWidth;
                //    BGImage.Height = hostScreenHeight;
                //    BGWritable = new WriteableBitmap((BitmapSource)bitmap);
                //    buffer = new RenderTargetBitmap((int)BGWritable.Width, (int)BGWritable.Height, BGWritable.DpiX, BGWritable.DpiY, PixelFormats.Pbgra32);

                //    var drawingVisual = new DrawingVisual();
                //     using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                //    {

                //        drawingContext.DrawImage(BGWritable, new Rect(0, 0, BGWritable.Width, BGWritable.Height));
                //        // drawingContext.DrawImage(bitmap, new Rect(screenshot.Region.X, screenshot.Region.Y, screenshot.Region.Width, screenshot.Region.Height));
                //        // drawingContext.DrawImage()  
                //        //    drawingContext.DrawRectangle(new SolidColorBrush(Colors.Red), null,
                //        //                      new Rect(screenshot.Region.X,screenshot.Region.Y,screenshot.Region.Width,screenshot.Region.Height));
                //        //}
                //    }
                //    buffer.Render(drawingVisual);
                //    await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => BGImage.Source = buffer)));

                //}
                //else
                //{
                //    int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel + 7) / 8;
                //    //int size = stride * bitmap.PixelHeight;
                //    //byte[] bitmapByteArray = new byte[size];
                //    //bitmap.CopyPixels(bitmapByteArray, 0, 0);
                //    var dirtyRectangle = new Int32Rect(screenshot.Region.X, screenshot.Region.Y, (Int32)bitmap.Width/ImageDivisor, (Int32)bitmap.Height/ImageDivisor);
                //    ////BGWritable.AddDirtyRect(dirtyRectangle);
                //    //BGWritable.Lock();
                //    //BGWritable.WritePixels(new Int32Rect(screenshot.Region.X, screenshot.Region.Y, (Int32)bitmap.Width, (Int32)bitmap.Height),bitmapByteArray, stride, screenshot.Region.X, screenshot.Region.Y);
                //    //BGWritable.Unlock();
                //    //buffer =new RenderTargetBitmap((int)BGWritable.Width, (int)BGWritable.Height, BGWritable.DpiX,BGWritable.DpiY, PixelFormats.Pbgra32);
                //    var drawingVisual = new DrawingVisual();
                //    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                //    {

                //        //drawingContext.DrawImage(BGWritable, new Rect(0, 0, BGWritable.Width, BGWritable.Height));
                //        drawingContext.DrawImage(bitmap, new Rect(screenshot.Region.X/ImageDivisor, screenshot.Region.Y/ImageDivisor, screenshot.Region.Width/ImageDivisor, screenshot.Region.Height/ImageDivisor));
                //        // drawingContext.DrawImage()  
                //        // drawingContext.DrawRectangle(new SolidColorBrush(Colors.Red), null,
                //        //                      new Rect(screenshot.Region.X,screenshot.Region.Y,screenshot.Region.Width,screenshot.Region.Height));
                //        //}
                //    }
                //    buffer.Render(drawingVisual);
                //    //var img = new DrawingImage(drawingVisual.Drawing);

                //    // var mergedBitmap = mergetwoBitmaps(BGWritable,new WriteableBitmap(bitmap), dirtyRectangle,stride);

                //    //BGImage.Width = bitmap.Width;
                //    //BGImage.Height = bitmap.Height;
                //    //this.Dispatcher.Invoke(() => BGImage.Source = buffer);
                //    await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => BGImage.Source = buffer)));
                //    //BGWritable = new WriteableBitmap((BitmapSource)BGImage.Source);
                //}

            }
        }

        private async Task UpdateImage(Model.LiveControl.Screenshot screenshot)
        {
            Console.WriteLine("Region rcvd:: " + screenshot.Region.ToString());
            using (var stream = new System.IO.MemoryStream(screenshot.Image))
            {

                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                MemoryStream memoryStream = new MemoryStream();
                // Save to a memory stream...
                image.Save(memoryStream, ImageFormat.Bmp);
                // Rewind the stream...
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();

                //xaml
                //BGImage.Width = hostScreenWidth;
                //BGImage.Height = hostScreenHeight;
                BGWritable = new WriteableBitmap((BitmapSource)bitmap);
                //xaml
                //await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => BGImage.Source = bitmap)));

            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await InitNetworkManagerClient();
            //xaml
            await NovaManagerClient.RequestIntroductionAsTask("5h6", "44");

        }
    }
}
