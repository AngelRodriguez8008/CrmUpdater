using Microsoft.Xrm.Sdk.Client;
using System;
using System.ServiceModel.Description;

namespace McTools.Xrm.Connection
{
    public static class CrmConnectionHelper
    {
        public static OrganizationServiceProxy GetOrganizationServiceProxy(ConnectionDetail detail)
        {
            var serviceUrl = detail.OrganizationServiceUrl;
            string homeRealmUrl = null;

            ClientCredentials clientCredentials = new ClientCredentials();
            ClientCredentials deviceCredentials = null;

            if (detail.IsCustomAuth)
            {
                string username = detail.UserName;
                if (!string.IsNullOrEmpty(detail.UserDomain))
                {
                    username = $"{detail.UserDomain}\\{detail.UserName}";
                }
                clientCredentials.UserName.UserName = username;
                clientCredentials.UserName.Password = detail.UserPassword;
            }

            if (detail.UseOnline && !detail.UseOsdp)
            {
                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));
            }

            if (detail.UseIfd && !string.IsNullOrEmpty(detail.HomeRealmUrl))
            {
                homeRealmUrl = detail.HomeRealmUrl;
            }

            Uri serviceUri = new Uri(serviceUrl);
            Uri homeRealmUri = homeRealmUrl == null ? null : Uri.IsWellFormedUriString(homeRealmUrl, UriKind.RelativeOrAbsolute) ? new Uri(homeRealmUrl) : null;

            return new OrganizationServiceProxy(serviceUri, homeRealmUri, clientCredentials, deviceCredentials);
        }

        public static DiscoveryServiceProxy GetDiscoveryService(ConnectionDetail detail) {

            var serviceUrl = detail.GetDiscoveryServiceUrl();
            string homeRealmUrl = null;
            ClientCredentials clientCredentials = new ClientCredentials();
            ClientCredentials deviceCredentials = null;

            if (detail.IsCustomAuth)
            {
                string username = detail.UserName;
                if (!string.IsNullOrEmpty(detail.UserDomain))
                {
                    username = $"{detail.UserDomain}\\{detail.UserName}";
                }
                clientCredentials.UserName.UserName = username;
                clientCredentials.UserName.Password = detail.UserPassword;
            }

            if (detail.UseOnline && !detail.UseOsdp)
            {
                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));
            }

            if (detail.UseIfd && !string.IsNullOrEmpty(detail.HomeRealmUrl))
            {
                homeRealmUrl = detail.HomeRealmUrl;
            }

            Uri serviceUri = new Uri(serviceUrl);
            Uri homeRealmUri = homeRealmUrl == null ? null : Uri.IsWellFormedUriString(homeRealmUrl, UriKind.RelativeOrAbsolute) ?  new Uri(homeRealmUrl) : null;

            return new DiscoveryServiceProxy(serviceUri, homeRealmUri, clientCredentials, deviceCredentials);
        }
        
    }
}
