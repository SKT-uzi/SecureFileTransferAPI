using System;
using System.Collections.Generic;
using System.Text;

namespace ResumableFileTransfer.FunctionApp.Models
{
    public class CompletionQueue
    {
        public string FileID { get; set; }
        public string SoftwareToken { get; set; }
    }
}
