﻿using System;
using System.IO;

namespace IrixiStepperControllerHelper
{
    public class CommandStruct
    {
        UInt32 _cmd_counter = 0;

        public EnumCommand Command { set; get; }
        public int AxisIndex { set; get; }
        public int AccSteps { set; get; }
        public int DriveVelocity { set; get; }
        public int TotalSteps { set; get; }
        public EnumMoveMode Mode { set; get; }

        /// <summary>
        /// Convert the command struct to the byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] data = new byte[PublicDefinitions.MAX_WRITEDATA_LEN];

            MemoryStream stream = new MemoryStream(data);
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(_cmd_counter++);
            writer.Write((int)this.Command);
            writer.Write(this.AxisIndex);
            writer.Write(this.AccSteps);
            writer.Write(this.DriveVelocity);
            writer.Write(this.TotalSteps);

            writer.Close();
            stream.Close();

            return data;

        }

    }
}