﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Providers.Nova.Modules;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
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
using WindowsInput;
using WindowsInput.Native;
using HookerServer;
using Controls.LiveControl;
using DotRas;
using SharpDX;


namespace DXGI_DesktopDuplication
{
    public partial class AppStart : Page
    {

        public static UpdateUI RefreshUI;
        private Thread duplicateThread = null;

        public Managers.Nova.Client.NovaManager NovaManagerClient;
        public Managers.LiveControl.Client.LiveControlManager LiveControlManagerClient;

        public NovaManager NovaManagerServer;
        public Managers.LiveControl.Server.LiveControlManager LiveControlManagerServer;


        //Hook Servers
        private InputSimulator inputSimulator;
        private BitmapImage BGBitmap;
        private Task updateImageThread;
        private Queue<Model.LiveControl.Screenshot> LiveShots;

        //Hook Client
        RamGecTools.MouseHook mouseHook = new RamGecTools.MouseHook();
        RamGecTools.KeyboardHook keyboardHook = new RamGecTools.KeyboardHook();
        private LayoutManager layout;
        private ServerManager serverManger;

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

 
        public AppStart()
        {
            InitializeComponent();
            screenImagePositionX = SystemParameters.WorkArea.Width;
            screenImagePositionY = SystemParameters.WorkArea.Height;

            Console.WriteLine("{0}, {1}", SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            Console.WriteLine("{0}, {1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }


        #region ServeSettings
        public async Task InitNetworkManagerServer()
        {

            NovaManagerServer = Managers.NovaServer.Instance.NovaManager;
            LiveControlManagerServer = Managers.NovaServer.Instance.LiveControlManager;

            LiveControlManagerServer.Provider.SetQualityParameters(Int32.Parse(MTUBox.Text),Int32.Parse(QualityBox.Text), Int32.Parse(ColorDepthBox.Text));

            inputSimulator = new InputSimulator();
            NovaManagerServer.OnIntroducerRegistrationResponded += NovaManager_OnIntroducerRegistrationResponded;
            NovaManagerServer.OnNewPasswordGenerated += new EventHandler<PasswordGeneratedEventArgs>(ServerManager_OnNewPasswordGenerated);
            NovaManagerServer.Network.OnConnected += new EventHandler<Network.ConnectedEventArgs>(Network_OnConnected);

            PasswordGeneratedEventArgs passArgs = await NovaManagerServer.GeneratePassword();
            //LabelPassword.Content = passArgs.NewPassword;
            IntroducerRegistrationResponsedEventArgs regArgs = await NovaManagerServer.RegisterWithIntroducerAsTask();
            //LabelNovaId.Content = regArgs.NovaId;
            Status.Content = "Ready to Connect";
        }

        // Network handshakes for HOST
        void Network_OnConnected(object sender, Network.ConnectedEventArgs e)
        {
            Status.Content = "Connected";

        }
        void ServerManager_OnNewPasswordGenerated(object sender, Providers.Nova.Modules.PasswordGeneratedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LabelPassword.Content = e.NewPassword;
            }));

        }
        private void NovaManager_OnIntroducerRegistrationResponded(object sender, Providers.Nova.Modules.IntroducerRegistrationResponsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LabelNovaId.Content = e.NovaId;
            }));

        }
        private async void startCapture_Click(object sender, RoutedEventArgs e)
        {
            //Start Server Network Registration
            await InitNetworkManagerServer();
            LiveControlManagerServer.OnMouseKeyboardEventReceived += LiveControlManagerServer_OnMouseKeyboardEventReceived;
        }
        void LiveControlManagerServer_OnMouseKeyboardEventReceived(object sender, Network.Messages.LiveControl.MouseKeyboardNotification e)
        {
            string msgRecvd = e.data;
            parseMessage(msgRecvd);
        }

   



        #endregion

        #region clientManagement

        public async Task InitNetworkManagerClient()
        {
            NovaManagerClient = Managers.NovaClient.Instance.NovaManager;
            LiveControlManagerClient = Managers.NovaClient.Instance.LiveControlManager;
            LiveControlManagerClient.Provider.SetQualityParameters(int.Parse(MTUBox.Text), int.Parse(QualityBox.Text), int.Parse(ColorDepthBox.Text));
            LiveControlManagerClient.OnScreenshotReceived += new EventHandler<ScreenshotMessageEventArgs>(LiveControlManager_OnScreenshotReceived);

            NovaManagerClient.OnIntroductionCompleted += new EventHandler<IntroducerIntroductionCompletedEventArgs>(ClientManager_OnIntroductionCompleted);
            NovaManagerClient.OnNatTraversalSucceeded += new EventHandler<Network.NatTraversedEventArgs>(ClientManager_OnNatTraversalSucceeded);
            NovaManagerClient.OnConnected += new EventHandler<ConnectedEventArgs>(ClientManager_OnConnected);

            ImageDivisor = LiveControlManagerClient.Provider.GetImageQualityDiv();
            mtu = LiveControlManagerClient.Provider.GetMTU();
            //LiveShots = new Queue<Model.LiveControl.Screenshot>();
            drawingVisual = new DrawingVisual();

        }
       
        void ClientManager_OnConnected(object sender, ConnectedEventArgs e)
        {
            //  ButtonConnect.Set(() => ButtonConnect.Text, "Connected.");
            remoteConnection.Content = "Connected to remote";
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
                            remoteConnection.Content = "Enter correct password/login";
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
            remoteConnection.Content = "Connecting to machine..";

        }

        public async void SetImage(Bitmap bitmap)
        {
            IntPtr pointer = bitmap.GetHbitmap();

            BGImage.Source = Imaging.CreateBitmapSourceFromHBitmap(pointer, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            //DeleteObject(pointer);
        }
     
        // Network handshakes for CLIENT
        private async void ConnectRemote_Click(object sender, RoutedEventArgs e)
        {
            await InitNetworkManagerClient();
            await NovaManagerClient.RequestIntroductionAsTask(RID.Text, PWD.Text);


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
            if (hostScreenWidth == 0 && hostScreenHeight == 0)
            {
                hostScreenWidth = (int)e.Screenshot.ScreenWidth;
                hostScreenHeight = (int)e.Screenshot.ScreenHeight;
                GoFullscreen();
            }
            UpdateRegion(screenshot);
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

                if ((int)bitmap.Height == hostScreenHeight / ImageDivisor && (int)bitmap.Width == hostScreenWidth / ImageDivisor)
                {
                    BGImage.Width = hostScreenWidth;
                    BGImage.Height = hostScreenHeight;
                    BGWritable = new WriteableBitmap((BitmapSource)bitmap);
                    buffer = new RenderTargetBitmap((int)BGWritable.Width, (int)BGWritable.Height, BGWritable.DpiX, BGWritable.DpiY, PixelFormats.Pbgra32);

                    var drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {

                        drawingContext.DrawImage(BGWritable, new Rect(0, 0, BGWritable.Width, BGWritable.Height));
                        // drawingContext.DrawImage(bitmap, new Rect(screenshot.Region.X, screenshot.Region.Y, screenshot.Region.Width, screenshot.Region.Height));
                        // drawingContext.DrawImage()  
                        //    drawingContext.DrawRectangle(new SolidColorBrush(Colors.Red), null,
                        //                      new Rect(screenshot.Region.X,screenshot.Region.Y,screenshot.Region.Width,screenshot.Region.Height));
                        //}
                    }
                    buffer.Render(drawingVisual);
                    //await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => BGImage.Source = buffer)));
                    BGImage.Source = buffer;


                }
                else
                {
                    //int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel + 7) / 8;
                    //int size = stride * bitmap.PixelHeight;
                    //byte[] bitmapByteArray = new byte[size];
                    //bitmap.CopyPixels(bitmapByteArray, 0, 0);
                    //var dirtyRectangle = new Int32Rect(screenshot.Region.X, screenshot.Region.Y, (Int32)bitmap.Width / ImageDivisor, (Int32)bitmap.Height / ImageDivisor);
                    ////BGWritable.AddDirtyRect(dirtyRectangle);
                    //BGWritable.Lock();
                    //BGWritable.WritePixels(new Int32Rect(screenshot.Region.X, screenshot.Region.Y, (Int32)bitmap.Width, (Int32)bitmap.Height),bitmapByteArray, stride, screenshot.Region.X, screenshot.Region.Y);
                    //BGWritable.Unlock();
                    //buffer =new RenderTargetBitmap((int)BGWritable.Width, (int)BGWritable.Height, BGWritable.DpiX,BGWritable.DpiY, PixelFormats.Pbgra32);
                    var drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {

                        //drawingContext.DrawImage(buffer, new Rect(0, 0, BGWritable.Width, BGWritable.Height));
                        drawingContext.DrawImage(bitmap, new Rect(screenshot.Region.X / ImageDivisor, screenshot.Region.Y / ImageDivisor, screenshot.Region.Width / ImageDivisor, screenshot.Region.Height / ImageDivisor));
                        // drawingContext.DrawImage()  
                        // drawingContext.DrawRectangle(new SolidColorBrush(Colors.Red), null,
                        //                      new Rect(screenshot.Region.X,screenshot.Region.Y,screenshot.Region.Width,screenshot.Region.Height));
                        //}
                    }
                    buffer.Render(drawingVisual);
                    //var img = new DrawingImage(drawingVisual.Drawing);

                    // var mergedBitmap = mergetwoBitmaps(BGWritable,new WriteableBitmap(bitmap), dirtyRectangle,stride);

                    //BGImage.Width = bitmap.Width;
                    //BGImage.Height = bitmap.Height;
                    //this.Dispatcher.Invoke(() => BGImage.Source = buffer);
                    BGImage.Source = buffer;
                    //await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => BGImage.Source = buffer)));
                    //BGWritable = new WriteableBitmap((BitmapSource)BGImage.Source);
                }

            }
        }

        private WriteableBitmap mergetwoBitmaps(WriteableBitmap bitmap1, WriteableBitmap bitmap2, Int32Rect rect, int stride)
        {
            //bitmap1 = new WriteableBitmap(new BitmapImage(new Uri("Koala.jpg", UriKind.Relative)));
            //bitmap2 = new WriteableBitmap(new BitmapImage(new Uri("Desert.jpg", UriKind.Relative)));

            //Get the pixel arrays of both bitmaps
            int width = bitmap1.PixelWidth;
            int height = bitmap1.PixelHeight;
            int stride2 = bitmap1.BackBufferStride;

            int[] pixels1 = new int[width * height];
            int[] pixels2 = new int[pixels1.Length];

            bitmap1.CopyPixels(pixels1, stride, 0);
            bitmap2.CopyPixels(pixels2, stride, 0);

            //Detect the rectangle that has difference
            //int top = 0, bottom = 0, left = 0, right = 0;
            //for (int i = 0; i < pixels1.Length; i++)
            //{
            //    if (pixels1[i] != pixels2[i])
            //    {
            //        int row = i / width;
            //        int col = i % width;
            //        top = Math.Min(top, row);
            //        bottom = Math.Max(bottom, row);
            //        left = Math.Min(left, col);
            //        right = Math.Max(right, col);
            //    }
            //}
            //Int32Rect rect = new Int32Rect(left, top, right - left + 1, bottom - top + 1);

            //Copy pixels of the different rectangle in the second bitmap into another array
            int[] pixelDiff = new int[rect.Width * rect.Height];
            for (int i = 0; i < rect.Width; i++)
            {
                for (int j = 0; j < rect.Height; j++)
                {
                    pixelDiff[i + j * rect.Width] = pixels2[rect.X + i + (rect.Y + j) * width];
                }
            }

            //Write the new pixels into the first bitmap
            bitmap1.WritePixels(rect, pixelDiff, stride * rect.Width / width, 0);
            return bitmap1;
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
                BGImage.Width = hostScreenWidth;
                BGImage.Height = hostScreenHeight;
                BGWritable = new WriteableBitmap((BitmapSource)bitmap);
                await Task.Factory.StartNew(() => Dispatcher.BeginInvoke((Action)(() => BGImage.Source = bitmap)));

            }
        }

        private void BGImage_IO(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            BGImage.MouseMove += BGImage_MouseMove;
            //ScrollView.Width = 900;
            //ScrollView.Height = 600;
            InstallKeyboard();
            
            //Questo bind vale solo mentre si è connessi
            bindHotkeyCommands();
           // GoFullscreen();

        }

        private async void MouseKeyboardIO_Checked(object sender, RoutedEventArgs e)
        {

            BGImage.MouseLeave += BGImage_MouseLeave;
            BGImage.MouseMove += BGImage_MouseMove;
            BGImage.MouseDown += BGImage_MouseDown;
            BGImage.MouseUp += BGImage_MouseUp;
            
            InstallKeyboard();
            bindHotkeyCommands();

        }

        private async void BGImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // throw new NotImplementedException();
            double Xabs = e.GetPosition(BGImage).X;
            double Yabs = e.GetPosition(BGImage).Y;
            double x = Math.Round((Xabs / hostScreenWidth), 2); //must send relative position REAL/RESOLUTION System.Windows.SystemParameters.PrimaryScreenHeigh
            double y = Math.Round((Yabs / hostScreenHeight), 2);

            double x1 = Math.Round((Xabs / System.Windows.SystemParameters.PrimaryScreenWidth), 4); //must send relative position REAL/RESOLUTION System.Windows.SystemParameters.PrimaryScreenHeigh
            double y2 = Math.Round((Yabs / System.Windows.SystemParameters.PrimaryScreenHeight), 4);

            //this.serverManger.sendMessage
             var eventMessage =  "C" + " " + "WM_LBUTTONUP" + " " + x + " " + y;
            LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage(eventMessage);
            Dispatcher.Invoke(new Action(() =>
            {
                MousePositionXLabel.Content = Xabs.ToString();
                MousePositionYLabel.Content = Yabs.ToString();
                MouseEventLabel.Content = eventMessage;
            }));
            Console.WriteLine("C" + " " + x + "WM_LBUTTONUP");

        }

        private async void BGImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double Xabs = e.GetPosition(BGImage).X;
            double Yabs = e.GetPosition(BGImage).Y;
            double x = Math.Round((Xabs / hostScreenWidth), 2); //must send relative position REAL/RESOLUTION System.Windows.SystemParameters.PrimaryScreenHeigh
            double y = Math.Round((Yabs / hostScreenHeight),2);

            double x1 = Math.Round((Xabs / System.Windows.SystemParameters.PrimaryScreenWidth), 4); //must send relative position REAL/RESOLUTION System.Windows.SystemParameters.PrimaryScreenHeigh
            double y2 = Math.Round((Yabs / System.Windows.SystemParameters.PrimaryScreenHeight), 4);
            // throw new NotImplementedException();
            string eventMessage = "C" + " " + "WM_LBUTTONDOWN" + " " + x + " " + y;
            LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage(eventMessage);
            Console.WriteLine("C" + " " + x + "WM_LBUTTONDOWN" );
            Dispatcher.Invoke(new Action(() =>
            {
                MousePositionXLabel.Content = Xabs.ToString();
                MousePositionYLabel.Content = Yabs.ToString();
                MouseEventLabel.Content = eventMessage;
            }));

        }
        
        async void  BGImage_MouseMove(object sender, MouseEventArgs e)
        {

            double Xabs = e.GetPosition(BGImage).X;
            double Yabs = e.GetPosition(BGImage).Y;
            double x = Math.Round((Xabs / hostScreenWidth),2); //must send relative position REAL/RESOLUTION System.Windows.SystemParameters.PrimaryScreenHeigh
            double y = Math.Round((Yabs / hostScreenHeight),2);

            double x1 = Math.Round((Xabs / System.Windows.SystemParameters.PrimaryScreenWidth), 4); //must send relative position REAL/RESOLUTION System.Windows.SystemParameters.PrimaryScreenHeigh
            double y2 = Math.Round((Yabs / System.Windows.SystemParameters.PrimaryScreenHeight), 4);

            //this.serverManger.sendMessage
            var eventMessage = ("M" + " " + x + " " + y);
            LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage(eventMessage);
            
             Dispatcher.Invoke(new Action(() =>
            {
                MousePositionXLabel.Content = Xabs.ToString();
                MousePositionYLabel.Content = Yabs.ToString();
                MouseEventLabel.Content = eventMessage;
            }));
        }

        private void BGImage_MouseLeave(object sender, MouseEventArgs e)
        {
            UnistallMouseAndKeyboard();
            unbindHotkeyCommands();
            MouseKeyboardIO.IsChecked = false;
        }

        private async void UpdateQualityBtn_Click(object sender, RoutedEventArgs e)
        {
            //string newMTU = MTUBox
            try
            {
                int newIQ = Int32.Parse(QualityBox.Text);

                await LiveControlManagerClient.Provider.ChangeScreenShareDynamics(250, newIQ);
                await LiveControlManagerClient.Provider.ChangeColorDepth(Int32.Parse(ColorDepthBox.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                myRasPhonebook = new RasPhoneBook();
                myRasPhonebook.Open();
                RasEntry entry = RasEntry.CreateVpnEntry("VPN_DXGI", "69.87.217.138", RasVpnStrategy.PptpOnly, RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn, false));
                if (!RasEntry.Exists("VPN_DXGI", myRasPhonebook.Path))
                    this.myRasPhonebook.Entries.Add(entry);

                myRasDialer = new RasDialer();
                myRasDialer.StateChanged += myRasDialer_StateChanged;
                myRasDialer.EntryName = "VPN_DXGI";
                myRasDialer.PhoneBookPath = null;
                myRasDialer.Credentials = new System.Net.NetworkCredential(vpnuserbox.Text, vpnpwdbox.Text);
                myRasDialer.PhoneBookPath = myRasPhonebook.Path;
                var phonbookpath = myRasDialer.PhoneBookPath;
                if (phonbookpath != null)
                {
                    //Dispatcher.BeginInvoke((Action)(() => { updateLogVPN(phonbookpath); }));
                    Debug.WriteLine("Path to phonebook for VPN Entry:: " + phonbookpath);
                    INIFile inif = new INIFile(phonbookpath);
                    inif.Write("VPN_DXGI", "IpPrioritizeRemote", "0");
                    var msg2 = inif.Read("VPN_DXGI", "IpPrioritizeRemote");
                    Debug.WriteLine("DefaultGateway =" + msg2);
                }
                myRasDialer.DialAsync();
                MessageBox.Show("VPN Tunneling enabled");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void myRasDialer_StateChanged(object sender, StateChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            myRasDialer.DialAsyncCancel();

        }

        private void fullscreen_Checked(object sender, RoutedEventArgs e)
        {
            GoFullscreen();
        }

        private void fullscreen_Unchecked(object sender, RoutedEventArgs e)
        {
            //back to originial size. 
        }

        private void GoFullscreen()
        {
            ScrollView.Height = gridkhaki.Height;
            ScrollView.Width = gridkhaki.Width;
        }

        #endregion

        #region Hooks

        #region HooksServer
        private void parseMessage(string buffer)
        {
            //Console.WriteLine("[ [" + buffer + "]");
            String[] commands = buffer.Split(' '); //split incoming message
            if (commands.GetValue(0).Equals("M")) //mouse movement
            {
                //16 bit è più veloce di 32
                int x = Convert.ToInt16(Double.Parse(commands[1]) * System.Windows.SystemParameters.PrimaryScreenWidth);
                int y = Convert.ToInt16(Double.Parse(commands[2]) * System.Windows.SystemParameters.PrimaryScreenHeight);
                NativeMethods.SetCursorPos(x, y);
                inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(x, y);
            }
            else if (commands.GetValue(0).ToString().Equals("W"))
            { //scroll
                int scroll = Convert.ToInt32(commands.GetValue(1).ToString());
                inputSimulator.Mouse.VerticalScroll(scroll);
            }
            else if (commands.GetValue(0).ToString().Equals("C")) //click
            {
                if (commands.GetValue(1).ToString().Equals("WM_LBUTTONDOWN"))
                {
                    inputSimulator.Mouse.LeftButtonDown();
                }
                else if (commands.GetValue(1).ToString().Equals("WM_LBUTTONUP"))
                {
                    inputSimulator.Mouse.LeftButtonUp();

                }
                else if (commands.GetValue(1).ToString().Equals("WM_RBUTTONDOWN"))
                {
                    inputSimulator.Mouse.RightButtonDown();
                }
                else if (commands.GetValue(1).ToString().Equals("WM_RBUTTONUP"))
                {
                    inputSimulator.Mouse.RightButtonUp();
                }

            }
            else if (commands.GetValue(0).ToString().Equals("K")) //keyboard
            {
                VirtualKeyCode vk = (VirtualKeyCode)Convert.ToInt32(commands.GetValue(1).ToString());
                if (commands.GetValue(2).ToString().Equals("DOWN"))
                {
                    inputSimulator.Keyboard.KeyDown(vk); //keydown
                }
                else if (commands.GetValue(2).ToString().Equals("UP"))
                {
                    inputSimulator.Keyboard.KeyUp(vk); //keyup
                }
            }
            else if (commands.GetValue(0).ToString().Equals("G")) //used as callback for the clipboard
            {
                Console.WriteLine("Received : GIMME CLIPBOARD");
                ClipboardManager cb = new ClipboardManager();
                //Thread cbSenderThread = new Thread(() =>
                //{
                //    Thread.CurrentThread.IsBackground = true;
                //    // connect the fly 
                //    if (this.clientCB != null)
                //        this.clientCB.Client.Close();
                //    this.clientCB = new TcpClient();
                //    this.clientCB.Connect(((IPEndPoint)this.client.Client.RemoteEndPoint).Address, 9898); //questo è il client che riceve
                //    cb.sendClipBoardFaster(this.clientCB);
                //});
                //cbSenderThread.SetApartmentState(ApartmentState.STA);
                //cbSenderThread.Start();
                //cbSenderThread.Join();
                //icon.ShowBalloonTip("Clipboard", "La clipboard è stata trasferita al client!", new Hardcodet.Wpf.TaskbarNotification.BalloonIcon());
            }


        }
        #endregion

        #region Core
        public void closeCommunication(object sender, ExecutedRoutedEventArgs e)
        {
            //Piccolo stratagemma x evitare che al server arrivino solo gli eventi KEYDOWN (che causerebberro problemi)
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LCONTROL + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LMENU + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.KEY_E + " " + "UP");
            UnistallMouseAndKeyboard();
            unbindHotkeyCommands(); //rimuovo vincoli su hotkeys
            this.serverManger.disconnect();

        }

        private void pauseCommunication(object sender, ExecutedRoutedEventArgs e)
        {
            //Piccolo stratagemma x evitare che al server arrivino solo gli eventi KEYDOWN (che causerebberro problemi)
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LCONTROL + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LMENU + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.KEY_P + " " + "UP");
            UnistallMouseAndKeyboard();
            unbindHotkeyCommands();

        }

        private void continueCommunication()
        {
            InstallKeyboard();
            bindHotkeyCommands();

        }

        public void closeOnException(String s)
        {
            //MessageBox.Show(s);
            UnistallMouseAndKeyboard();
            unbindHotkeyCommands(); //rimuovo vincoli su hotkeys
            this.serverManger.disconnect();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void gimmeClipboard(object sender, ExecutedRoutedEventArgs e)
        {
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LCONTROL + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LMENU + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.KEY_Z + " " + "UP");
            this.serverManger.sendMessage("G"); //this is the gimme message
        }

        private void switchToNextServer(object sender, ExecutedRoutedEventArgs e)
        {
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LCONTROL + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LMENU + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.KEY_N + " " + "UP");
            //chiamo il metodo che realmente switcha il puntatore al sender udp 
            this.serverManger.nextSelectedServers();
            //aggiorno la label in base ai risultati effettivi dell'operazione
            //lblMessages.Dispatcher.Invoke(DispatcherPriority.Background,
            //new Action(() => { lblMessages.Content = "Connesso al server : " + this.serverManger.selectedServers.ElementAt(this.serverManger.serverPointer).name; }));

        }
        public void sendClipboard(object sender, ExecutedRoutedEventArgs e)
        {
            //Piccolo stratagemma x evitare che al server arrivino solo gli eventi KEYDOWN (che causerebberro problemi)
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LCONTROL + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.LMENU + " " + "UP");
            this.serverManger.sendMessage("K" + " " + (int)RamGecTools.KeyboardHook.VKeys.KEY_X + " " + "UP");
            //this.serverManger.sendClipBoardFaster(this.serverManger.selectedServers.ElementAt(this.serverManger.serverPointer).CBClient);
        }

        #endregion

        #region HooksClient
        //TODO: passare un'oggetto al server in modo che questo possa eseguire azione
        void keyboardHook_KeyPress(int op, RamGecTools.KeyboardHook.VKeys key)
        {
            try
            {
                if (op == 0)
                {
                    //key is down

                    LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage("K" + " " + (int)key + " " + "DOWN");
                    Console.WriteLine("K" + " " + (int)key + " " + "DOWN");

                }
                else
                {
                    //key is up
                    //this.serverManger.sendMessage
                    LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage("K" + " " + (int)key + " " + "UP");
                    Console.WriteLine("K" + " " + (int)key + " " + "UP");
                }
            }
            catch (Exception ex)
            {
                closeOnException(ex.Message);
                MessageBox.Show("La connessione si è interrotta");

            }
        }

        /*
         Questo metodo è stato creato per il seguente motivo: il keyboard hooker fa in modo che gli hotkeys tipo alt-tab, non vadano al sistema operativo 
         * del client: in pratica è come se l'hooker si "mangiasse" gli eventi key_down, di conseguenza bisogna
         * generarli "artificialmente" in questo modo, per far sì che il server li riceva
         */
        void keyboardHook_HotKeyPress(int virtualKeyCode)
        {

            //this.serverManger.sendMessage
            LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage("K" + " " + (int)virtualKeyCode + " " + "DOWN");
            Console.WriteLine("K" + " " + (int)virtualKeyCode + " " + "DOWN");
        }

        void mouseHook_MouseEvent(int type, RamGecTools.MouseHook.MSLLHOOKSTRUCT mouse, RamGecTools.MouseHook.MouseMessages move)
        {
            switch (type)
            {
                case 0:  //mouse click
                    LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage("C" + " " + move.ToString());
                    Console.Write("C" + " " + move.ToString());

                    break;
                case 1: // Mouse movement


                    System.Windows.Point PointOnImage = BGImage.PointFromScreen((new System.Windows.Point(mouse.pt.x, mouse.pt.y)));

                    double x = Math.Round((PointOnImage.X / hostScreenWidth), 4); //must send relative position REAL/RESOLUTION
                    double y = Math.Round((PointOnImage.Y / hostScreenHeight), 4);
                    //this.serverManger.sendMessage
                    LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage("M" + " " + x.ToString() + " " + y.ToString());
                    Console.WriteLine("M" + " " + x + " " + y);
                    break;
                default:
                    break;
            }
        }

        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            LiveControlManagerClient.Provider.sendMouseKeyboardStateMessage("W" + " " + ((int)e.Delta / 120).ToString());
        }
        public KeyEventHandler wnd_KeyDown { get; set; }

        private void InstallKeyboard()
        {
            //Insatllo keyboard 
            keyboardHook.KeyPress += new RamGecTools.KeyboardHook.myKeyboardHookCallback(keyboardHook_KeyPress);
            //Questo qui sotto era un vecchio handler che usavo per i problemi degli shortcut, momentaneamente lascio commentato
            keyboardHook.HotKeyPress += new RamGecTools.KeyboardHook.myKeyboardHotkeyCallback(keyboardHook_HotKeyPress);
            keyboardHook.Install();
            //Installo Mouse
            //mouseHook.MouseEvent += new RamGecTools.MouseHook.myMouseHookCallback(mouseHook_MouseEvent);
            //mouseHook.Install();
            //this.MouseWheel += MouseWheelEventHandler;
        }

        private void UnistallMouseAndKeyboard()
        {
            keyboardHook.KeyPress -= new RamGecTools.KeyboardHook.myKeyboardHookCallback(keyboardHook_KeyPress);
            //mouseHook.MouseEvent -= new RamGecTools.MouseHook.myMouseHookCallback(mouseHook_MouseEvent);
            keyboardHook.Uninstall();
            //mouseHook.Uninstall();
            //this.MouseWheel += MouseWheelEventHandler;

        }


        private void bindHotkeyCommands()
        {
            try
            {
                //aggancio CTRL+ALT+P con pauseCommunication
                RoutedCommand pauseComm = new RoutedCommand();
                pauseComm.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Alt));
                CommandBindings.Add(new CommandBinding(pauseComm, pauseCommunication));
                //aggancio CTRL+ALT+E con closeCommunication
                RoutedCommand closeComm = new RoutedCommand();
                closeComm.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Alt));
                CommandBindings.Add(new CommandBinding(closeComm, closeCommunication));
                //aggancio CTRL+ALT+N per next server
                RoutedCommand nextServer = new RoutedCommand();
                nextServer.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Alt));
                CommandBindings.Add(new CommandBinding(nextServer, switchToNextServer));
                //aggancio CTRL+ALT+X per inviare mia clipboard
                RoutedCommand sendClipboardcmd = new RoutedCommand();
                sendClipboardcmd.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control | ModifierKeys.Alt));
                CommandBindings.Add(new CommandBinding(sendClipboardcmd, sendClipboard));
                //aggancio CTRL+ALT+Z per ricevere la clipboard dal server
                RoutedCommand gimmeClipboardcmd = new RoutedCommand();
                gimmeClipboardcmd.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control | ModifierKeys.Alt));
                CommandBindings.Add(new CommandBinding(gimmeClipboardcmd, gimmeClipboard));

            }
            catch (Exception e)
            {
                //MessageBox.Show("bindHotKeyCommands: " + e.Message);
                Application.Current.Shutdown();
            }
        }

        private void unbindHotkeyCommands()
        {
            try
            {
                CommandBindings.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot unbind : " + ex.Message);
            }
        }

        private void wnd_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true; //AVOID ALT F4
        }
        #endregion

        public enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208
        }
        #endregion

    }
}
