// Created by: Rodriguez Mustelier Angel (rodang)
// Modify On: 2021-03-29 01:01

using Microsoft.Xrm.Sdk;

namespace CrmWebResourcesUpdater.Forms
{
    public class WebResourceType
    {
        public WebResourceType(int value, string displayName, params string[] extensions)
        {
            DisplayName = displayName;
            Extensions = extensions;
            Option = new OptionSetValue(value);
        }

        public string DisplayName { get; }
        public string[] Extensions { get; }

        public OptionSetValue Option { get; }

        public override string ToString() => DisplayName;
    }
}