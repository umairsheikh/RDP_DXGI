using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Managers;
using Managers.LiveControl.Server;
using Managers.Nova.Server;
using Model.Extensions;
using Providers.Nova.Modules;
using System.Diagnostics;
using DotRas;
using System.Runtime.InteropServices;
using WindowsInput;
using Network.Messages.LiveControl;
using WindowsInput.Native;

namespace Server.Forms
{
    public partial class FormMain : Form
    {
        public NovaManager NovaManager { get { return NovaServer.Instance.NovaManager; } }
        public LiveControlManager LiveControlManager { get { return NovaServer.Instance.LiveControlManager; } }

        //Input Simulator 
        private InputSimulator inputSimulator;
        
        //VPN
        private DotRas.RasPhoneBook myRasPhonebook;
        private static DotRas.RasDialer myRasDialer;

        public FormMain()
        {
            NovaManager.OnIntroducerRegistrationResponded += NovaManager_OnIntroducerRegistrationResponded;
            NovaManager.OnNewPasswordGenerated += new EventHandler<PasswordGeneratedEventArgs>(ServerManager_OnNewPasswordGenerated);
            NovaManager.Network.OnConnected += new EventHandler<Network.ConnectedEventArgs>(Network_OnConnected);
            inputSimulator = new InputSimulator();
            InitializeComponent();
        }

        private async void FormMain_Shown(object sender, EventArgs e)
        {
            
            //CheckMirrorDriverExists();
        }

