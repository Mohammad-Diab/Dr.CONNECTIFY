using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Dr_Connectify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!IsRunningAsAdmin())
            {
                try
                {
                    Process h = new Process();
                    h.StartInfo.Verb = "runas";
                    h.StartInfo.FileName = Environment.CommandLine.Replace("\"", "");
                    h.Start();
                    Close();
                }
                catch (Exception)
                { MessageBox.Show("You must run this app as administrator.\n", "Dr.Connectify", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
            else
                Title += " (Adminstrator)";
            Refresh();
        }

        bool refresh_list = true, new_pass, auto_inc, continue_sweep = true;

        string[] hints = { "Need new features: just contact us on our email.", "Version 2 is ready, we are working on 3", "New version, new features.", "Its version 2, we believe that you will love it.", "Welcome to version 2.", "Version 2 errors free, and more compatibility.", "Version 2, many buttons wait for you to discover.", "Don't forget to enable internet sharing after creating network.", "Version 2 powered by Dr.Mohammad.", "Version 2, fast and amazing.", "Version 2 says: Hello World.", "To controll by keyboard, use Left & Right arrow, F2, F1, Escape and Tap." };
        Brush[] brushes = { Brushes.Blue, Brushes.DarkCyan, Brushes.Red, Brushes.RoyalBlue, Brushes.ForestGreen, Brushes.Chocolate, Brushes.Purple, Brushes.Firebrick, Brushes.LightSlateGray, Brushes.YellowGreen, Brushes.DarkRed, Brushes.Teal };
        bool can_continue = false;
        bool CanContinue
        {
            get { return can_continue; }
            set
            {
                can_continue = value;
                if (!value)
                {
                    NetworkStarted = false;
                    total_clients.Text = "Total Clients = 0";
                    client_list.Items.Clear();
                }
                // start_stop_grid.IsEnabled = value;
            }
        }

        bool network_started = false;
        bool NetworkStarted
        {
            get { return network_started; }
            set
            {
                network_started = value;
                ThicknessAnimation go_animation;
                ThicknessAnimation stop_animation;
                if (value)
                {
                    state2.Text = state.Text = "Activate";
                    background_color.Color = Colors.ForestGreen;
                    state2.Foreground = state.Foreground = Brushes.ForestGreen;
                    go_animation = new ThicknessAnimation(new Thickness(-320, 5, 0, 0), new Duration(new TimeSpan(2000000)));
                    stop_animation = new ThicknessAnimation(new Thickness(80, 5, 0, 0), new Duration(new TimeSpan(2000000)));
                    RefreshClientsList_Checked(null, null);
                    goto_clients.Visibility = Visibility.Visible;
                }
                else
                {
                    state2.Text = state.Text = "Deactivate";
                    state2.Foreground = state.Foreground = Brushes.Tomato;
                    background_color.Color = Colors.Tomato;
                    go_animation = new ThicknessAnimation(new Thickness(80, 5, 0, 0), new Duration(new TimeSpan(2000000)));
                    stop_animation = new ThicknessAnimation(new Thickness(400, 5, 0, 0), new Duration(new TimeSpan(2000000)));
                    goto_clients.Visibility = Visibility.Collapsed;
                }
                go.BeginAnimation(MarginProperty, go_animation);
                stop.BeginAnimation(MarginProperty, stop_animation);
            }
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += FirstStartup_DoWork;
            bw.ProgressChanged += FirstStartup_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
            CheckDrivers();
        }

        private void FirstStartup_DoWork(object sender, DoWorkEventArgs e)
        {
            ((BackgroundWorker)sender).ReportProgress(0);
            Thread.Sleep(3650);
        }

        private void FirstStartup_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int hint = new Random((int)DateTime.Now.Ticks).Next(hints.Length);
            int color = new Random((int)DateTime.Now.Ticks).Next(brushes.Length);
            // hi.Foreground = brushes[color];
            // hi.Text = hints[hint];
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (continue_sweep)
            {
                continue_sweep = false;
                ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0), new Duration(new TimeSpan(2780000)));
                mother.BeginAnimation(MarginProperty, ta);
            }
        }

        void CheckDrivers()
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                psi.Verb = "runas";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                process.StartInfo = psi;
                process.Start();
                process.StandardInput.WriteLine("netsh wlan show hostednetwork");
                process.StandardInput.WriteLine("exit");
                while (!process.StandardOutput.EndOfStream)
                {
                    string x = process.StandardOutput.ReadLine();
                    if (x.Length > 9 && x.Substring(0, 10).ToLower().Contains("status"))
                    {
                        if (x.ToLower().Contains("started"))
                            NetworkStarted = true;
                        break;
                    }
                }
                process.Start();
                process.StandardInput.WriteLine("netsh wlan show drivers");
                process.StandardInput.WriteLine("exit");
                while (!process.StandardOutput.EndOfStream)
                {
                    string x = process.StandardOutput.ReadLine();
                    if (x.ToLower().Contains("there is no wireless interface on the system"))
                    {
                        Start_Animation(0, 1, 4000000, "There is no wireless device on this system.", color.Stroke, true);
                        CanContinue = false;
                        return;
                    }
                    if (x.ToLower().Contains("hosted network supported"))
                        if (x.ToLower().Contains("yes"))
                        {
                            CanContinue = true;
                            return;
                        }
                }
                Start_Animation(0, 1, 4000000, "The wireless device isn't support hosted network.", color.Stroke, true);
                CanContinue = false;
            }
            catch (Exception ex)
            {
                can_continue = false;
                Start_Animation(0, 1, 4000000, ex.Message, color.Stroke, true);
            }
        }

        void Add_Click(object sender, RoutedEventArgs e)
        {
            if (CanContinue && !NetworkStarted)
            {
                string pass;
                if (new_pass)
                    pass = GenerateNewPassword();
                if (auto_inc)
                    name.Text = Properties.Settings.Default.network_name + (Properties.Settings.Default.count + 1);
                pass = password.Visibility == Visibility.Visible ? password.Password : password_shadow.Text;
                if (name.Text == string.Empty || password.Password == string.Empty) ShuckWindow("Write network name and password then try again.");
                else if (pass.Length < 8) ShuckWindow("Password length must be more than 7 characters.");
                else if (pass.Length > 64) ShuckWindow("Password length must be less than 64 characters.");
                else if (ContainNotASCII(pass)) ShuckWindow("Password cannot contains a nonASCII characters.");
                else
                {
                    try
                    {
                        Process process = new Process();
                        ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                        psi.RedirectStandardOutput = true;
                        psi.RedirectStandardInput = true;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        process.StartInfo = psi;
                        process.StartInfo.Verb = "runas";
                        process.Start();
                        process.StandardInput.WriteLine("netsh wlan set hostednetwork mode=allow ssid=" + name.Text + " key=" + pass);
                        process.StandardInput.WriteLine("netsh wlan start hostednetwork");
                        process.StandardInput.WriteLine("exit");
                        while (!process.StandardOutput.EndOfStream)
                        {
                            string x = process.StandardOutput.ReadLine();
                            if (x.ToLower().Contains("the hosted network started"))
                            {
                                NetworkStarted = true;
                                Start_Animation(0, 1, 4000000, "Wireless network has been Added.", color.Fill, true);
                                if (auto_inc) Properties.Settings.Default.count++;
                                Properties.Settings.Default.Save();
                                break;
                            }
                            if (x.ToLower().Contains("the hosted network couldn't be started."))
                            {
                                if (!process.StandardOutput.EndOfStream)
                                {
                                    string y = process.StandardOutput.ReadLine();
                                    ShuckWindow(y);
                                }
                                break;
                            }
                            if (x.ToLower().Contains("you must run this command from a command prompt with administrator privilege"))
                            {
                                ShuckWindow("you must run this app as adminstrator.");
                                break;
                            }
                        }
                    }
                    catch (Exception ex) { Start_Animation(0, 1, 4000000, ex.Message, color.Stroke, true); }
                }
            }
            else
                ShuckWindow("The hosted network couldn't be started."); 
            }

        private string GenerateNewPassword()
        {
            string password_text = "";
            char[] c = { ' ' };
            var matrix = Properties.Settings.Default.chars.Split(c, StringSplitOptions.RemoveEmptyEntries);
            List<string> returned_list = new List<string>();
            foreach (var i in matrix)
                if (i.Length == 1)
                    if (i[0] > 32 && i[0] < 128)
                        if (!returned_list.Contains(i))
                            returned_list.Add(i);
            Random rand = new Random((int)DateTime.Now.Ticks);
            int tall = rand.Next(8, 16);
            if (returned_list.Count > 0)
                for (int i = 0; i < tall; i++)
                    password_text += returned_list[rand.Next(returned_list.Count)];
            else
                password_text = "00000000";
            password_shadow.Text = password.Password = password_text;
            return password_text;
        }

        private bool ContainNotASCII(string pass)
        {
            bool result = false;
            foreach (char i in pass)
                if (i > 32 && i < 128)
                    result = true;
            return result;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkStarted)
            {
                try
                {
                    Process process = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                    psi.Verb = "runas";
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardInput = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    process.StartInfo = psi;
                    process.Start();
                    process.StandardInput.WriteLine("netsh wlan stop hostednetwork");
                    process.StandardInput.WriteLine("exit");
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string x = process.StandardOutput.ReadLine();
                        if (x.ToLower().Contains("the hosted network stopped"))
                        {
                            NetworkStarted = false;
                            Start_Animation(0, 1, 4000000, "Wireless network has been Stoped.", color.Fill, true);
                            break;
                        }
                        if (x.ToLower().Contains("the hosted network couldn't be stoped."))
                        {
                            if (!process.StandardOutput.EndOfStream)
                            {
                                string y = process.StandardOutput.ReadLine();
                                ShuckWindow(y);
                            }
                        }
                        if (x.ToLower().Contains("you must run this command from a command prompt with administrator privilege"))
                        {
                            ShuckWindow("you must run this app as adminstrator.");
                            break;
                        }
                    }

                }
                catch (Exception ex) { ShuckWindow(ex.Message); }
            }
        }

        private void Start_Animation(float start, float end, long speed, string run_text, Brush background, bool worker)
        {
            back.Fill = background;
            DoubleAnimation animation1 = new DoubleAnimation(start, end, new Duration(new TimeSpan(speed)));
            text.BeginAnimation(StackPanel.OpacityProperty, animation1);
            DoubleAnimation animation2 = new DoubleAnimation(start, end, new Duration(new TimeSpan(speed)));
            back.BeginAnimation(StackPanel.OpacityProperty, animation2);
            back.ToolTip = run.ToolTip = run.Text = run_text;
            if (worker)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += bw_DoWork;
                bw.ProgressChanged += bw_ProgressChanged;
                bw.RunWorkerAsync();
            }
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

        private void About_Click(object sender, RoutedEventArgs e)
        {
            new about().ShowDialog();
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


        #region Main Menu Apperance
        private void FirstGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            // if (main_menu.Margin.Left < -60)
            {
                ThicknessAnimation animation = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(1500000)));
                // main_menu.BeginAnimation(MarginProperty, animation);
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += ShowMenu_DoWork;
                bw.ProgressChanged += ShowMenu_ProgressChanged;
                bw.WorkerReportsProgress = true;
                bw.RunWorkerAsync();
            }
        }

        void ShowMenu_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            while (i < 4)
            {
                ((BackgroundWorker)sender).ReportProgress(i++);
                Thread.Sleep(80);
            }
        }

        void ShowMenu_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // ((Button)main_menu_stack.Children[e.ProgressPercentage]).Background = null;
            // ThicknessAnimation animation = new ThicknessAnimation(new Thickness(0, ((Button)main_menu_stack.Children[e.ProgressPercentage]).Margin.Top, 0, 0), new Duration(new TimeSpan(750000)));
            // main_menu_stack.Children[e.ProgressPercentage].BeginAnimation(Button.MarginProperty, animation);
        }

        void MainMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            // if (main_menu.Margin.Left > -4)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += HideMenu_DoWork;
                bw.ProgressChanged += HideMenu_ProgressChanged;
                bw.WorkerReportsProgress = true;
                bw.RunWorkerAsync();
                bw.RunWorkerCompleted += HideMenu_RunWorkerCompleted;
            }
        }

        void HideMenu_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 4;
            Thread.Sleep(200);
            while (i > 0)
            {
                ((BackgroundWorker)sender).ReportProgress(--i);
                Thread.Sleep(55);
            }
        }

        void HideMenu_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // ThicknessAnimation animation = new ThicknessAnimation(new Thickness(-128, ((Button)main_menu_stack.Children[e.ProgressPercentage]).Margin.Top, 0, 0), new Duration(new TimeSpan(750000)));
            // main_menu_stack.Children[e.ProgressPercentage].BeginAnimation(Button.MarginProperty, animation);
        }

        void HideMenu_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // ThicknessAnimation animation = new ThicknessAnimation(new Thickness(-64, 0, 0, 0), new Duration(new TimeSpan(750000)));
            // main_menu.BeginAnimation(Grid.MarginProperty, animation);
        }

        void RoundButton_MouseOver(object sender, MouseEventArgs e)
        {
            BackgroundWorker BW = new BackgroundWorker();
            BW.DoWork += RoundButtonColor_DoWork;
            BW.ProgressChanged += RoundButtonColor_ProgressChanged;
            BW.WorkerReportsProgress = true;
            BW.RunWorkerAsync(((Grid)sender).Children[0]);
        }

        void RoundButtonColor_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker BW = (BackgroundWorker)sender;
            for (int b = 0; b < 200; b += 10)
            {
                BW.ReportProgress(b, e.Argument);
                Thread.Sleep(10);
            }
        }

        void RoundButtonColor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            byte i = (byte)e.ProgressPercentage;
            ((Ellipse)e.UserState).Fill = new SolidColorBrush(Color.FromArgb((byte)(5 * (i / 4)), 0, (byte)(3 * i / 4), i));
        }

        void RoundButton_MouseLeave(object sender, MouseEventArgs e)
        {
            BackgroundWorker BW = new BackgroundWorker();
            BW.DoWork += RoundButtonRecolor_DoWork;
            BW.ProgressChanged += RoundButtonRecolor_ProgressChanged;
            BW.WorkerReportsProgress = true;
            BW.RunWorkerAsync((((Grid)sender).Children[0]));
        }

        void RoundButtonRecolor_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker BW = (BackgroundWorker)sender;
            for (int b = 200; b > 0; b -= 10)
            {
                BW.ReportProgress(b, e.Argument);
                Thread.Sleep(10);
            }
        }

        void RoundButtonRecolor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            byte i = (byte)e.ProgressPercentage;
            ((Ellipse)e.UserState).Fill = new SolidColorBrush(Color.FromArgb((byte)(5 * (i / 4)), 0, (byte)(3 * i / 4), i));
        }

        void RoundButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Ellipse)((Grid)sender).Children[0]).Fill = new SolidColorBrush(Color.FromArgb(255, 15, 100, 150));
        }

        void RoundButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Ellipse)((Grid)sender).Children[0]).Fill = new SolidColorBrush(Color.FromArgb(255, 0, 150, 200));
        }

        #endregion

        private void NetworkTap_Click(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(2000000)));
            mother.BeginAnimation(MarginProperty, ta);
        }

        private void ClientTap_Click(object sender, RoutedEventArgs e)
        {
            continue_sweep = false;
            ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-380, 0, 0, 0), new Duration(new TimeSpan(2000000)));
            mother.BeginAnimation(MarginProperty, ta);
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            new Options().ShowDialog();
            Refresh();
        }

        private void Refresh()
        {
            name.Text = Properties.Settings.Default.network_name + (auto_inc ? Properties.Settings.Default.count.ToString() : "");
            password.Password = password_shadow.Text = Properties.Settings.Default.password;
            new_pass = Properties.Settings.Default.new_password;
            auto_inc = Properties.Settings.Default.auto_increase;
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            CheckDrivers();
        }

        private void RefreshClientsList_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
                refresh_list = true;
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;
            while (NetworkStarted && refresh_list)
            {
                try
                {
                    Process process = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                    psi.Verb = "runas";
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardInput = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    process.StartInfo = psi;
                    process.Start();
                    process.StandardInput.WriteLine("netsh wlan show hostednetwork");
                    process.StandardInput.WriteLine("exit");
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string x = process.StandardOutput.ReadLine();
                        if (x.Length > 9 && x.Substring(0, 10).ToLower().Contains("status"))
                        {
                            if (x.ToLower().Contains("not available"))
                                bw.ReportProgress(1);
                            else if (x.ToLower().Contains("not started"))
                                bw.ReportProgress(2);
                            else if (x.ToLower().Contains("started"))
                            {
                                string client_count;
                                while (!(client_count = process.StandardOutput.ReadLine()).ToLower().Contains("number of clients")) ;
                                client_count = client_count.Substring(29);
                                int i;
                                List<string> clients = new List<string>();
                                int.TryParse(client_count, out i);
                                for (; i > 0; i--)
                                    try
                                    { clients.Add(process.StandardOutput.ReadLine().Substring(8, 17).ToUpper()); }
                                    catch (Exception) { }
                                bw.ReportProgress(0, clients);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    can_continue = false;
                    Start_Animation(0, 1, 4000000, "Can not refresh client list.", color.Stroke, true);
                }
                Thread.Sleep(6000);
            }
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    client_list.Items.Clear();
                    List<string> clients = (List<string>)e.UserState;
                    total_clients.Text = "Total Clients = " + clients.Count;
                    foreach (string i in clients)
                        client_list.Items.Add(i);
                    break;
                case 1:
                    CanContinue = false;
                    break;
                case 2:
                    NetworkStarted = false;
                    break;
            }
        }

        private void RefreshClientsList_Unchecked(object sender, RoutedEventArgs e)
        {
            refresh_list = false;
        }

        private void WindowKeyControll_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Right:
                    if (continue_sweep)
                        NetworkTap_Click(null, null);
                    else
                        if ((!name.IsFocused) || name.IsFocused && name.Text.Equals(string.Empty))
                        if (password.Visibility.Equals(Visibility.Visible) ? (!password.IsFocused) || password.IsFocused && password.Password.Equals(string.Empty) : (!password_shadow.IsFocused) || password_shadow.IsFocused && password_shadow.Text.Equals(string.Empty))
                            ClientTap_Click(null, null);
                    break;
                case Key.Left:
                    NetworkTap_Click(null, null);
                    break;
                case Key.F1:
                    About_Click(null, null);
                    break;
                case Key.F2:
                    Options_Click(null, null);
                    break;
                case Key.Escape:
                    go.Focus();
                    break;
            }
        }

        private void GeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            GenerateNewPassword();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            // glind_image.Opacity = 0;
        }

        private void GlintGrid_MouceEnter(object sender, MouseEventArgs e)
        {
            // glind_image.Opacity = (9 - glind_mother.Children.IndexOf((Grid)sender)) * 0.11;
        }


        #region Shuck Window Functions
        double old_left;
        string message;
        public void ShuckWindow(string msg)
        {
            old_left = Left;
            message = msg;
            DoubleAnimation animation = new DoubleAnimation(Left, Left + 10, new Duration(new TimeSpan(0, 0, 0, 0, 20)));
            BeginAnimation(LeftProperty, animation);
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Shuck_DoWork;
            bw.ProgressChanged += Shuck_ProgressChanged;
            bw.RunWorkerAsync();
        }

        void Shuck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(Left, Left - 20, new Duration(new TimeSpan(0, 0, 0, 0, 20)));
            BeginAnimation(LeftProperty, animation);
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Shuck_DoWork;
            bw.ProgressChanged += bw_ProgressChanged2;
            bw.RunWorkerAsync();
        }

        void bw_ProgressChanged2(object sender, ProgressChangedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(Left, Left + 20, new Duration(new TimeSpan(0, 0, 0, 0, 20)));
            BeginAnimation(LeftProperty, animation);
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Shuck_DoWork;
            bw.ProgressChanged += bw_ProgressChanged3;
            bw.RunWorkerAsync();
        }

        void bw_ProgressChanged3(object sender, ProgressChangedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(Left, Left - 20, new Duration(new TimeSpan(0, 0, 0, 0, 20)));
            BeginAnimation(LeftProperty, animation);
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Shuck_DoWork;
            bw.ProgressChanged += bw_ProgressChanged4;
            bw.RunWorkerAsync();
        }

        void bw_ProgressChanged4(object sender, ProgressChangedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(Left, Left + 10, new Duration(new TimeSpan(0, 0, 0, 0, 20)));
            BeginAnimation(LeftProperty, animation);
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Shuck_DoWork;
            bw.ProgressChanged += Shuck_ProgressChanged5;
            bw.RunWorkerAsync();
        }

        void Shuck_ProgressChanged5(object sender, ProgressChangedEventArgs e)
        {
            Left = old_left;
            Start_Animation(0, 1, 4000000, message, color.Stroke, true);
        }

        static void Shuck_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(50);
            ((BackgroundWorker)sender).ReportProgress(0);
        }

        #endregion
    }
}