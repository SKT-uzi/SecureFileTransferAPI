using System;
using System.Collections.Generic;
using System.Text;

namespace ResumableFileTransfer.FunctionApp.Models
{
    internal class ScanningFileRequest
    {
        public string Name { get; set; }
        public string FileUrl { get; set; }
    }
}
