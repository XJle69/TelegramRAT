using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TelegramRAT
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void regformButton_Click(object sender, RoutedEventArgs e)
        {
            authPanel.Visibility = Visibility.Hidden;
            regPanel.Visibility = Visibility.Visible;
            passwordRepeatPanel.Visibility = Visibility.Visible;
            caption.Content = "Регистрация";
            formReset();


        }

        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void logformButton_Click(object sender, RoutedEventArgs e)
        {
            authPanel.Visibility = Visibility.Visible;
            regPanel.Visibility = Visibility.Hidden;
            passwordRepeatPanel.Visibility = Visibility.Hidden;
            caption.Content = "Авторизация";
            formReset();



        }


        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void closeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            logRegForm.Close();
        }

        private void ControlGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                logRegForm.Opacity = 0.5;
                this.DragMove();
                
            }
            logRegForm.Opacity = 1;
        }

        private void formReset()
        {
            DoubleAnimation formRestart = new DoubleAnimation();
            formRestart.From = 0;
            formRestart.To = 420;
            formRestart.Duration = TimeSpan.FromSeconds(0.45);
            logRegForm.BeginAnimation(Window.HeightProperty, formRestart);
            loginBox.Text = "";
            passwordBox.Password = "";
            passwordRepeatBox.Password = "";
        }

        private void minimazeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            logRegForm.WindowState = WindowState.Minimized;
        }
    }
}