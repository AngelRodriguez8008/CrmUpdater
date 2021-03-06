﻿using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmWebResourcesUpdater.Common.Extensions;
using McTools.Xrm.Connection;
using System.Security.Cryptography;
using CrmWebResourcesUpdater.Common;

namespace CrmWebResourcesUpdater
{

    /// <summary>
    /// Provides methods for loading and saving user settings
    /// </summary>
    public class Settings
    {
        protected const string CollectionBasePath = "CrmPublisherSettings";
        protected const string ConnectionsPropertyName = "Connections";
        protected const string SelectedConnectionIdPropertyName = "SelectedConnectionId";
        protected const string AutoPublishPropertyName = "AutoPublishEnabled";
        protected const string IgnoreExtensionsProprtyName = "IgnoreExtensions";
        protected const string ExtendedLogProprtyName = "ExtendedLog";

        public const string FileKindGuid =         "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string ProjectKindGuid =      "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string MappingFileName = "UploaderMapping.config";

        private readonly byte[] _entropy = Encoding.Unicode.GetBytes("crm.publisher");
        private readonly WritableSettingsStore _settingsStore;
        private readonly Guid _projectGuid;
        

        /// <summary>
        /// Crm Connections
        /// </summary>
        public CrmConnections CrmConnections { get; set; }

        /// <summary>
        /// Selected Connection Guid
        /// </summary>
        public Guid? SelectedConnectionId { get; set; }

        /// <summary>
        /// Collection Path based on project guid
        /// </summary>
        public string CollectionPath => $"{CollectionBasePath}_{_projectGuid}";

        /// <summary>
        /// Selected Connection used to publish web resources
        /// </summary>
        public ConnectionDetail SelectedConnection
        {
            get
            {
                //Load();
                if (CrmConnections?.Connections == null || SelectedConnectionId == null)
                {
                    return null;
                }
                return CrmConnections.Connections.FirstOrDefault(c => c.ConnectionId == SelectedConnectionId.Value);
            }
            set
            {
                if(value == null)
                {
                    SelectedConnectionId = null;
                    return;
                }
                if(CrmConnections == null)
                {
                    CrmConnections = new CrmConnections { Connections = new List<ConnectionDetail> { value } };
                }
                if(CrmConnections.Connections == null)
                {
                    CrmConnections.Connections = new List<ConnectionDetail> { value };
                }
                if(CrmConnections.Connections.Any(c => c.ConnectionId == value.ConnectionId) == false)
                {
                    CrmConnections.Connections.Add(value);
                }
                SelectedConnectionId = value.ConnectionId;
            }
        }

