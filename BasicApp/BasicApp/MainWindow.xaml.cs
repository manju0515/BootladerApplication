using System;
using System.Windows;
using System.Windows.Controls;
using Peak.Can.Basic;
using TPCANHandle = System.UInt16;

namespace WpfTransmissionApp
{
    public partial class MainWindow : Window
    {
        private TPCANHandle selectedChannel = PCANBasic.PCAN_USBBUS1; // Default to PCAN_USBBUS1

        public MainWindow()
        {
            InitializeComponent();
            InitializeCAN();
            //DetectCANChannels();
        }

        private void InitializeCAN()
        {
            TPCANBaudrate baudrate = TPCANBaudrate.PCAN_BAUD_500K;

            TPCANStatus status = PCANBasic.Initialize(selectedChannel, baudrate);

            if (status == TPCANStatus.PCAN_ERROR_OK)
            {
                canStatusTextBlock.Text = "CAN cable connected";
                MessageBox.Show("CAN initialized successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                canStatusTextBlock.Text = "CAN cable disconnected";
                MessageBox.Show("Error initializing CAN: " + status.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Handle the error accordingly
            }
        }

        private void DetectCANChannels()
        {
            // Assuming PCAN_USBBUS1 and PCAN_USBBUS2, adjust based on your hardware
            TPCANHandle[] channelsToDetect = { PCANBasic.PCAN_USBBUS1, PCANBasic.PCAN_USBBUS2 };

            foreach (var channel in channelsToDetect)
            {
                TPCANBaudrate baudrate = TPCANBaudrate.PCAN_BAUD_500K;

                TPCANStatus status = PCANBasic.Initialize(channel, baudrate);

                if (status == TPCANStatus.PCAN_ERROR_OK)
                {
                    // CAN cable connected on this channel
                    MessageBox.Show($"CAN cable detected on Channel {channel}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    PCANBasic.Uninitialize(channel); // Uninitialize after detection
                }
                else
                {
                    // No CAN cable on this channel
                    // Handle the error accordingly or just continue the loop
                }
            }
        }

        private void OnSendButtonClick(object sender, RoutedEventArgs e)
        {
            // Get the message from the TextBox
            string message = messageTextBox.Text;

            // Check if the CAN channel is initialized
            if (PCANBasic.GetStatus(selectedChannel) == TPCANStatus.PCAN_ERROR_OK)
            {
                // Transmit the message
                TPCANMsg messageToSend = new TPCANMsg
                {
                    ID = 0x123, // Use your desired CAN message ID
                    MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD,
                    LEN = (byte)message.Length,
                    DATA = new byte[8] // Initialize the DATA array with a fixed size (assuming a standard CAN frame)
                };

                // Copy the message string to the message structure
                for (int i = 0; i < message.Length && i < 8; i++)
                {
                    messageToSend.DATA[i] = (byte)message[i];
                }

                TPCANStatus transmitStatus = PCANBasic.Write(selectedChannel, ref messageToSend);

                if (transmitStatus == TPCANStatus.PCAN_ERROR_OK)
                {
                    // Display the transmitted message and selected CAN channel in the Label.
                    transmittedMessageLabel.Content = $"Message Transmitted on Channel {selectedChannel}: {message}";
                }
                else
                {
                    MessageBox.Show("Error transmitting message: " + transmitStatus.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Handle the error accordingly
                }
            }
            else
            {
                MessageBox.Show("CAN channel not initialized. Please initialize the CAN channel first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the selected CAN channel when the ComboBox selection changes
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedContent = selectedItem.Content.ToString();

                if (selectedContent == "Channel 1")
                {
                    selectedChannel = PCANBasic.PCAN_USBBUS1;
                }
                else if (selectedContent == "Channel 2")
                {
                    selectedChannel = PCANBasic.PCAN_USBBUS2;
                }
                else
                {
                    throw new InvalidOperationException("Unexpected CAN channel selection.");
                }

                // Reinitialize CAN with the new selected channel
                InitializeCAN();
            }
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
