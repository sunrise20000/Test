﻿using System;

namespace Irixi_Aligner_Common.Configuration.Base
{
    public class ConfigurationBase
    {
        public bool Enabled { get; set; }
        public Guid DeviceClass { get; set; }
        public string Desc { get; set; }
        public string Port { get; set; }
        public int BaudRate { get; set; }
        public string Caption { get; set; }
        public string Icon { get; set; }
        public string Comment { get; set; }
    }
}
