using System;
using System.Collections.Generic;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Common.Extensions;
using McTools.Xrm.Connection;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace CrmWebResourcesUpdater.Helpers
{

    /// <summary>
    /// Provides methods for loading and saving user settings
    /// </summary>
    public class SettingsManager<T>
    {
        internal static Dictionary<Guid, T> Cache = new Dictionary<Guid, T>();

        public string SettingsPropertyName => typeof(T).FullName;
        
        protected const string CollectionBasePath = "CrmPublisherSettings";
       
        private readonly WritableSettingsStore settingsStore;
        private readonly Guid projectGuid;
        
        /// <summary>
        /// Collection Path based on project guid
        /// </summary>
        public string CollectionPath => $"{CollectionBasePath}_{projectGuid}";
              
        /// <summary>
        /// Gets Settings Instance
        /// </summary>
        /// <param name="serviceProvider">Extension service provider</param>
        /// <param name="projectGuid">Guid of project to read settings of</param>
        public SettingsManager(IServiceProvider serviceProvider, Guid projectGuid)
        {
            this.projectGuid = projectGuid;
            settingsStore = GetWritableSettingsStore(serviceProvider);
        }

        /// <summary>
        /// Gets settings store for current user
        /// </summary>
        /// <param name="serviceProvider">Extension service provider</param>
        /// <returns>Returns Instanse of Writtable Settings Store for current user</returns>
        private WritableSettingsStore GetWritableSettingsStore(IServiceProvider serviceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(serviceProvider);
            return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        /// <summary>
        /// Reads and parses setting from settings store
        /// </summary>
        public T GetSettings()
        {
            if (!settingsStore.CollectionExists(CollectionPath))
                return default(T);

            var type = typeof(T);
            if (settingsStore.PropertyExists(CollectionPath, type.FullName) == false)
                return default(T);

            var settingsXml = settingsStore.GetString(CollectionPath, SettingsPropertyName);
            try
            {
                var settings = (T) XmlSerializerHelper.Deserialize(settingsXml, type);
                return settings;
            }
            catch (Exception)
            {
                Logger.WriteLine($"Failed to parse settings of type <{SettingsPropertyName}>");
                return default(T);
            }
        }

        /// <summary>
        /// Writes setting to settings store
        /// </summary>
        public void Save(T settings)
        {
            if(settings == null)
            {
                settingsStore.DeletePropertyIfExists(CollectionPath, SettingsPropertyName);
                return;
            }
          
            if (!settingsStore.CollectionExists(CollectionPath))
                settingsStore.CreateCollection(CollectionPath);

            var settingnsXml = XmlSerializerHelper.Serialize(settings);
            settingsStore.SetString(CollectionPath, SettingsPropertyName, settingnsXml);
        }
    }
}
