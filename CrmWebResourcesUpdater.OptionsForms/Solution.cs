// Created by: Rodriguez Mustelier Angel (rodang)
// Modify On: 2021-03-18 04:01

namespace CrmWebResourcesUpdater.OptionsForms
{
    public class Solution
    {
        public SolutionDetails SolutionDetails { get; set; }

        public override string ToString() => SolutionDetails.FriendlyName;
    }
}