using System.Collections.Generic;

namespace McTools.Xrm.Connection
{
    /// <summary>
    /// Stores the list of Crm connections
    /// </summary>
    public class CrmConnections
    {
        public List<ConnectionDetail> Connections { get; set; }
        public string ProxyAddress { get; set; }
        public string ProxyPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseCustomProxy { get; set; }
        public bool PublishAfterUpload { get; set; } = true;
        public bool IgnoreExtensions { get; set; } = false;
        public bool ExtendedLog { get; set; } = false;
    }
}