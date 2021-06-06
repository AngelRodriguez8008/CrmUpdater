// Created by: Rodriguez Mustelier Angel (rodang)
// Modify On: 2021-03-18 04:10

using McTools.Xrm.Connection;

namespace CrmWebResourcesUpdater.OptionsForms
{
    public class Settings
    {
        public ConnectionDetail Connection {get;set; }
        public SolutionDetails Solution { get; set; }

        public bool AutoPublish { get; set; } = true;
        public bool IgnoreExtensions { get; set; } = false;
        public bool ExtendedLog { get; set; } = true;
    }
}