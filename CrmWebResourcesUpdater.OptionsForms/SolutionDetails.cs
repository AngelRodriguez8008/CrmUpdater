// Created by: Rodriguez Mustelier Angel (rodang)
// Modify On: 2021-03-18 04:02

using System;

namespace CrmWebResourcesUpdater.OptionsForms
{
    public class SolutionDetails
    {
        public Guid SolutionId { get; set; }
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public string PublisherPrefix { get; set; }

        public override string ToString() => FriendlyName;
    }
}