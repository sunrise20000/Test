﻿using Irixi_Aligner_Common.Interfaces;
using System.Diagnostics;
using System.Threading;

namespace Irixi_Aligner_Common.AlignmentArithmetic
{
    public class AlignmentBase : IServiceSystem
    {

        protected CancellationTokenSource cts;
        protected CancellationToken cts_token;

        public AlignmentBase(AlignmentArgsBase Args)
        {
            this.Args = Args;
        }
       
        public AlignmentArgsBase Args
        {
            protected set; get;
        }

        public virtual void Start()
        {
            cts = new CancellationTokenSource();
            cts_token = cts.Token;

            Args.Validate();

            Args.ClearScanCurve();
        }

        public virtual void Stop()
        {
            cts.Cancel();
        }
        
    }
}
