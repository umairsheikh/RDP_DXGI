using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Managers;
using Managers.Nova.Server;
using Managers.Nova.Client;
using Model.Extensions;
using Network;
using Network.Messages.Nova;
using Providers.Nova.Modules;
using System.Threading.Tasks;
using Managers.LiveControl.Server;
using System.Windows;

namespace Client.Forms
{
    public partial class FormConnect : Form
    {
        //Client Properties
        public Managers.Nova.Client.NovaManager NovaManagerClient { get { return NovaClient.Instance.NovaManager; } }


        //Server Properties
        public Managers.Nova.Server.NovaManager NovaManagerServer { get { return NovaServer.Instance.NovaManager; } }
        public LiveControlManager LiveControlManagerServer { get { return NovaServer.Instance.LiveControlManager; } }



        //VPN
        private DotRas.RasPhoneBook myRasPhonebook;
        private static DotRas.RasDialer myRasDialer;



        public FormConnect()
        {
            NovaManagerClient.OnIntroductionCompleted += new EventHandler<IntroducerIntroductionCompletedEventArgs>(ClientManager_OnIntroductionCompleted);
            NovaManagerClient.OnNatTraversalSucceeded += new EventHandler<NatTraversedEventArgs>(ClientManager_OnNatTraversalSucceeded);
            NovaManagerClient.OnConnected += new EventHandler<ConnectedEventArgs>(ClientManager_OnConnected);

            InitializeComponent();
        }

        private async void ButtonConnect_Click(object sender, EventArgs e)
        {
            await NovaManagerClient.RequestIntroductionAsTask(IdBox.Text, TextBox_Password.Text);
        }

        void ClientManager_OnConnected(object sender, ConnectedEventArgs e)
        {
            ButtonConnect.Set(() => ButtonConnect.Text, "Connected.");

            this.Dispose();
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
                            TextBox_Password.Set(() => TextBox_Password.Text, String.Empty); // clear the password box for re-entry
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
            ButtonConnect.Set(() => ButtonConnect.Text, "Connecting to " + IdBox.Text + " ...");
        }
        
        //HOST METHODS
        private async void hostButton_Click(object sender, EventArgs e)
        {
            //Start Server Network Registration
             InitNetworkManagerServer();
            //LiveControlManagerServer.OnMouseKeyboardEventReceived += LiveControlManagerServer_OnMouseKeyboardEventReceived;

        }

        public async void InitNetworkManagerServer()
        {
            //NovaManagerServer = Managers.NovaServer.Instance.NovaManager;
            //LiveControlManagerServer = Managers.NovaServer.Instance.LiveControlManager;

            ////inputSimulator = new InputSimulator();
            NovaManagerServer.OnIntroducerRegistrationResponded += NovaManager_OnIntroducerRegistrationResponded;
            NovaManagerServer.OnNewPasswordGenerated += new EventHandler<PasswordGeneratedEventArgs>(ServerManager_OnNewPasswordGenerated);
            NovaManagerServer.Network.OnConnected += new EventHandler<Network.ConnectedEventArgs>(Network_OnConnected);

            PasswordGeneratedEventArgs passArgs = await NovaManagerServer.GeneratePassword();
            TextBox_Password.Text = passArgs.NewPassword;
            IntroducerRegistrationResponsedEventArgs regArgs = await NovaManagerServer.RegisterWithIntroducerAsTask();
            IdBox.Text = regArgs.NovaId;
            statusUpdateBox.Text = "Host is live";
            //Status.Content = "Ready to Connect";
        }

        // Network handshakes for HOST
        void Network_OnConnected(object sender, Network.ConnectedEventArgs e)
        {
            statusUpdateBox.Text = statusUpdateBox.Text + "\n Connected";
        }

        void ServerManager_OnNewPasswordGenerated(object sender, Providers.Nova.Modules.PasswordGeneratedEventArgs e)
        {
            TextBox_Password.Text = e.NewPassword;
        }

        private void NovaManager_OnIntroducerRegistrationResponded(object sender, Providers.Nova.Modules.IntroducerRegistrationResponsedEventArgs e)
        {
            IdBox.Text = e.NovaId;
        }

        private  void startCapture_Click(object sender, RoutedEventArgs e)
        {
            //Start Server Network Registration
             InitNetworkManagerServer();
            //LiveControlManagerServer.OnMouseKeyboardEventReceived += LiveControlManagerServer_OnMouseKeyboardEventReceived;


        }

        private void VPNConnect_Click(object sender, EventArgs e)
        {

        }
    }
}
