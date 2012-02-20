﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    public abstract class ComDevicePlugin : Plugin
    {
        private bool running;
        protected string port;
        protected int baudRate;
        protected bool newData;

        protected abstract void Init(SerialPort serialPort);
        protected abstract void Read(SerialPort serialPort);
        protected abstract string BaudRateHelpText { get; }
        protected abstract int DefaultBaudRate { get; }

        public override Action Start()
        {
            return ThreadAction;
        }

        private void ThreadAction()
        {
            running = true;
            OnStarted(this, new EventArgs());

            using (var serialPort = new SerialPort(port, baudRate))
            {
                serialPort.Open();
                Init(serialPort);

                while (running)
                {
                    Read(serialPort);
                }
                serialPort.Close();
            }
        }

        public override void Stop()
        {
            running = false;
        }

        public override void DoBeforeNextExecute()
        {
            if (newData)
            {
                OnUpdate();
                newData = false;
            }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            switch (index)
            {
                case 0:
                    property.Name = "Port";
                    property.Caption = "Com port";
                    property.HelpText = "The com port of the FTDI device";

                    foreach (var p in SerialPort.GetPortNames())
                    {
                        property.Choices.Add(p, p);
                    }

                    property.DefaultValue = "COM3";
                    return true;
                case 1:
                    property.Name = "BaudRate";
                    property.Caption = "Baud rate";
                    property.DefaultValue = DefaultBaudRate;
                    property.HelpText = BaudRateHelpText;

                    foreach (var rate in new int[] { 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 })
                    {
                        property.Choices.Add(rate.ToString(CultureInfo.InvariantCulture), rate);
                    }

                    return true;
            }

            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            port = properties["Port"] as string;
            baudRate = (int)properties["BaudRate"];

            return true;
        }
    }
}
