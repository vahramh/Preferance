using System;

namespace Preferance.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public string RequestString { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
