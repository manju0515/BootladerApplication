using System;
using System.Windows;
using Peak.Can.Basic;
using TPCANHandle = System.UInt16;
public partial class MainWindow : Window
{
    private CanCommunication canCommunication;

    public MainWindow()
    {
        InitializeComponent();
        canCommunication = new CanCommunication();
        if (canCommunication.InitializeCan())
        {
            // CAN initialization successful
            MessageBox.Show("CAN Initialization Successful!");
        }
        else
        {
            // Handle initialization error
            MessageBox.Show("CAN Initialization Failed!");
        }
    }

    private void SendMessageButton_Click(object sender, RoutedEventArgs e)
    {
        // Create a CAN message and send it
        TPCANMsg message = new TPCANMsg
        {
            ID = 0x123, // CAN message ID
            MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD,
            DATA = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 },
            LEN = 8 // Data length
        };

        if (canCommunication.SendMessage(message))
        {
            // Message sent successfully
            MessageBox.Show("Message Sent Successfully!");
        }
        else
        {
            // Handle send message error
            MessageBox.Show("Error Sending Message!");
        }
    }

    private void ReceiveMessageButton_Click(object sender, RoutedEventArgs e)
    {
        // Receive a CAN message
        TPCANMsg receivedMessage;
        if (canCommunication.ReceiveMessage(out receivedMessage))
        {
            // Display received message in TextBlock
            DisplayReceivedMessage(receivedMessage);
        }
        else
        {
            // Handle receive message error
            MessageBox.Show("Error Receiving Message!");
        }
    }

    private void DisplayReceivedMessage(TPCANMsg message)
    {
        // Display received message in TextBlock
        ReceivedMessagesTextBlock.Text = $"Received Message:\nID: 0x{message.ID:X}\nData: {BitConverter.ToString(message.DATA)}";
    }
}

public class CanCommunication
{
    private TPCANHandle m_PcanHandle;
    private TPCANStatus m_PcanStatus;

    public CanCommunication()
    {
        m_PcanHandle = PCANBasic.PCAN_NONE;
        m_PcanStatus = PCANBasic.PCAN_ERROR_INITIALIZE;
    }

    public bool InitializeCan()
    {
        m_PcanStatus = PCANBasic.Initialize(m_PcanHandle, TPCANBaudrate.PCAN_BAUD_500K);
        return m_PcanStatus == TPCANStatus.PCAN_ERROR_OK;
    }

    public bool SendMessage(TPCANMsg message)
    {
        m_PcanStatus = PCANBasic.Write(m_PcanHandle, ref message);
        return m_PcanStatus == TPCANStatus.PCAN_ERROR_OK;
    }

    public bool ReceiveMessage(out TPCANMsg message)
    {
        m_PcanStatus = PCANBasic.Read(m_PcanHandle, out message, null);
        return m_PcanStatus == TPCANStatus.PCAN_ERROR_OK;
    }
}
