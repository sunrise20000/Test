﻿using Irixi_Aligner_Common.Configuration;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Interfaces
{
    public interface IAxis
    {
        #region Properties
        /// <summary>
        /// Get or set the readable name of axis
        /// </summary>
        string AxisName { get; }

        /// <summary>
        /// Get the axis index in its mother controller
        /// </summary>
        int AxisIndex { get; }

        /// <summary>
        /// Get whether the axis is busy or not.
        /// if is busy, the operation to the axis is denied.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Get or set whether the axis could be operated manually or not
        /// </summary>
        bool IsManualEnabled { set; get; }

        /// <summary>
        /// Get or set whether the axis works on ABS mode
        /// </summary>
        bool IsAbsMode { set; get; }

        /// <summary>
        /// Get whether the axis is homed or not
        /// </summary>
        bool IsHomed { get; }

        /// <summary>
        /// Get or set whether the axis could be used or not
        /// </summary>
        bool IsEnabled { set; get; }

        /// <summary>
        /// Get the initial position after homed
        /// </summary>
        int InitPosition { get; }

        /// <summary>
        /// Get the absolute position
        /// </summary>
        int AbsPosition { set; get; }

        /// <summary>
        /// Get the relative position
        /// </summary>
        int RelPosition { get; }

        /// <summary>
        /// Get the maximum stroke the axis supports
        /// </summary>
        int MaxStroke { get; }

        /// <summary>
        /// Get or set the CW limitaion
        /// </summary>
        int CWL { set; get; }

        /// <summary>
        /// Get or set the CCW limitaion
        /// </summary>
        int CCWL { set; get; }

        /// <summary>
        /// Get or set the tag of axis
        /// </summary>
        object Tag { set; get; }

        /// <summary>
        /// Get distance per step, unit in nm
        /// </summary>
        int Dps { get; }

        /// <summary>
        /// Get the last error
        /// </summary>
        string LastError { set; get; }

        /// <summary>
        /// Get the parent motiong controller
        /// </summary>
        IMotionController ParentController { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Lock the axis before any operation
        /// </summary>
        /// <returns></returns>
        bool Lock();

        /// <summary>
        /// Unlock the axis after any operation
        /// </summary>
        void Unlock();

        /// <summary>
        /// Set the parameters once the class is instantiated
        /// </summary>
        /// <param name="AxisIndex"></param>
        /// <param name="Config"></param>
        /// <param name="Controller"></param>
        void SetParameters(int AxisIndex, ConfigPhysicalAxis Config, IMotionController Controller);

        /// <summary>
        /// Start a task to home the axis
        /// </summary>
        /// <returns></returns>
        Task<bool> Home();

        /// <summary>
        /// Start a task to move the axis
        /// </summary>
        /// <param name="Mode">ABS/REL</param>
        /// <param name="Speed">1 ~ 100</param>
        /// <param name="Distance">The steps to move, in steps</param>
        /// <returns></returns>
        Task<bool> Move(MoveMode Mode, int Speed, int Distance);

        /// <summary>
        /// Start a task to move the axis and output a series of trigger pulses on the specified channel(I/O)
        /// </summary>
        /// <param name="Mode">ABS/REL</param>
        /// <param name="Speed">1 ~ 100</param>
        /// <param name="Distance">The steps to move, in steps</param>
        /// <param name="Interval">The steps between adjacent trigger pulse, in steps</param>
        /// <param name="Channel">The trigger channel</param>
        /// <returns></returns>
        Task<bool> MoveWithTrigger(MoveMode Mode, int Speed, int Distance, int Interval, int Channel);

        /// <summary>
        /// Start a task to move the axis and activate a series of conversion with the specified adc
        /// </summary>
        /// <param name="Mode">ABS/REL</param>
        /// <param name="Speed">1 ~ 100</param>
        /// <param name="Distance">The steps to move, in steps</param>
        /// <param name="Interval">The steps between adjacent ADC conversion, in steps</param>
        /// <param name="AdcIndex">The channle of ADC</param>
        /// <returns></returns>
        Task<bool> MoveWithInnerADC(MoveMode Mode, int Speed, int Distance, int Interval, int Channel);

        /// <summary>
        /// Stop the current activity immediately 
        /// </summary>
        void Stop();

        /// <summary>
        /// Toggle the move mode between ABS and REL
        /// </summary>
        void ToggleMoveMode();

        #endregion

    }
}