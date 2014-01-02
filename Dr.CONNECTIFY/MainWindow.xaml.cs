using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace My_Connectify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string pass = password.Visibility == Visibility.Visible ? password.Password : password_shadow.Text;
            if (name.Text == string.Empty || password.Password == string.Empty)
                Start_Animation(0, 1, 6000000, "Write network name and password then try again.", color.Stroke, true);
            else if (pass.Length < 8)
                Start_Animation(0, 1, 6000000, "Password length must be more than 7 characters.", color.Stroke, true);
            else if (pass.Length > 64)
                Start_Animation(0, 1, 6000000, "Password length must be less than 64 characters.", color.Stroke, true);
            else if (ContainNotASCII(pass))
                Start_Animation(0, 1, 6000000, "Password cannot contains a nonASCII characters.", color.Stroke, true);
            else
            {
                int x = (new Random((int)(DateTime.UtcNow.Ticks))).Next();
                try
                {
                    string command = "netsh wlan set hostednetwork mode=allow ssid=" + name.Text + " key=" + pass + "\n\rnetsh wlan start hostednetwork";
                    StreamWriter SW = new StreamWriter(File.Open(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Dr.Conectify" + x + ".bat", FileMode.OpenOrCreate));
                    SW.Write(command);
                    SW.Close();
                    ProcessStartInfo st = new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Dr.Conectify" + x + ".bat");
                    st.CreateNoWindow = true;
                    st.Verb = "runas";
                    Process create_network = new Process();
                    st.CreateNoWindow = true;
                    create_network.StartInfo = st;
                    st.WindowStyle = ProcessWindowStyle.Hidden;
                    create_network.Start();
                    create_network.WaitForExit();
                    Start_Animation(0, 1, 6000000, "Wireless network has been Added.", color.Fill, true);
                }
                catch (Exception ex)
                { Start_Animation(0, 1, 6000000, ex.Message + ".", color.Stroke, true); }
                try
                { File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Dr.Conectify" + x + ".bat"); }
                catch (Exception) { }
            }
        }

        private bool ContainNotASCII(string pass)
        {
            bool result = false;
            foreach(char i in pass)
                if (i > 127)
                    result = true;
            return result;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            string command = "/Cnetsh wlan stop hostednetwork";
            Process create_network = new Process();
            create_network.StartInfo.FileName = "cmd.exe";
            create_network.StartInfo.Arguments = command;
            create_network.StartInfo.Verb = "runas";
            create_network.StartInfo.CreateNoWindow = true;
            create_network.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                create_network.Start();
                create_network.WaitForExit();
                Start_Animation(0, 1, 6000000, "Wireless network has been Removed.", color.Fill, true);
            }
            catch (Exception ex) { Start_Animation(0, 6000000, 1, ex.Message + ".", color.Stroke, true); }
        }

        private void ShowHide_Click(object sender, RoutedEventArgs e)
        {
            if (password.Visibility == Visibility.Visible)
            {
                password.Visibility = Visibility.Collapsed;
                password_shadow.Visibility = Visibility.Visible;
                password_shadow.Text = password.Password;
                show_password.ToolTip = "Hide password";
            }
            else
            {
                password.Visibility = Visibility.Visible;
                password_shadow.Visibility = Visibility.Collapsed;
                password.Password = password_shadow.Text;
                show_password.ToolTip = "Show password";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new about().ShowDialog();
        }

        private void Start_Animation(float start, float end, long speed, string run_text, Brush background, bool worker)
        {
            back.Fill = background;
            DoubleAnimation animation1 = new DoubleAnimation(start, end, new Duration(new TimeSpan(speed)));
            text.BeginAnimation(StackPanel.OpacityProperty, animation1);
            DoubleAnimation animation2 = new DoubleAnimation(start, end, new Duration(new TimeSpan(speed)));
            back.BeginAnimation(StackPanel.OpacityProperty, animation2);
            run.Text = run_text;
            if (worker)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += bw_DoWork;
                bw.ProgressChanged += bw_ProgressChanged;
                bw.RunWorkerAsync();
            }
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Start_Animation(1, 0, 5000000, run.Text, back.Fill, false);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(4000);
            ((BackgroundWorker)sender).ReportProgress(0);
        }

        public bool IsRunningAsAdmin()
        {
            WindowsIdentity cur = WindowsIdentity.GetCurrent();
            foreach (IdentityReference role in cur.Groups)
            {
                if (role.IsValidTargetType(typeof(SecurityIdentifier)))
                {
                    SecurityIdentifier sid = (SecurityIdentifier)role.Translate(typeof(SecurityIdentifier));
                    if (sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) || sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid))
                        return true;
                }
            }
            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsRunningAsAdmin())
            {
                this.Title += " (Adminstrator)";
                go.Content = "Go ahead";
                stop.Content = "Stop network";
            }
        }
    }
}

//string strOutput = string.Empty;
//var stdOutput = new StringBuilder();
//create_network.OutputDataReceived += (sender1, args) => stdOutput.Append(args.Data);
//string stdError = string.Empty;
//create_network.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
//create_network.StartInfo.UseShellExecute = false;
//create_network.StartInfo.RedirectStandardError = true;
//create_network.StartInfo.RedirectStandardOutput = true;
//create_network.BeginOutputReadLine();
//stdError = create_network.StandardError.ReadToEnd();