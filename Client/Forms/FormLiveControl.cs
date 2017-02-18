﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Managers;
using Managers.LiveControl.Client;
using Managers.Nova.Client;
using Model.LiveControl;
using Network.Messages.LiveControl;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using HookerClient;

namespace Client.Forms
{
    public partial class FormLiveControl : Form
    {
        private bool ShowRegionOutlines = false;
        //private Pen pen = new Pen(Color.Magenta, 2.0f);

        public LiveControlManager LiveControlManager { get { return NovaClient.Instance.LiveControlManager; } }
     
        //Client Screen share 
        private int ImageDivisor = 1;
        private int mtu = 1;
        private int hostScreenWidth;
        private int hostScreenHeight;

        RamGecTools.MouseHook mouseHook = new RamGecTools.MouseHook();
        RamGecTools.KeyboardHook keyboardHook = new RamGecTools.KeyboardHook();
       

        public FormLiveControl()
        {
            LiveControlManager.OnScreenshotReceived += new EventHandler<Providers.LiveControl.Client.ScreenshotMessageEventArgs>(LiveControlManager_OnScreenshotReceived);

            InitializeComponent();
        }

        void LiveControlManager_OnScreenshotReceived(object sender, Providers.LiveControl.Client.ScreenshotMessageEventArgs e)
        {
            var screenshot = e.Screenshot;

            using (var stream = new MemoryStream(screenshot.Image))
            {

                System.Drawing.Image image = Image.FromStream(stream);
                //Application.DoEvents();
                //this.BackgroundImage = image;
                //if (ShowRegionOutlines)
                //{
                //    var gfx = gdiScreen1.CreateGraphics();
                //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y));
                //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y + e.Screenshot.Region.Y));
                //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y + e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y + e.Screenshot.Region.Y));
                //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y + e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y));
                //    gfx.Dispose();
                //}
                gdiScreen1.Draw(image, screenshot.Region);
            }

            //LiveControlManagerClient.RequestScreenshot();

            Trace.WriteLine(String.Format("Processed Image #{0}, Size: {1} KB", e.Screenshot.Number, GetKBFromBytes(e.Screenshot.Image.Length)));
        }

        private static float GetKBFromBytes(long bytes)
        {
            return (float)((float)bytes / (float)1024);
        }

        private static float GetTotalScreenshotsKB(List<Screenshot> screenshots)
        {
            float total = 0f;
            screenshots.ForEach((x) =>
            {
                total += GetKBFromBytes(x.Image.Length);
            });
            return total;
        }

        private void ButtonRequestScreenshot_Click(object sender, EventArgs e)
        {
            ButtonRequestScreenshot.Hide();
            LiveControlManager.RequestScreenshot();
        }

        
        #region HooksClient
        //TODO: passare un'oggetto al server in modo che questo possa eseguire azione
        void keyboardHook_KeyPress(int op, RamGecTools.KeyboardHook.VKeys key)
        {
            try
            {
                if (op == 0)
                {
                    //key is down

                    LiveControlManager.Provider.sendMouseKeyboardStateMessage("K" + " " + (int)key + " " + "DOWN");
                    Console.WriteLine("K" + " " + (int)key + " " + "DOWN");

                }
                else
                {
                    //key is up
                    //this.serverManger.sendMessage
                    LiveControlManager.Provider.sendMouseKeyboardStateMessage("K" + " " + (int)key + " " + "UP");
                    Console.WriteLine("K" + " " + (int)key + " " + "UP");
                }
            }
            catch (Exception ex)
            {
                //closeOnException(ex.Message);
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
            LiveControlManager.Provider.sendMouseKeyboardStateMessage("K" + " " + (int)virtualKeyCode + " " + "DOWN");
            Console.WriteLine("K" + " " + (int)virtualKeyCode + " " + "DOWN");
        }

        void mouseHook_MouseEvent(int type, RamGecTools.MouseHook.MSLLHOOKSTRUCT mouse, RamGecTools.MouseHook.MouseMessages move)
        {
            switch (type)
            {
                case 0:  //mouse click
                    LiveControlManager.Provider.sendMouseKeyboardStateMessage("C" + " " + move.ToString());
                    Console.Write("C" + " " + move.ToString());

                    break;
                case 1: // Mouse movement

                    //point2Screen
                    System.Windows.Point PointOnImage = BGImage.PointFromScreen((new System.Windows.Point(mouse.pt.x, mouse.pt.y)));

                    double x = Math.Round((PointOnImage.X / hostScreenWidth), 4); //must send relative position REAL/RESOLUTION
                    double y = Math.Round((PointOnImage.Y / hostScreenHeight), 4);
                    //this.serverManger.sendMessage
                    LiveControlManager.Provider.sendMouseKeyboardStateMessage("M" + " " + x.ToString() + " " + y.ToString());
                    Console.WriteLine("M" + " " + x + " " + y);
                    break;
                default:
                    break;
            }
        }

        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            LiveControlManager.Provider.sendMouseKeyboardStateMessage("W" + " " + ((int)e.Delta / 120).ToString());
        }
        public System.Windows.Input.KeyEventHandler wnd_KeyDown { get; set; }

        private void InstallKeyboard()
        {
            //Insatllo keyboard 
            keyboardHook.KeyPress += new RamGecTools.KeyboardHook.myKeyboardHookCallback(keyboardHook_KeyPress);
            //Questo qui sotto era un vecchio handler che usavo per i problemi degli shortcut, momentaneamente lascio commentato
            keyboardHook.HotKeyPress += new RamGecTools.KeyboardHook.myKeyboardHotkeyCallback(keyboardHook_HotKeyPress);
            keyboardHook.Install();
            //Installo Mouse
            mouseHook.MouseEvent += new RamGecTools.MouseHook.myMouseHookCallback(mouseHook_MouseEvent);
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

        private void wnd_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true; //AVOID ALT F4
        }
        #endregion

    }
}
