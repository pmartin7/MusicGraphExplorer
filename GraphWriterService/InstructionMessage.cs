using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphWriterService
{
    public class InstructionMessage
    {
        #region properties

        public string Message { get; set; }

        public ushort InstructionCode { get; set; }

        public string InstructionKey { get; set; }

        #endregion

        #region constructors

        public InstructionMessage(ushort instructionCode, string key)
        {
            Message = string.Format("{0}|{1}", instructionCode, key);
            InstructionCode = instructionCode;
            InstructionKey = key;
        }

        public InstructionMessage(string message)
        {
            Message = message;

            if ((null != message) && (message.Contains("|")))
            {
                try
                {
                    string[] strings = message.Split('|');
                    InstructionCode = ushort.Parse(strings[0].Trim(' '));
                    InstructionKey = strings[1].Trim(' ');
                }
                catch { } //do nothing, just construct the object with a message
            }
        }

        #endregion
    }
}
