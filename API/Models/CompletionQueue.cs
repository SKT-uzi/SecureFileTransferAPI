using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResumableFileTransfer.API.Models
{
    public class CompletionQueue
    {
        public string FileID { get; set; }
        public string SoftwareToken { get; set; }
    }
}
