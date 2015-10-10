﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;

namespace OpenDACT.Class_Files
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            UserVariables userVariables = UserInterface.returnUserVariablesObject();

            InitializeComponent();
            consoleMain.Text = "";
            consoleMain.ScrollBars = RichTextBoxScrollBars.Vertical;
            consolePrinter.Text = "";
            consolePrinter.ScrollBars = RichTextBoxScrollBars.Vertical;


            // Basic set of standard baud rates.
            baudRateCombo.Items.Add("250000");
            baudRateCombo.Items.Add("115200");
            baudRateCombo.Items.Add("57600");
            baudRateCombo.Items.Add("38400");
            baudRateCombo.Items.Add("19200");
            baudRateCombo.Items.Add("9600");
            baudRateCombo.Text = "250000";  // This is the default for most RAMBo controllers.

            heuristicModeCombo.Items.Add("True");
            heuristicModeCombo.Items.Add("False");
            heuristicModeCombo.Text = "False";

            comboBoxZMinimumValue.Items.Add("FSR");
            comboBoxZMinimumValue.Items.Add("Z-Probe");
            comboBoxZMinimumValue.Text = "FSR";

            advancedPanel.Visible = false;
            printerLogPanel.Visible = false;

            Connection.readThread = new Thread(ConsoleRead.Read);
            Connection._serialPort = new SerialPort();


            // Build the combobox of available ports.
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length >= 1)
            {
                Dictionary<string, string> comboSource = new Dictionary<string, string>();

                int count = 0;

                foreach (string element in ports)
                {
                    comboSource.Add(ports[count], ports[count]);
                    count++;
                }

                portsCombo.DataSource = new BindingSource(comboSource, null);
                portsCombo.DisplayMember = "Key";
                portsCombo.ValueMember = "Value";
            }
            else
            {
                UserInterface.logConsole("No ports available\n");
            }

            /*
            String[] zMinArray = { "FSR", "Z-Probe" };
            comboZMin.DataSource = zMinArray;

            // clear the result labels.
            lblXAngleTower.Text = "";
            lblXPlate.Text = "";
            lblXAngleTop.Text = "";
            lblXPlateTop.Text = "";
            lblYAngleTower.Text = "";
            lblYPlate.Text = "";
            lblYAngleTop.Text = "";
            lblYPlateTop.Text = "";
            lblZAngleTower.Text = "";
            lblZPlate.Text = "";
            lblZAngleTop.Text = "";
            lblZPlateTop.Text = "";
            lblScaleOffset.Text = "";
            */

            accuracyTime.Series["Accuracy"].Points.AddXY(0, 1);
            UserInterface.isInitiated = true;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Connection.connect();
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            Connection.disconnect();
        }

        private void calibrateButton_Click(object sender, EventArgs e)
        {
            if (Connection._serialPort.IsOpen)
            {
                EEPROMFunctions.readEEPROM();
                Calibration.calibrationState = true;
                Calibration.calibrationSelection = 0;
            }
            else
            {
                UserInterface.logConsole("Not connected\n");
            }
        }

        private void resetPrinter_Click(object sender, EventArgs e)
        {
            if (Connection._serialPort.IsOpen)
            {
                GCode.emergencyReset();
            }
            else
            {
                UserInterface.logConsole("Not connected\n");
            }
        }
        public void appendMainConsole(string value)
        {
            Invoke((MethodInvoker)delegate { consoleMain.AppendText(value + "\n"); });
            Invoke((MethodInvoker)delegate { consoleMain.ScrollToCaret(); });
        }
        public void appendPrinterConsole(string value)
        {
            Invoke((MethodInvoker)delegate { consolePrinter.AppendText(value + "\n"); });
            Invoke((MethodInvoker)delegate { consolePrinter.ScrollToCaret(); });
        }

        private void openAdvanced_Click(object sender, EventArgs e)
        {
            if (advancedPanel.Visible == false) {
                advancedPanel.Visible = true;
                printerLogPanel.Visible = true;
            }
            else
            {
                advancedPanel.Visible = false;
                printerLogPanel.Visible = false;
            }
        }

        private void sendGCode_Click(object sender, EventArgs e)
        {
            if (Connection._serialPort.IsOpen)
            {
                Connection._serialPort.WriteLine(GCodeBox.Text.ToString().ToUpper());
                UserInterface.logConsole("Sent: " + GCodeBox.Text.ToString().ToUpper());
            }
            else
            {
                UserInterface.logConsole("Not Connected");
            }
        }

        public void setAccuracyPoint(float x, float y)
        {
            Invoke((MethodInvoker)delegate {
                accuracyTime.Refresh();
                accuracyTime.Series["Accuracy"].Points.AddXY(x, y);
            });
        }
    }
}
