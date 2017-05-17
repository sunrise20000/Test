﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using USBHIDDRIVER;

namespace IrixiStepperControllerHelper
{
    public class IrixiMotionController : INotifyPropertyChanged
    {
        #region Variables

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DeviceStateReport> OnReportUpdated;
        public event EventHandler<ConnectionEventArgs> OnConnectionStatusChanged;

        const string VID = "vid_0483";
        const string PID = "pid_574e";

        /// <summary>
        /// The total steps which is used to acceleration and deceleration
        /// </summary>
        const int ACC_DEC_STEPS = 1000;

        /// <summary>
        /// The maximum drive veloctiy
        /// The real velocity is Velocity_Set * MAX_VELOCITY
        /// </summary>
        const int MAX_VELOCITY = 2000;

        USBInterface _hid_device;

        bool _is_connected = false; // whether the contoller is connected
        string _last_err = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceSN">The serial number of the controller to be connected</param>
        /// <param name="MaxDistance"></param>
        /// <param name="PosAfterHome"></param>
        /// <param name="SCCWLS">Soft CCW limitation sensor</param>
        /// <param name="SCWLS">Soft CW limitation sensor</param>
        public IrixiMotionController(string DeviceSN = "A000000001")
        {
            // Generate the instance of the state report object
            this.Report = new DeviceStateReport();
            
            this.TotalAxes = -1;
            this.SerialNumber = DeviceSN;
            this.AxisCollection = new ObservableCollection<Axis>();

            _hid_device = new USBInterface(VID, PID, DeviceSN);
            _hid_device.EnableUsbBufferEvent(OnReportReceived);
            _hid_device.EnableUsbDisconnectEvent(OnDisconnected);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Get the last error information
        /// </summary>
        public string LastError
        {
            private set
            {
                UpdateProperty<string>(ref _last_err, value);
            }
            get
            {
                return _last_err;
            }
        }

        /// <summary>
        /// Get the number of axes that the HID Controller supports
        /// </summary>
        public int TotalAxes
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the serial number of HID stepper controller
        /// </summary>
        public string SerialNumber
        {
            private set;
            get;
        }

        /// <summary>
        /// Get whether the HID Controller is connected
        /// </summary>
        public bool IsConnected
        {
            private set
            {
                UpdateProperty<bool>(ref _is_connected, value);
            }
            get
            {
                return _is_connected;
            }
        }

        /// <summary>
        /// Get the state report from the HID Controller
        /// </summary>
        public DeviceStateReport Report
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the axis collection instance of the device
        /// </summary>
        public ObservableCollection<Axis> AxisCollection
        {
            private set;
            get;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Read the controllers' serial number and output as a string list
        /// </summary>
        /// <returns></returns>
        public static string[] GetDeviceList()
        {
            USBInterface hid = new USBInterface(PID, VID);
            return hid.GetDeviceList();
        }

        public void OpenDevice()
        {
            int _reconn_counter = 0;
            this.IsConnected = false;

            while (true)
            {
                try
                {
                    _reconn_counter++;

                    if (_hid_device.Connect())
                    {
                        // Start the received task on the UI thread
                        OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionSuccess, null));

                        // Wait the first report from HID device in order to get the 'TotalAxes'
                        do
                        {
                            this.TotalAxes = this.Report.TotalAxes;

                            // Don't check it so fast, the interval of two adjacent report is normally 20ms but not certain
                            Thread.Sleep(200);

                        } while (this.TotalAxes <= 0);

                        this.IsConnected = true;

                        // initialize this.Report property on UI thread
                        OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.TotalAxesReturned, this.TotalAxes));


                        Thread.Sleep(200);

                        break;

                    }
                    else
                    {
                        // pass the try-times to UI thread
                        OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionRetried, _reconn_counter));
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    this.LastError = ex.Message;
                }
            }
        }

        /// <summary>
        /// Start to connect to the HID Controller
        /// </summary>
        /// <returns></returns>
        public Task<bool> OpenDeviceAsync()
        {
            // the progress of connection task
            var progressHandler = new Progress<ConnectionEventArgs>(arg =>
            {
                // the total axes was returned, generate the AxisState collection of this.Report
                if (arg.Event == ConnectionEventArgs.EventType.TotalAxesReturned)
                {
                    this.Report.AxisStateCollection.Clear();

                    for (int i = 0; i < this.TotalAxes; i++)
                    {
                        this.Report.AxisStateCollection.Add(new AxisState() { AxisIndex = i });
                        this.AxisCollection.Add(new Axis()
                        {
                            MaxDistance = 15000,
                            SoftCCWLS = 0,
                            SoftCWLS = 15000
                        });
                    }
                }
                // HID device opened, start the DataRead task
                else if (arg.Event == ConnectionEventArgs.EventType.ConnectionSuccess)
                {
                    _hid_device.StartRead();
                }

                OnConnectionStatusChanged?.Invoke(this, arg);
            });

            var progress = progressHandler as IProgress<ConnectionEventArgs>;

            return Task.Run<bool>(() =>
            {
                int _reconn_counter = 0;
                this.IsConnected = false;

                while (true)
                {
                    try
                    {
                        _reconn_counter++;

                        if (_hid_device.Connect())
                        {
                            // Start the received task on the UI thread
                            progress.Report(new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionSuccess, null));

                            // Wait the first report from HID device in order to get the 'TotalAxes'
                            do
                            {
                                this.TotalAxes = this.Report.TotalAxes;

                                // Don't check it so fast, the interval of two adjacent report is normally 20ms but not certain
                                Thread.Sleep(200);

                            } while (this.TotalAxes <= 0);

                            this.IsConnected = true;

                            // initialize this.Report property on UI thread
                            progress.Report(new ConnectionEventArgs(ConnectionEventArgs.EventType.TotalAxesReturned, this.TotalAxes));


                            Thread.Sleep(200);

                            break;

                        }
                        else
                        {
                            // pass the try-times to UI thread
                            progress.Report(new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionRetried, _reconn_counter));
                            Thread.Sleep(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.LastError = ex.Message;
                    }
                }

                return this.IsConnected;
            });
        }
        
        /// <summary>
        /// Close the HID Controller
        /// </summary>
        public void CloseDevice()
        {
            _hid_device.Disconnect();
        }

        /// <summary>
        /// Start to read report from HID Controller.
        /// Once the report is received, the OnReportReceived event should be fired.
        /// </summary>
        public void StartReadReport()
        {
            _hid_device.StartRead();
        }

        /// <summary>
        /// Home the speicified axis
        /// </summary>
        /// <param name="Axis"></param>
        /// <returns></returns>
        public Task<bool> HomeAsync(int AxisIndex)
        {
            return Task.Run<bool>(() =>
            { 

                // if the controller is not connected, return
                if(!this.IsConnected)
                {
                    this.LastError = string.Format("The controller is not connected.");
                    return false;
                }

                // If the axis is busy, return.
                if (this.Report.AxisStateCollection[AxisIndex].IsRunning)
                {
                    this.LastError = string.Format("Axis {0} is busy.", AxisIndex);
                    return false;
                }

                try
                {
                    CommandStruct cmd = new CommandStruct()
                    {
                        Command = EnumCommand.HOME,
                        AxisIndex = AxisIndex
                    };
                    _hid_device.Write(cmd.ToBytes());

                    Thread.Sleep(500);
                    
                    while(true)
                    {
                        if (this.Report.AxisStateCollection[AxisIndex].IsHomed && this.Report.AxisStateCollection[AxisIndex].IsRunning == false && this.Report.AxisStateCollection[AxisIndex].AbsPosition == 0)
                            break;

                        Thread.Sleep(200);
                    }

                    return true;
                }
                catch(Exception ex)
                {
                    this.LastError = ex.Message;
                    return false;
                }
            });
        }

       /// <summary>
       /// Move the speified axis
       /// </summary>
       /// <param name="AxisIndex"></param>
       /// <param name="Acceleration"></param>
       /// <param name="Velocity"></param>
       /// <param name="Distance"></param>
       /// <param name="Direction"></param>
       /// <returns></returns>
        public Task<bool> MoveAsync(int AxisIndex, int Velocity, int Distance, EnumMoveMode Mode)
        {
            return Task.Run<bool>(() =>
            {
                int _curr_pos = this.Report.AxisStateCollection[AxisIndex].AbsPosition;   // Get current ABS position
                int _pos_aftermove = 0;

                // if the controller is not connected, return
                if (!this.IsConnected)
                {
                    this.LastError = string.Format("The controller is not connected.");
                    return false;
                }

                // If the axis is not homed, return.
                if (this.Report.AxisStateCollection[AxisIndex].IsHomed == false)
                {
                    this.LastError = string.Format("Axis {0} is not homed.", AxisIndex);
                    return false;
                }

                // If the axis is busy, return.
                if (this.Report.AxisStateCollection[AxisIndex].IsRunning)
                {
                    this.LastError = string.Format("Axis {0} is busy.", AxisIndex);
                    return false;
                }
                 
                if(Velocity > 100)
                {
                    this.LastError = string.Format("The velocity should be 1 ~ 100.");
                    return false;
                }

                //
                // Validate the configurable parameters
                //
                // MaxDistance > 0
                if (this.AxisCollection[AxisIndex].MaxDistance <= 0)
                {
                    this.LastError = string.Format("The value of the Max Distance has not been set.");
                    return false;
                }

                // SoftCWLS > SoftCCWLS
                if (this.AxisCollection[AxisIndex].SoftCWLS <= this.AxisCollection[AxisIndex].SoftCCWLS)
                {
                    this.LastError = string.Format("The value of the SoftCWLS should be greater than the value of the SoftCCWLS.");
                    return false;
                }

                // SoftCWLS >= MaxDistance
                if (this.AxisCollection[AxisIndex].SoftCWLS < this.AxisCollection[AxisIndex].MaxDistance)
                {
                    this.LastError = string.Format("The value of the SoftCWLS should be greater than the value of the Max Distance.");
                    return false;
                }

                // SoftCCWLS <= PosAfterHome <= SoftCWLS
                if ((this.AxisCollection[AxisIndex].SoftCCWLS > this.AxisCollection[AxisIndex].PosAfterHome) || 
                (this.AxisCollection[AxisIndex].PosAfterHome > this.AxisCollection[AxisIndex].SoftCWLS))
                {
                    this.LastError = string.Format("The value of the PosAfterHome exceeds the soft limitaion.");
                    return false;
                }

                //
                // Validate the position after moving,
                // if the position exceeds the soft limitation, do not move
                //
                if (Mode == EnumMoveMode.ABS)
                {
                    if(Distance < this.AxisCollection[AxisIndex].SoftCCWLS || Distance > this.AxisCollection[AxisIndex].SoftCWLS)
                    {
                        this.LastError = string.Format("The abs position you are going to move exceeds the soft limitaion.");
                        return false;
                    }
                    else
                    {
                        _pos_aftermove = Distance;
                        
                        Distance = Distance - _curr_pos;
                    }
                }
                else
                {
                    _pos_aftermove = (int)(_curr_pos + Distance);

                    if (Distance > 0) // CW
                    {
                        if (_pos_aftermove > this.AxisCollection[AxisIndex].SoftCWLS)
                        {
                            this.LastError = string.Format("The position you are going to move exceeds the soft CW limitation.");
                            return false;
                        }
                    }
                    else // CCW
                    {
                        if (_pos_aftermove < this.AxisCollection[AxisIndex].SoftCCWLS)
                        {
                            this.LastError = string.Format("The position you are going to move exceeds the soft CCW limitation.");
                            return false;
                        }
                    }
                }

                try
                {
                    // No need to move
                    if (Distance == 0)
                        return true;

                    CommandStruct cmd = new CommandStruct()
                    {
                        Command = EnumCommand.MOVE,
                        AxisIndex = AxisIndex,
                        AccSteps = ACC_DEC_STEPS,
                        DriveVelocity = Velocity * MAX_VELOCITY / 100,
                        TotalSteps = Distance
                    };
                    _hid_device.Write(cmd.ToBytes());

                    //! Wait for 2 report packages to ensure that the move command has been 
                    //! executed by the device.
                    uint _report_counter = this.Report.Counter + 2;

                    do
                    {
                        Thread.Sleep(20);
                    } while (this.Report.Counter <= _report_counter);



                    while (this.Report.AxisStateCollection[AxisIndex].IsRunning)
                    {
                        Thread.Sleep(20);
                    }

                    if(Report.AxisStateCollection[AxisIndex].Error != 0)
                    {
                        this.LastError = string.Format("error code {0:d}", Report.AxisStateCollection[AxisIndex].Error);
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                    
                }
                catch (Exception ex)
                {
                    this.LastError = ex.Message;
                    return false;
                }
            });
        }

        public bool Stop(int AxisIndex)
        {
            // if the controller is not connected, return
            if (!this.IsConnected)
            {
                this.LastError = string.Format("The controller is not connected.", AxisIndex);
                return false;
            }

            try
            {
                CommandStruct cmd = new CommandStruct()
                {
                    Command = EnumCommand.STOP,
                    AxisIndex = AxisIndex
                };
                _hid_device.Write(cmd.ToBytes());
               
                return true;
            }
            catch (Exception ex)
            {
                this.LastError = ex.Message;
                return false;
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// The connected hid device was disconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisconnected(object sender, EventArgs e)
        {
            this.IsConnected = false;
            this.AxisCollection.Clear();
            this.Report.AxisStateCollection.Clear();

            OnConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(ConnectionEventArgs.EventType.ConnectionLost, null));
        }

        /// <summary>
        /// A package was received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReportReceived(object sender, byte[] e)
        {
            byte temp = 0x0;

            MemoryStream stream = new MemoryStream(e);
            BinaryReader reader = new BinaryReader(stream);

            reader.ReadByte(); // Ignore the first dummy byte

            this.Report.Counter = reader.ReadUInt32();
            this.Report.TotalAxes = reader.ReadByte();
            this.Report.IsBusy = reader.ReadByte();
            this.Report.SystemError = reader.ReadByte();

            // Read the Trigger Input State
            temp = reader.ReadByte();
            this.Report.TriggerInput0 = ((temp >> 0) & 0x1) > 0 ? true : false;
            this.Report.TriggerInput1 = ((temp >> 1) & 0x1) > 0 ? true : false;



            if (this.Report.AxisStateCollection == null || this.Report.AxisStateCollection.Count == 0)
                return;

            for (int i = 0; i < this.Report.AxisStateCollection.Count; i++)
            {
                ///
                /// The following parsing process are base on the type of AxisState_TypeDef which is defined in the controller firmware
                ///

                this.Report.AxisStateCollection[i].AbsPosition = reader.ReadInt32();

                // parse Usability
                temp = reader.ReadByte();
                this.Report.AxisStateCollection[i].IsHomed = ((temp >> 0) & 0x1) > 0 ? true : false;
                this.Report.AxisStateCollection[i].IsRunning = ((temp >> 1) & 0x1) > 0 ? true : false;

                // parse input signal
                temp = reader.ReadByte();
                this.Report.AxisStateCollection[i].CWLS = ((temp >> 0) & 0x1) > 0 ? true : false;
                this.Report.AxisStateCollection[i].CCWLS = ((temp >> 1) & 0x1) > 0 ? true : false;
                this.Report.AxisStateCollection[i].ORG = ((temp >> 2) & 0x1) > 0 ? true : false;
                this.Report.AxisStateCollection[i].ZeroOut = ((temp >> 3) & 0x1) > 0 ? true : false;
                this.Report.AxisStateCollection[i].IN_A = ((temp >> 4) & 0x1) > 0 ? true : false;
                this.Report.AxisStateCollection[i].IN_B = ((temp >> 5) & 0x1) > 0 ? true : false;

                // 
                this.Report.AxisStateCollection[i].Error = reader.ReadByte();

                reader.ReadByte();  // read dummy byte, this is used to align struct on 4-byte
            }

            reader.Close();
            stream.Close();

            OnReportUpdated?.Invoke(this, this.Report);
        }
        #endregion

        #region RaisePropertyChangedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="PropertyName"></param>
        protected void UpdateProperty<T>(ref T OldValue, T NewValue, [CallerMemberName]string PropertyName = "")
        {
            if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
                return;

            OldValue = NewValue;                // Set the property value to the new value
            OnPropertyChanged(PropertyName);    // Raise the notify event
        }

        protected void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null)
            //    handler(this, new PropertyChangedEventArgs(PropertyName));
            //RaisePropertyChanged(PropertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }
        #endregion



    }
}