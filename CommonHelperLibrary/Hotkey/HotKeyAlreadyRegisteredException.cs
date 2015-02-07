using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelperLibrary.Hotkey
{
    [Serializable]
    public class HotKeyAlreadyRegisteredException : Exception
    {
        public HotKey HotKey { get; private set; }

        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey)
            : base(message)
        {
            HotKey = hotKey;
        }

        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey, Exception inner)
            : base(message, inner)
        {
            HotKey = hotKey;
        }

        protected HotKeyAlreadyRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
