using System;
using Android.Util;
using Java.IO;
using Java.Util;

namespace RootDevelop.RootDetection
{
    internal static class Utils
    {
        /// <summary>
        /// Checks if binary exists.
        /// </summary>
        /// <returns><c>true</c>, if binary exists, <c>false</c> otherwise.</returns>
        /// <param name="filename">Filename.</param>
        public static bool CheckForBinary(string filename)
        {

            bool result = false;

            foreach (var path in Constants.SuPaths)
            {
                var completePath = path + filename;
                var f = new File(completePath);
                var fileExists = f.Exists();
                if (fileExists)
                {
                    Log.Info("CheckForBinary", $"{completePath} binary detected!");
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Reads all android props
        /// </summary>
        /// <returns>The reader.</returns>
        public static string[] PropsReader()
        {
            System.IO.Stream inputstream = null;
            try
            {
                inputstream = Java.Lang.Runtime.GetRuntime().Exec("getprop").InputStream;
            }
            catch (IOException ex)
            {
                Log.Error("propsReader", $"Unable to retrieve inputstream {ex.Message}");
            }
            var propval = "";
            try
            {
                propval = new Scanner(inputstream).UseDelimiter("\\A").Next();

            }
            catch (NoSuchElementException ex)
            {
                Log.Error("propsReader", $"NoSuchElementException: {ex.Message}");
            }

            return propval.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
        }

        /// <summary>
        /// Read mount points
        /// </summary>
        /// <returns>The reader.</returns>
        public static string[] MountReader()
        {
            System.IO.Stream inputstream = null;
            try
            {
                inputstream = Java.Lang.Runtime.GetRuntime().Exec("mount").InputStream;
            }
            catch (IOException ex)
            {
                Log.Error("propsReader", $"Unable to retrieve inputstream {ex.Message}");
            }

            // If input steam is null, we can't read the file, so return null
            if (inputstream == null) return null;

            var propval = "";
            try
            {
                propval = new Scanner(inputstream).UseDelimiter("\\A").Next();
            }
            catch (Java.Util.NoSuchElementException ex)
            {
                Log.Error("propsReader", $"NoSuchElementException: {ex.Message}");
            }

            return propval.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
        }
    }
}