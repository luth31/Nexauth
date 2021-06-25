using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace nexauth_client {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            this.LOGIN_BUTTON.Click += loginButton_click;
            client = new Client(this);
        }

        private async void loginButton_click(object sender, RoutedEventArgs e) {
            bool success = client.SetUsername(USERNAME_TEXTBOX.Text) & await client.SetHost(HOST_TEXTBOX.Text, 8300);
            if (!success) {
                UpdateStatusText(STATUS_LABEL, "There are errors in the input!", Brushes.Red);
            }
            else {
                ClearStatusText(STATUS_LABEL);
                ClearStatusText(HOST_STATUS_LABEL);
                ClearStatusText(USERNAME_STATUS_LABEL);
                client.Connect();
                client.SendUsername();
            }
            
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            client.Disconnect();
        }

        public void UpdateStatusText(Label label, string text) {
            label.Content = text;
            label.Foreground = Brushes.Black;
        }

        public void UpdateStatusText(Label label, string text, Brush color) {
            label.Content = text;
            label.Foreground = color;
        }

        public void ClearStatusText(Label label) {
            label.Content = "";
        }

        private Client client;
    }
}
