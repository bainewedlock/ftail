using PowerArgs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FairyTail
{
    public class MyArgs
    {
        [ArgRequired]
        [ArgExistingFile]
        public string File { get; set; }
    }
}
