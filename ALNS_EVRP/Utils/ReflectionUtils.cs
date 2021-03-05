using System;
using System.IO;
using System.Reflection;

namespace ALNS_EVRP.Utils
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Given an assembly and the name of the resource, load it as text 
        /// Used to read embedded files in the Dll
        /// </summary>
        /// <param name="assembly">assembly that contain the resource</param>
        /// <param name="resourceName">complete path of embedded resource</param>
        /// <returns></returns>
        public static string ReadEmbeddedResourceAsText(Assembly assembly, string resourceName)
        {
            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException($"{resourceName} missing", ex);
            }
        }
    }
}
