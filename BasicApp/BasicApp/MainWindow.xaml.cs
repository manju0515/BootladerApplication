using System.Windows;
using System.Windows.Controls;
using Peak.Can.Basic;
namespace WpfTransmissionApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnSendButtonClick(object sender, RoutedEventArgs e)
        {
            // Get the selected CAN channel from the ComboBox
            string selectedCanChannel = (canChannelsComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Get the message from the TextBox
            string message = messageTextBox.Text;

           
            // Display the transmitted message and selected CAN channel in the Label.
            transmittedMessageLabel.Content = $"Message Transmitted on {selectedCanChannel}: {message}";
        }

        private void OnTextBoxMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            // Clear the text when the TextBox is double-clicked
            if (sender is TextBox textBox)
            {
                textBox.Text = string.Empty;
            }
        }
    }
}
