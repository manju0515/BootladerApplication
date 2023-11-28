using System;
using System.Threading;
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
            StartReceivingMessages();
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
                MessageBox.Show($"Error initializing CAN: {status.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error initializing CAN: {status.ToString()}");
                // Handle the error accordingly
            }
        }

        private void StartReceivingMessages()
        {
            // Start a background thread to listen for CAN messages
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        private void ReceiveMessages()
        {
            TPCANMsg canMessage = new TPCANMsg();
            TPCANTimestamp timestamp = new TPCANTimestamp();
            TPCANStatus status;

            while (true)
            {
                // Read incoming CAN messages
                status = PCANBasic.Read(selectedChannel, out canMessage, out timestamp);

                if (status == TPCANStatus.PCAN_ERROR_OK)
                {
                    // Process the received CAN message
                    string receivedMessage = GetMessageFromCANData(canMessage);

                    // Update UI with the received message
                    Dispatcher.Invoke(() =>
                    {
                        receivedMessageLabel.Content = $"Received Message from Bus master : {receivedMessage}";
                    });
                }
                else if (status == TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                {
                    // No message received, continue listening
                }
                else
                {
                    // Handle other errors accordingly
                    Console.WriteLine("Error receiving CAN message: " + status.ToString());
                    break;
                }
            }
        }

        private string GetMessageFromCANData(TPCANMsg canMessage)
        {
            // Assuming ASCII characters in the CAN message data
            return System.Text.Encoding.ASCII.GetString(canMessage.DATA).Trim('\0');
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
                    ID = 0x123, // Use your desired CAN message ID (e.g., 0x123)
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
                    transmittedMessageLabel.Content = $"Message Transmitted from Windows application : {message}";
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
