using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content.PM;
using Android.Util;
using Java.IO;

namespace RootDevelop.RootDetection
{
    public class RootChecker
    {
        private readonly PackageManager _packageManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageManager"></param>
        public RootChecker(PackageManager packageManager)
        {
            _packageManager = packageManager;
        }


        /// <summary>
        /// Checks if we think there's a good *indication* of root | false good *indication* of no root
        /// </summary>
        /// <returns><c>true</c>, if a indication of root is detected, <c>false</c> otherwise.</returns>
        public bool IsRooted()
        {
            return DetectTestKeys()
             || CheckForDangerousProps()
             || CheckSuExists()
             || CheckForRwPaths()
             || DetectRootManagementApps()
             || DetectRootCloakingApps()
             || DetectPotentiallyDangerousApps()
             || CheckForSuBinary()
             || CheckForBusyBoxBinary();
        }


        /// <summary>
        /// Release-Keys and Test-Keys has to do with how the kernel is signed when it is compiled.
        /// Test-Keys means it was signed with a custom key generated by a third-party developer.
        /// </summary>
        /// <returns><c>true</c>, if signed with Test-keys, <c>false</c> otherwise.</returns>
        private static bool DetectTestKeys()
        {
            var buildTags = Android.OS.Build.Tags;

            return buildTags != null && buildTags.Contains("test-keys");
        }

        /// <summary>
        /// Checks for several dangerous properties (ro.debuggable == 1 or ro.secure==0)
        /// </summary>
        /// <returns><c>true</c>, if dangerous properties are found, <c>false</c> otherwise.</returns>
        private static bool CheckForDangerousProps()
        {
            var activeProps = Utils.PropsReader();

            return activeProps.Contains("[ro.secure]: [0]") || activeProps.Contains("[ro.debuggable]: [1]");
        }

        /// <summary>
        /// A variation on the checking for SU, this attempts a 'which su'
        /// </summary>
        /// <returns><c>true</c>, if su exists, <c>false</c> otherwise.</returns>
        private static bool CheckSuExists()
        {
            Java.Lang.Process process = null;
            try
            {
                var command = new[] { "/system/xbin/which", "su" };
                process = Java.Lang.Runtime.GetRuntime().Exec(command);
                var s = new BufferedReader(new InputStreamReader(process.InputStream));
                return s.ReadLine() != null;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                process?.Destroy();
            }
        }


        /// <summary>
        /// Checks if system directories are writeable.
        /// </summary>
        /// <returns><c>true</c>, if one or more system directories are writeable, <c>false</c> otherwise.</returns>
        private static bool CheckForRwPaths()
        {
            var result = false;

            var lines = Utils.MountReader();
            foreach (var line in lines)
            {
                // Split lines into parts
                var args = line.Split(' ');

                if (args.Length < 4)
                {
                    // If we don't have enough options per line, skip this and log an error
                    Log.Error("CheckForRWPaths", $"Error formatting mount line: {line}");
                    continue;
                }

                var mountPoint = args[1];
                var mountOptions = args[3];

                foreach (var pathToCheck in Constants.PathsThatShouldNotBeWriteable)

                    if (mountPoint.Equals(pathToCheck, StringComparison.InvariantCultureIgnoreCase))
                    {

                        // Split options out and compare against "rw" to avoid false positives
                        if (mountOptions.Split(',').Any(option => option.Equals("rw", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Log.Info("CheckForRWPaths", $"{pathToCheck} path is mounted with rw permissions! {line}");
                            result = true;
                        }
                    }
            }

            return result;
        }


        /// <summary>
        /// Detects root management apps.
        /// </summary>
        /// <returns><c>true</c>, if root management apps was detected, <c>false</c> otherwise.</returns>
        private bool DetectRootManagementApps()
        {
            return IsAnyPackageFromListInstalled(Constants.KnownRootAppsPackages);
        }

        /// <summary>
        /// Detects root cloaking apps.
        /// </summary>
        /// <returns><c>true</c>, if root cloaking apps was detected, <c>false</c> otherwise.</returns>
        private bool DetectRootCloakingApps()
        {
            return IsAnyPackageFromListInstalled(Constants.KnownRootCloakingPackages);
        }

        /// <summary>
        /// Detects potentially dangerous apps.
        /// </summary>
        /// <returns><c>true</c>, if potentially dangerous apps was detected, <c>false</c> otherwise.</returns>
        private bool DetectPotentiallyDangerousApps()
        {
            return IsAnyPackageFromListInstalled(Constants.KnownDangerousAppsPackages);
        }

        /// <summary>
        /// Checks for su binary.
        /// </summary>
        /// <returns><c>true</c>, if for su binary was checked, <c>false</c> otherwise.</returns>
        private static bool CheckForSuBinary()
        {
            return Utils.CheckForBinary("su");
        }

        /// <summary>
        /// Checks for busy box binary.
        /// </summary>
        /// <returns><c>true</c>, if for busy box binary was checked, <c>false</c> otherwise.</returns>
        private static bool CheckForBusyBoxBinary()
        {
            return Utils.CheckForBinary("busybox");
        }


        /// <summary>
        /// Checks if any package from list is installed.
        /// </summary>
        /// <returns><c>true</c>, if any package from list installed was ised, <c>false</c> otherwise.</returns>
        /// <param name="packages">Packages.</param>
        private bool IsAnyPackageFromListInstalled(IEnumerable<string> packages)
        {
            var result = false;

            var pm = _packageManager;

            foreach (var packageName in packages)
            {
                try
                {
                    // Root app detected
                    pm.GetPackageInfo(packageName, 0);
                    Log.Info("IsAnyPackageFromListInstalled", "{packageName} ROOT management app detected!");
                    result = true;
                }
                catch (PackageManager.NameNotFoundException)
                {
                    // Exception thrown, package is not installed into the system
                }
            }

            return result;
        }


    }
}