        /// <summary>
        /// Gets Settings Instance
        /// </summary>
        /// <param name="serviceProvider">Extension service provider</param>
        /// <param name="projectGuid">Guid of project to read settings of</param>
        public Settings(IServiceProvider serviceProvider, Guid projectGuid)
        {
            _projectGuid = projectGuid;
            _settingsStore = GetWritableSettingsStore(serviceProvider);


            if (_settingsStore.CollectionExists(CollectionPath))
            {
                Load();
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
        /// Reads settings from settings store
        /// </summary>
        private void Load()
        {
            CrmConnections = GetCrmConnections();
            SelectedConnectionId = _settingsStore.GetGuid(CollectionPath, SelectedConnectionIdPropertyName);
        }

        /// <summary>
        /// Saves settings to settings store
        /// </summary>
        public void Save()
        {
            SetCrmConnections(CrmConnections);
            _settingsStore.SetGuid(CollectionPath, SelectedConnectionIdPropertyName, SelectedConnectionId);
        }

        /// <summary>
        /// Reads and parses Crm Connections from settings store
        /// </summary>
        /// <returns>Returns Crm Connetions</returns>
        private CrmConnections GetCrmConnections()
        {
            if (_settingsStore.PropertyExists(CollectionPath, ConnectionsPropertyName) == false)
                return null;

            var connectionsXml = _settingsStore.GetString(CollectionPath, ConnectionsPropertyName);
            try
            {
                var connections = XmlSerializerHelper.Deserialize<List<ConnectionDetail>>(connectionsXml);
                var crmConnections = new CrmConnections { Connections = connections };
                var publisAfterUpload = true;
                if(_settingsStore.PropertyExists(CollectionPath, AutoPublishPropertyName))
                {
                    publisAfterUpload = _settingsStore.GetBoolean(CollectionPath, AutoPublishPropertyName);
                }
                crmConnections.PublishAfterUpload = publisAfterUpload;

                var ignoreExtensions = false;
                if (_settingsStore.PropertyExists(CollectionPath, IgnoreExtensionsProprtyName))
                {
                    ignoreExtensions = _settingsStore.GetBoolean(CollectionPath, IgnoreExtensionsProprtyName);
                }
                crmConnections.IgnoreExtensions = ignoreExtensions;


                var extendedLog = false;
                if (_settingsStore.PropertyExists(CollectionPath, ExtendedLogProprtyName))
                {
                    extendedLog = _settingsStore.GetBoolean(CollectionPath, ExtendedLogProprtyName);
                }
                crmConnections.ExtendedLog = extendedLog;

                foreach (var connection in crmConnections.Connections)
                {
                    if (!string.IsNullOrEmpty(connection.UserPassword))
                    {
                        connection.UserPassword = DecryptString(connection.UserPassword);
                    }
                }

                return crmConnections;
            }
            catch (Exception)
            {
                Logger.WriteLine("Failed to parse connection settings");
                return null;
            }
        }

        /// <summary>
        /// Writes Crm Connection to settings store
        /// </summary>
        /// <param name="crmConnections">Crm Connections to write to settings store</param>
        private void SetCrmConnections(CrmConnections crmConnections)
        {
            if(crmConnections?.Connections == null)
            {
                _settingsStore.DeletePropertyIfExists(CollectionPath, ConnectionsPropertyName);
                return;
            }
            Dictionary<Guid, string> passwordCache = new Dictionary<Guid, string>();
            foreach(var connection in crmConnections.Connections)
            {
                if (connection.ConnectionId != null && !passwordCache.ContainsKey(connection.ConnectionId.Value))
                {
                    passwordCache.Add(connection.ConnectionId.Value, connection.UserPassword);
                }

                if (!string.IsNullOrEmpty(connection.UserPassword) && connection.SavePassword)
                {
                    connection.UserPassword = EncryptString(connection.UserPassword);
                }
                else
                {
                    connection.UserPassword = null;
                }
            }

            var connectionsXml = XmlSerializerHelper.Serialize(crmConnections.Connections);
            _settingsStore.SetString(CollectionPath, ConnectionsPropertyName, connectionsXml);
            _settingsStore.SetBoolean(CollectionPath, AutoPublishPropertyName, crmConnections.PublishAfterUpload);
            _settingsStore.SetBoolean(CollectionPath, IgnoreExtensionsProprtyName, crmConnections.IgnoreExtensions);
            _settingsStore.SetBoolean(CollectionPath, ExtendedLogProprtyName, crmConnections.ExtendedLog);

            foreach (var connection in crmConnections.Connections)
            {
                if (connection.ConnectionId!= null && passwordCache.ContainsKey(connection.ConnectionId.Value))
                {
                    connection.UserPassword = passwordCache[connection.ConnectionId.Value];
                }
            }
        }

        /// <summary>
        /// Encrypts string
        /// </summary>
        /// <param name="input">String to encrypt</param>
        /// <returns>Encrypted sting</returns>
        private string EncryptString(string input)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(input),
                _entropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts string
        /// </summary>
        /// <param name="encryptedData">Encrypted string to decrypt</param>
        /// <returns>Decrypted string</returns>
        private string DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData =ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    _entropy,
                    DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(decryptedData);
            }
            catch
            {
                return null;
            }
        }
    }
}
