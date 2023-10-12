#if UNITY_EDITOR && UNITY_ANDROID
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;
using System;
using UnityEngine;
using System.Collections.Generic;
using VoxelBusters.CoreLibrary;
using System.Linq;

namespace VoxelBusters.EssentialKit.Editor.Android
{
    public class AndroidGradleDependenciesPostProcessor : IPostGenerateGradleAndroidProject
    {
        private static string kLibraryPrefix                = $"{EssentialKitPackageLayout.AndroidProjectRootNamespace}";
        private static string kLibraryExtension             = "aar";
        private static string kAddressBookLibrary           = LibraryFileName("addressbook");
        private static string kBillingServicesLibrary       = LibraryFileName("billingservices");
        private static string kCloudServicesLibrary         = LibraryFileName("cloudservices");
        private static string kGameServicesLibrary          = LibraryFileName("gameservices");
        private static string kMediaServicesLibrary         = LibraryFileName("mediaservices");
        private static string kNativeUILibrary              = LibraryFileName("uiviews");
        private static string kNetworkServicesLibrary       = LibraryFileName("networkservices");
        private static string kNotificationServicesLibrary  = LibraryFileName("notificationservices");
        private static string kSharingServicesLibrary       = LibraryFileName("sharingservices");
        private static string kWebViewLibrary               = LibraryFileName("webview");
        private static string kDeepLinkServicesLibrary      = LibraryFileName("deeplinkservices");
        private static string kExtrasLibrary                = LibraryFileName("extras");
        private static string kSocialAuthLibrary            = LibraryFileName("socialauth");

        private static string LibraryFileName(string module) => $"{kLibraryPrefix}-{module}.{kLibraryExtension}";

        Dictionary<string, string> m_libraryMap = new Dictionary<string, string>()
        {
            { NativeFeatureType.kAddressBook,           kAddressBookLibrary},
            { NativeFeatureType.kBillingServices,       kBillingServicesLibrary},
            { NativeFeatureType.kCloudServices,         kCloudServicesLibrary},
            { NativeFeatureType.kGameServices,          kGameServicesLibrary},
            { NativeFeatureType.kMediaServices,         kMediaServicesLibrary},
            { NativeFeatureType.kNativeUI,              kNativeUILibrary},
            { NativeFeatureType.kNetworkServices,       kNetworkServicesLibrary },
            { NativeFeatureType.kNotificationServices,  kNotificationServicesLibrary },
            { NativeFeatureType.KSharingServices,       kSharingServicesLibrary },
            { NativeFeatureType.kWebView,               kWebViewLibrary },
            { NativeFeatureType.kDeepLinkServices,      kDeepLinkServicesLibrary },
            { NativeFeatureType.kExtras,                kExtrasLibrary }
        };

        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            if (!EssentialKitSettingsEditorUtility.SettingsExists)
            {
                EssentialKitSettingsEditorUtility.ShowSettingsNotFoundErrorDialog();
                return;
            }

            List<string> deletedLibraries = new List<string>();

            foreach(string eachFeature in m_libraryMap.Keys)
            {
                
                if(!EssentialKitSettings.Instance.IsFeatureUsed(eachFeature))
                {
                    try
                    {
                        string libraryName = m_libraryMap[eachFeature];
                        UpdateDeletedLibrary(basePath, libraryName, deletedLibraries);
                    }
                    catch(Exception e)
                    {
                        DebugLogger.LogError("Failed finding the library file to delete : " + e);
                    }
                }
            }

            //Special cases
            if (!EssentialKitSettings.Instance.IsFeatureUsed(NativeFeatureType.kGameServices) && !EssentialKitSettings.Instance.IsFeatureUsed(NativeFeatureType.kCloudServices))
            {
                UpdateDeletedLibrary(basePath, kSocialAuthLibrary, deletedLibraries);
            }

            //Update build.gradle file
            UpdateGradleFile(basePath, deletedLibraries);
        }

        public int callbackOrder { get { return 1; } }

        private void UpdateDeletedLibrary(string basePath, string libraryName, List<string> deletedList)
        {
            IOServices.DeleteFile(Path.Combine(basePath, "libs", libraryName), true);
            deletedList.Add(libraryName.Replace(".aar", ""));
        }

        private void UpdateGradleFile(string basePath, List<string> deletedLibraries)
        {
            string gradlePath = IOServices.CombinePath(basePath, "build.gradle");
            Debug.Log("Updating build.gradle at : " + gradlePath);
            string[] lines = File.ReadAllLines(gradlePath);
            StringBuilder builder = new StringBuilder();

            foreach (string each in lines)
            {
                if(!deletedLibraries.Any(eachLib => each.Contains(eachLib)))
                {
                    builder.AppendLine(each);
                }
            }
            File.WriteAllText(gradlePath, builder.ToString());
        }
    }
}
#endif
