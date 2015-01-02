using System;
using System.Collections.Generic;
using System.Windows;

namespace Dr_Connectify
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            name.Text = Properties.Settings.Default.network_name;
            password_text.Text = Properties.Settings.Default.password;
            name_count.Text = Properties.Settings.Default.count.ToString();
            chars.Text = Properties.Settings.Default.chars;
            checkBox.IsChecked = Properties.Settings.Default.auto_increase;
            checkBox1.IsChecked = Properties.Settings.Default.new_password;
        }

        void Save_Click(object sender, RoutedEventArgs e)
        {
            if (name.Text.Equals(""))
                name.Text = "Dr.Mohammad";
            if (name_count.Text.Equals(""))
                name.Text = "1";
            if (password_text.Text.Length < 8)
                password_text.Text = "Dr.Mohammad";
            password_text.Text = RemoveNonASCIICharsNoSplit(password_text.Text);
            chars.Text = RemoveNonASCIIChars(chars.Text);
            Properties.Settings.Default.network_name = name.Text;
            Properties.Settings.Default.password = password_text.Text;
            Properties.Settings.Default.count = RemoveNonNumberChars(name_count.Text);
            Properties.Settings.Default.chars = chars.Text;
            Properties.Settings.Default.auto_increase = checkBox.IsChecked.Value;
            Properties.Settings.Default.new_password = checkBox1.IsChecked.Value;
            Properties.Settings.Default.Save();
            Close();
        }

        string RemoveNonASCIIChars(string s)
        {
            char[] c = { ' ' };
            string[] matrix = s.Split(c, StringSplitOptions.RemoveEmptyEntries);
            List<string> returned_list = new List<string>();
            string to_return = "";
            foreach (var i in matrix)
                if (i.Length == 1)
                    if (i[0] > 32 && i[0] < 128)
                        if (!returned_list.Contains(i))
                        {
                            returned_list.Add(i);
                            to_return += (i + ' ');
                        }
            if (to_return.Equals(string.Empty))
                to_return = "! \" # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \\ ] ^ _ ` a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~";
            return to_return;
        }

        string RemoveNonASCIICharsNoSplit(string s)
        {
            string to_return = "";
            foreach (var i in s)
                if (i > 32 && i < 128)
                    to_return += i;
            return to_return;
        }

        int RemoveNonNumberChars(string s)
        {
            string b = s;
            foreach (char i in s)
                if (i < 48 || i > 57)
                    b = b.Replace(i.ToString(), "");
            int j;
            int.TryParse(b, out j);
            return j;
        }
    }
}
