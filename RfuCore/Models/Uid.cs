using System;

namespace ResumableFileTransfer.RfuCore.Models
{
    public class Uid
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
