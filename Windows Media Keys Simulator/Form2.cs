using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using IniParser;
using IniParser.Model;

namespace Windows_Media_Keys_Simulator
{
    public partial class keymanager : Form
    {
        #region Variables
        private Properties.Settings settings = Properties.Settings.Default;
        #endregion

        //CTOR
        public keymanager()
        {
            //Initializes Components.
            InitializeComponent();

            //Adds all available keys to the ComboBoxes.
            var c = Enum.GetValues(typeof(Keys));
            foreach (var x in c)
            {
                prev.Items.Add(x);
                next.Items.Add(x);
                resume.Items.Add(x);
                volup.Items.Add(x);
                voldown.Items.Add(x);
                toggle.Items.Add(x);
                hide.Items.Add(x);
            }

            checkHook.Checked = Properties.Settings.Default.hook;
            checkHook.CheckStateChanged += CheckStateChanged;
            checkStartup.Checked = Properties.Settings.Default.startup;
            checkStartup.CheckStateChanged += CheckStateChanged;
            SetCombo(prev, settings.keyPrevTrack);
            SetCombo(next, settings.keyNextTrack);
            SetCombo(resume, settings.keyResume);
            SetCombo(volup, settings.keyVolUp);
            SetCombo(voldown, settings.keyVolDown);
            SetCombo(toggle, settings.keyToggle);
            SetCombo(hide, settings.keyHide);
        }

        //Re-create the key_config.ini.
        private void RewriteConfigFile()
        {
            if (File.Exists(@"C:\WinMediaKeySim\key_config.ini"))
                File.Delete(@"C:\WinMediaKeySim\key_config.ini");

            using (StreamWriter writer = new StreamWriter(@"C:\WinMediaKeySim\key_config.ini", false))
            {
                writer.WriteLine("[Settings]");
                writer.WriteLine("Hook=" + checkHook.Checked);
                writer.WriteLine("Start=" + checkStartup.Checked);
                writer.WriteLine("");
                writer.WriteLine("[Keys]");
                writer.WriteLine("Prev=" + prev.SelectedItem.ToString());
                writer.WriteLine("Next=" + next.SelectedItem.ToString());
                writer.WriteLine("Res=" + resume.SelectedItem.ToString());
                writer.WriteLine("Up=" + volup.SelectedItem.ToString());
                writer.WriteLine("Down=" + voldown.SelectedItem.ToString());
                writer.WriteLine("Toggle=" + toggle.SelectedItem.ToString());
                writer.WriteLine("Hide=" + hide.SelectedItem.ToString());
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
        }

        //Reads the key_config.ini.
        private void ReadConfigFile()
        {
            File.SetAttributes(@"C:\WinMediaKeySim\key_config.ini", FileAttributes.Hidden);

            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(@"C:\WinMediaKeySim\key_config.ini");
            settings.keyPrevTrack = ChangeKey(data["Keys"]["Prev"]);
            settings.keyNextTrack = ChangeKey(data["Keys"]["Next"]);
            settings.keyResume = ChangeKey(data["Keys"]["Res"]);
            settings.keyVolUp = ChangeKey(data["Keys"]["Up"]);
            settings.keyVolDown = ChangeKey(data["Keys"]["Down"]);
            settings.keyToggle = ChangeKey(data["Keys"]["Toggle"]);
            settings.keyHide = ChangeKey(data["Keys"]["Hide"]);
            settings.hook = bool.Parse(data["Settings"]["Hook"]);
            settings.startup = bool.Parse(data["Settings"]["Start"]);
        }

        //Writes a new key_config.ini when triggered.
        //Triggers when Checkstate changes.
        private void CheckStateChanged(object sender, EventArgs e)
        {
            RewriteConfigFile();
            ReadConfigFile();
        }

        //Sets the ComboBox to the current key.
        private void SetCombo(ComboBox c, Keys k)
        {
            c.SelectedIndex = c.Items.IndexOf(k);
            c.SelectedIndexChanged += SelectedIndexChanged;
        }

        //Changes a specific settings key.
        private Keys ChangeKey(string keySearch)
        {
            return (Keys)Enum.Parse(typeof(Keys), keySearch, true);
        }

        //Writes a new key_config.ini file when triggered.
        //Triggers when index of a ComboBox changes.
        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            //Check if keys are only one time used.
            List<string> keyCompare = new List<string>();
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is ComboBox)
                {
                    ComboBox c = (ComboBox)ctrl;
                    if (!keyCompare.Contains(c.SelectedItem.ToString()))
                    {
                        keyCompare.Add(c.SelectedItem.ToString());
                    }
                    else
                    {
                        MessageBox.Show("A key can only be assigned once.");
                        prev.SelectedIndex = prev.Items.IndexOf(settings.keyPrevTrack);
                        next.SelectedIndex = next.Items.IndexOf(settings.keyNextTrack);
                        resume.SelectedIndex = resume.Items.IndexOf(settings.keyResume);
                        volup.SelectedIndex = volup.Items.IndexOf(settings.keyVolUp);
                        voldown.SelectedIndex = voldown.Items.IndexOf(settings.keyVolDown);
                        toggle.SelectedIndex = toggle.Items.IndexOf(settings.keyToggle);
                        hide.SelectedIndex = hide.Items.IndexOf(settings.keyHide);
                        return;
                    }
                }
            }

            RewriteConfigFile();
            ReadConfigFile();
        }
    }
}
