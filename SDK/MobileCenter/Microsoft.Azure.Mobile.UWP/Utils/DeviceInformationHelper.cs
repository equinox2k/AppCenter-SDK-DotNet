﻿using System;
using System.Threading;
using Windows.System.Profile;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Windows.UI.Xaml;

namespace Microsoft.Azure.Mobile.Utils
{
    public class DeviceInformationHelper : AbstractDeviceInformationHelper
    {
        private static bool _leftBackground;
        private static string _cachedScreenSize;
        public static event EventHandler InformationInvalidated;
        private static object _lockObject = new object();
        static DeviceInformationHelper()
        {
            CoreApplication.LeavingBackground += (o, e) => {
                lock (_lockObject)
                {
                    if (_leftBackground)
                    {
                        return;
                    }
                    DisplayInformation.DisplayContentsInvalidated += (displayInfo, obj) =>
                    {
                        RefreshDisplayCache();
                    };
                    _leftBackground = true;
                    RefreshDisplayCache();
                }
            };
        }

        //NOTE: This method MUST be called from the UI thread
        public static void RefreshDisplayCache()
        {
            lock (_lockObject)
            {

                DisplayInformation displayInfo = null;
                try
                {
                    displayInfo = DisplayInformation.GetForCurrentView();
                }
                catch (Exception)
                {
                    return;
                }
                if (_cachedScreenSize == ScreenSizeFromDisplayInfo(displayInfo))
                {
                    return;
                }
                _cachedScreenSize = ScreenSizeFromDisplayInfo(displayInfo);
                InformationInvalidated?.Invoke(null, EventArgs.Empty);
            }
        }

        private static string ScreenSizeFromDisplayInfo(DisplayInformation displayInfo)
        {
            return $"{displayInfo.ScreenWidthInRawPixels}x{displayInfo.ScreenHeightInRawPixels}";
        }

        protected override string GetSdkName()
        {
            return "mobilecenter.uwp";
        }

        protected override string GetDeviceModel()
        {
            var deviceInfo = new EasClientDeviceInformation();
            return deviceInfo.SystemProductName;
        }

        protected override string GetAppNamespace()
        {
            return Application.Current.GetType().Namespace;
        }

        protected override string GetDeviceOemName()
        {
            var deviceInfo = new EasClientDeviceInformation();
            return deviceInfo.SystemManufacturer;
        }

        protected override string GetOsName()
        {
            var deviceInfo = new EasClientDeviceInformation();
            return deviceInfo.OperatingSystem;
        }

        protected override string GetOsBuild()
        {
            // Adapted from https://social.msdn.microsoft.com/Forums/en-US/2d8a7dab-1bad-4405-b70d-768e4cb2af96/uwp-get-os-version-in-an-uwp-app?forum=wpdevelop
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var major = (version & 0xFFFF000000000000L) >> 48;
            var minor = (version & 0x0000FFFF00000000L) >> 32;
            var build = (version & 0x00000000FFFF0000L) >> 16;
            var revision = version & 0x000000000000FFFFL;

            return $"{major}.{minor}.{build}.{revision}";
        }

        protected override string GetOsVersion()
        {
            // Adapted from https://social.msdn.microsoft.com/Forums/en-US/2d8a7dab-1bad-4405-b70d-768e4cb2af96/uwp-get-os-version-in-an-uwp-app?forum=wpdevelop
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var major = (version & 0xFFFF000000000000L) >> 48;
            var minor = (version & 0x0000FFFF00000000L) >> 32;
            var build = (version & 0x00000000FFFF0000L) >> 16;
            return $"{major}.{minor}.{build}";
        }

        protected override string GetAppVersion()
        {
            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }

        protected override string GetAppBuild()
        {
            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }

        protected override string GetScreenSize()
        {
            lock (_lockObject)
            {
                return _cachedScreenSize;
            }
        }
    }
}
