using System;
using System.Runtime.Serialization;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Common.Extensions;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace CrmWebResourcesUpdater.Settings
{

    /// <summary>
    /// Provides methods for loading and saving user settings
    /// </summary>
    public class SettingsManager<T> where T: ISerializable, new()
    {
        public string SettingsPropertyName => typeof(T).FullName;
        
        protected const string CollectionBasePath = "CrmPublisherSettings";
        public const string FileKindGuid =         "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string ProjectKindGuid =      "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
       
        private readonly WritableSettingsStore _settingsStore;
        private readonly Guid _projectGuid;
        
        /// <summary>
        /// Collection Path based on project guid
        /// </summary>
        public string CollectionPath => $"{CollectionBasePath}_{_projectGuid}";
              
        public T Settings { get; set; }
      
        /// <summary>
        /// Gets Settings Instance
        /// </summary>
        /// <param name="serviceProvider">Extension service provider</param>
        /// <param name="projectGuid">Guid of project to read settings of</param>
        public SettingsManager(IServiceProvider serviceProvider, Guid projectGuid)
        {
            _projectGuid = projectGuid;
            _settingsStore = GetWritableSettingsStore(serviceProvider);
            
            if (_settingsStore.CollectionExists(CollectionPath))
            {
                Settings = GetSettings();
            }
            else
            {
                _settingsStore.CreateCollection(CollectionPath);
            }
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
        /// Reads and parses Crm Connections from settings store
        /// </summary>
        /// <returns>Returns Crm Connetions</returns>
        private T GetSettings()
        {
            if (_settingsStore.PropertyExists(CollectionPath, typeof(T).FullName) == false)
                return default(T);

            var settingsXml = _settingsStore.GetString(CollectionPath, SettingsPropertyName);
            try
            {
                var settings = XmlSerializerHelper.Deserialize<T>(settingsXml);
                return settings;
            }
            catch (Exception)
            {
                Logger.WriteLine($"Failed to parse settings of type <{SettingsPropertyName}>");
                return default(T);
            }
        }

        /// <summary>
        /// Writes Crm Connection to settings store
        /// </summary>
        /// <param name="crmConnections">Crm Connections to write to settings store</param>
        private void Save()
        {
            if(Settings == null)
            {
                _settingsStore.DeletePropertyIfExists(CollectionPath, SettingsPropertyName);
                return;
            }
          
            var settingnsXml = XmlSerializerHelper.Serialize(Settings);
            _settingsStore.SetString(CollectionPath, SettingsPropertyName, settingnsXml);
        }
    }
}
