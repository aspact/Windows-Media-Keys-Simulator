using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using IniParser;
using IniParser.Model;

namespace Windows_Media_Keys_Simulator
{
    public partial class WMKS : Form
    {
        #region Variables
        private globalKeyboardHook gkh = new globalKeyboardHook();
        private static bool isActive = false;
        #endregion

        //CTOR
        public WMKS()
        {
            //Initialize Components.
            InitializeComponent();

            //Hook all keyboard keys to the GlobalKeyboardHook.
            var c = Enum.GetValues(typeof(Keys));
            foreach (var x in c)
            {
                gkh.HookedKeys.Add((Keys)x);
            }
            gkh.KeyDown += Gkh_KeyDown;

            //Create and set configuration.
            if (!SetConfigKeys())
                Environment.Exit(0);

            if (Properties.Settings.Default.hook)
                gkh.hook();

            label3.Text = "Press <" + Properties.Settings.Default.keyHide + "> to show and hide this window";

            if (Properties.Settings.Default.startup)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        //Sets the key_config.ini keys to the Settings.settings.
        private bool SetConfigKeys()
        {
            //Check if file exists.
            if (!File.Exists(@"C:\WinMediaKeySim\key_config.ini"))
            {
                if (!CreateConfigFile())
                {
                    return false;
                }
            }

            //Trying to get the keys from the key_config.ini file and sets them to Settings.settings variables.
            try
            {
                FileIniDataParser parser = new FileIniDataParser();
                IniData data = parser.ReadFile(@"C:\WinMediaKeySim\key_config.ini");
                Properties.Settings.Default.keyPrevTrack = (Keys)Enum.Parse(typeof(Keys), data["Keys"]["Prev"], true);
                Properties.Settings.Default.keyNextTrack = (Keys)Enum.Parse(typeof(Keys), data["Keys"]["Next"], true);
                Properties.Settings.Default.keyResume = (Keys)Enum.Parse(typeof(Keys), data["Keys"]["Res"], true);
                Properties.Settings.Default.keyVolUp = (Keys)Enum.Parse(typeof(Keys), data["Keys"]["Up"], true);
                Properties.Settings.Default.keyVolDown = (Keys)Enum.Parse(typeof(Keys), data["Keys"]["Down"], true);
                Properties.Settings.Default.keyToggle = (Keys)Enum.Parse(typeof(Keys), data["Keys"]["Toggle"], true);
                Properties.Settings.Default.keyHide = (Keys)Enum.Parse(typeof(Keys), data["Keys"]["Hide"], true);
                Properties.Settings.Default.hook = bool.Parse(data["Settings"]["Hook"]);
                Properties.Settings.Default.startup = bool.Parse(data["Settings"]["Start"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            return true;
        }

        //Creates the key_config.ini file.
        private bool CreateConfigFile()
        {
            //Check if directory exists.
            if (!Directory.Exists(@"C:\WinMediaKeySim\"))
            {
                try { Directory.CreateDirectory(@":\WinMediaKeySim\"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Cannot create directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            //Trying to write the key_config.ini file.
            try
            {
                using (StreamWriter writer = new StreamWriter(@"C:\WinMediaKeySim\key_config.ini", false))
                {
                    writer.WriteLine("[Settings]");
                    writer.WriteLine("Hook=False");
                    writer.WriteLine("Start=False");
                    writer.WriteLine("");
                    writer.WriteLine("[Keys]");
                    writer.WriteLine("Prev=Left");
                    writer.WriteLine("Next=Right");
                    writer.WriteLine("Res=Down");
                    writer.WriteLine("Up=PageUp");
                    writer.WriteLine("Down=PageDown");
                    writer.WriteLine("Toggle=LControlKey");
                    writer.WriteLine("Hide=Home");
                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Cannot create config file.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            File.SetAttributes(@"C:\WinMediaKeySim\key_config.ini", FileAttributes.Hidden);
            return true;
        }

        //Catches the key code.
        private void Gkh_KeyDown(object sender, KeyEventArgs e)
        {
            //Checks if media toggle key is pressed and toggles it on or off.            
            if (e.KeyCode == Properties.Settings.Default.keyToggle)
            {
                IntPtr handleIcon = new IntPtr();
                if (!isActive)
                {
                    isActive = true;
                    notify.BalloonTipText = "Media Keys are activated!";
                    handleIcon = Properties.Resources.success.GetHicon();
                    notify.ShowBalloonTip(100);
                }
                else
                {
                    isActive = false;
                    notify.BalloonTipText = "Media Keys are disabled!";
                    handleIcon = Properties.Resources.error.GetHicon();
                    notify.ShowBalloonTip(100);
                }
                notify.Icon = Icon.FromHandle(handleIcon);
            }

            //Checks if the correct key is pressed.
            if (isActive)
            {
                if (e.KeyCode == Properties.Settings.Default.keyPrevTrack)
                    KeySimulations(0);

                if (e.KeyCode == Properties.Settings.Default.keyNextTrack)
                    KeySimulations(1);

                if (e.KeyCode == Properties.Settings.Default.keyResume)
                    KeySimulations(2);

                if (e.KeyCode == Properties.Settings.Default.keyVolUp)
                    KeySimulations(3);

                if (e.KeyCode == Properties.Settings.Default.keyVolDown)
                    KeySimulations(4);
            }

            if (e.KeyCode == Properties.Settings.Default.keyHide)
            {
                if (this.ShowInTaskbar == true)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                }
            }
        }

        /* Simulates the key pressed using InputSimulator.
         * cases:
         * 0=prev
         * 1=next
         * 2=res
         * 3=up
         * 4=down */        
        private void KeySimulations(int Simulation)
        {
            switch (Simulation)
            {
                case 0:
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);
                    return;
                case 1:
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);
                    return;
                case 2:
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
                    return;
                case 3:
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_UP);
                    return;
                case 4:
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_DOWN);
                    return;
            }
        }

        //Opens the Key Manager form.
        private void open_Click(object sender, EventArgs e)
        {
            gkh.unhook();
            keymanager keymgr = new keymanager();
            DialogResult result = keymgr.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                label3.Text = "Press <" + Properties.Settings.Default.keyHide + "> to show and hide this window";
                if (Properties.Settings.Default.hook)
                {
                    gkh.hook();
                }
            }
        }
    }
}
