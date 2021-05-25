﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.HttpJob.Agent.Config
{
    public class JobMetaData
    {
        public bool Transien { get; set; }
        public bool Hang { get; set; }

        public string RegisterName { get; set; }
        public string RegisterId { get; set; }
        public bool? EnableAutoRegister { get; set; }
        public bool AutoRegisterResult { get; set; }
    }
}
