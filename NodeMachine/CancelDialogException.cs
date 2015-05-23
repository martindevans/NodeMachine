using System;

namespace NodeMachine
{
    public class CancelDialogException
        : Exception
    {
        public CancelDialogException(string message)
            : base(message)
        {
            
        }
    }
}