        private void CheckMirrorDriverExists()
        {
           
                /* This error is referring to:
                 
                while (deviceFound = EnumDisplayDevices(null, deviceIndex, ref device, 0))
                {
                    if (device.DeviceString == driverName)
                        break;
                    deviceIndex++;
                }

                if (!deviceFound) return false;

                   * in MirrorDriver.DesktopMirror.cs. Basically, it enumerates through all of your graphic providers, and it's looking for "DF Mirage Driver", and it can't find it. Check Device Manager to verify that it's been installed (it's under Device Manager -> Display Adapters -> Mirage Driver). If you see it there, most likely you simply have to restart your computer.
                     */

                var dialogResult = MessageBox.Show("You either don't have the DemoForge mirror driver installed, or you haven't restarted your computer after the installation of the mirror driver. Without a mirror driver, this application will not work. The mirror driver is responsible for notifying the application of any changed screen regions and passing the application bitmaps of those changed screen regions. Press 'Yes' to directly download the driver (you'll still have to install it after). You can visit the homepage if you'd like too: http://www.demoforge.com/dfmirage.htm", "Mirror Driver Not Installed", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (dialogResult == DialogResult.Yes)
                {
                    Process.Start("http://www.demoforge.com/tightvnc/dfmirage-setup-2.0.301.exe");
                }
            
        }

        private void Timer_KeepAlive_Tick(object sender, EventArgs e)
        {
            NovaManager.SendKeepAliveMessage();
        }  

        void Network_OnConnected(object sender, Network.ConnectedEventArgs e)
        {
            LabelStatus.Set(() => LabelStatus.Text, "Connected");
            Timer_KeepAlive.Enabled = false;
        }

        void ServerManager_OnNewPasswordGenerated(object sender, PasswordGeneratedEventArgs e)
        {
            LabelPassword.Set(() => LabelPassword.Text, e.NewPassword);
        }

        private void NovaManager_OnIntroducerRegistrationResponded(object sender, IntroducerRegistrationResponsedEventArgs e)
        {
            LabelNovaId.Set(() => LabelNovaId.Text, e.NovaId);
        }

       
        //Making VPN Connection
        private void OnConnectVPN(object sender, EventArgs e)
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
                myRasDialer.Credentials = new System.Net.NetworkCredential(VpnUserBox.Text, VPNPasswordBox.Text);
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

        //Notify VPN Connection
        private void myRasDialer_StateChanged(object sender, StateChangedEventArgs e)
        {
            StatusVPNLabel.Text = e.State.ToString();
        }

        //Disconnect VPN 
        private void DisconnectVPNBtn_Click(object sender, EventArgs e)
        {
            myRasDialer.DialAsyncCancel();
        }

        //UpdateColor Bits Per Pixels
        private void OnUpdateColor(object sender, EventArgs e)
        {
            //try
            //{
            //    int newIQ = Int32.Parse(QualityBox.Text);
            //    await LiveControlManagerClient.Provider.ChangeScreenShareDynamics(250, newIQ);
            //    await LiveControlManagerClient.Provider.ChangeColorDepth(Int32.Parse(ColorDepthBox.Text));
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }
        
        //start hosting Screen share
        private async void StartHosting(object sender, EventArgs e)
        {
            PasswordGeneratedEventArgs passArgs = await NovaManager.GeneratePassword();
            LabelPassword.Text = passArgs.NewPassword;
            IntroducerRegistrationResponsedEventArgs regArgs = await NovaManager.RegisterWithIntroducerAsTask();
            LabelNovaId.Text = regArgs.NovaId;


            LiveControlManager.OnMouseKeyboardEventReceived += LiveControlManagerServer_OnMouseKeyboardEventReceived;

        }

        //MouseKeyboard Hook event main
        private void LiveControlManagerServer_OnMouseKeyboardEventReceived(object sender, MouseKeyboardNotification e)
        {
            string msgRecvd = e.data;
            parseMessage(msgRecvd);
        }


        #region HooksServer
        private void parseMessage(string buffer)
        {
            //Console.WriteLine("[ [" + buffer + "]");
            String[] commands = buffer.Split(' '); //split incoming message
            if (commands.GetValue(0).Equals("M")) //mouse movement
            {
                //16 bit è più veloce di 32
                int x = Convert.ToInt16(Double.Parse(commands[1]) * Screen.PrimaryScreen.Bounds.Width);//System.Windows.SystemParameters.PrimaryScreenWidth);
                int y = Convert.ToInt16(Double.Parse(commands[2]) * Screen.PrimaryScreen.Bounds.Height);//System.Windows.SystemParameters.PrimaryScreenHeight);
                NativeMethods.SetCursorPos(x, y);
                inputSimulator.Mouse.MoveMouseTo(x, y);
            }
            else if (commands.GetValue(0).ToString().Equals("W"))
            { //scroll
                int scroll = Convert.ToInt32(commands.GetValue(1).ToString());
                inputSimulator.Mouse.VerticalScroll(scroll);
            }
            else if (commands.GetValue(0).ToString().Equals("C")) //click
            {
                Debug.WriteLine(commands.GetValue(0).ToString());
                inputSimulator.Mouse.LeftButtonDown();
                inputSimulator.Mouse.LeftButtonUp();
                //Debug.WriteLine(commands.GetValue(1).ToString());

                //if (commands.GetValue(1).ToString().Equals("WM_LBUTTONDOWN"))
                //{
                //    inputSimulator.Mouse.LeftButtonDown();
                //}
                //else if (commands.GetValue(1).ToString().Equals("WM_LBUTTONUP"))
                //{
                //    inputSimulator.Mouse.LeftButtonUp();

                //}
                //else if (commands.GetValue(1).ToString().Equals("WM_RBUTTONDOWN"))
                //{
                //    inputSimulator.Mouse.RightButtonDown();
                //}
                //else if (commands.GetValue(1).ToString().Equals("WM_RBUTTONUP"))
                //{
                //    inputSimulator.Mouse.RightButtonUp();
                //}

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
              //  ClipboardManager cb = new ClipboardManager();
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

    }
    class INIFile
    {
        private string filePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
        string key,
        string val,
        string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

        public INIFile(string filePath)
        {
            this.filePath = filePath;
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value.ToLower(), this.filePath);
        }

        public string Read(string section, string key)
        {
            StringBuilder SB = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", SB, 255, this.filePath);
            return SB.ToString();
        }

        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }
    }
    public partial class NativeMethods
    {
        /// Return Type: BOOL->int  
        ///X: int  
        ///Y: int  
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);

    }

}
