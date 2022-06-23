using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace RodizioSmartRestuarant.Helpers
{
    /// <summary>
    /// This is supposedd to be a class that will give in the application settings for both prod and dev environments, ( database and SMS api keys and such)s
    /// </summary>
    // NOTE: For some weird reason, the App.config refuses to be programtically checked for values. I spent an entire day trying to figure it out but come up with nothing
    [Obsolete]
    public static class ConnectionStringManager
    {

        public static string ConnectionStrings { get; set; }

        /// <summary>
        /// This takes in the name of the application so that it can encrypt/unencrypt connectionstrings in its configuration file where the settings will be stored
        /// </summary>
        /// <param name="exeFile">This should be the application name, the default being the name RodizioSmartRestuarant</param>
        private static void ToggleConfigEncryption(string exeFile= "C:/Users/cash/source/repos/Pixel-Pro-Inc/RodizioExpressDesktopApp/RodizioSmartRestuarant/bin/Debug/app.publish/RodizioSmartRestuarant.exe")
        {
            //Takes the filename and removes the .config extension if it exists
            string exeConfigName = exeFile.Replace(".config", "");
            // Takes the executable file name without the
            // .config extension.
            try
            {
                // REFACTOR: Consider just having configuration from system instead of opening from OpenExeConfiguration
                System.Configuration.Configuration config = ConfigurationManager.
                                OpenExeConfiguration(exeConfigName);
                ConnectionStringsSection section = FindconnectionStringsSection(config);


                // TESTING: We don't want the connections strings to be encrypted yet but it should be removed when done
                if (section.SectionInformation.IsProtected)
                {
                    // Remove encryption.
                    section.SectionInformation.UnprotectSection();
                }
                else
                {
                    // Encrypt the section.
                    section.SectionInformation.ProtectSection(
                        "DataProtectionConfigurationProvider");
                }
                // Save the current configuration.
                config.Save();

                Console.WriteLine("Protected={0}",
                    section.SectionInformation.IsProtected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Open the configuration file and retrieve
        // the connectionStrings section.
        private static ConnectionStringsSection FindconnectionStringsSection(System.Configuration.Configuration config)
        {
            ConnectionStringsSection section =
                config.GetSection("connectionStrings")
                as ConnectionStringsSection;
            return section;
        }

        /// <summary>
        /// This takes in the variable name we are looking for in the config file and returns the value
        /// <para>
        /// Retrieve a connection string by specifying the providerName.
        /// Assumes one connection string per provider in the config file.
        /// </para>
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="variableName"></param>
        /// <returns> The setting value stored in the config file. Eg, the basepath of the firebaseDatabaseSettings </returns>
        public static string GetConnectionString(string providerName, string variableName)
        {
            // This is so the data is unencrypted
            ToggleConfigEncryption();

            // Return null on failure.
            string returnValue = null;

            // Get the collection of connection strings.
            ConnectionStringSettingsCollection settings =
                ConfigurationManager.ConnectionStrings;

            // Walk through the collection and return the first
            // connection string matching the providerName.
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.ProviderName == providerName&& cs.Name==variableName)
                        returnValue = cs.ConnectionString;
                    break;
                }
            }

            // This is so the data is re encrypted
            ToggleConfigEncryption();

            return returnValue;
        }

        public static string GetConnection(string variableName="BasePath")
        {
            string result = null;

            System.Configuration.Configuration config = ConfigurationManager.
                                OpenExeConfiguration("C:/Users/cash/source/repos/Pixel-Pro-Inc/RodizioExpressDesktopApp/RodizioSmartRestuarant/bin/Debug/RodizioSmartRestuarant.exe");
            ConnectionStringsSection section = FindconnectionStringsSection(config);
           
            if (section != null)
            {
                foreach (ConnectionStringSettings cs in section.ConnectionStrings)
                {
                    //if (cs.Name == "BasePath")
                    //{
                    //    result = cs.ConnectionString;
                    //    Console.WriteLine(result);
                    //    Console.WriteLine(cs.ConnectionString);
                    //}
                    if (cs.Name == "RodizioSmartRestuarant.Properties.Settings.BasePath")
                    {
                        return result = "https://rodizoapp-default-rtdb.firebaseio.com/";
                    }

                }
            }
            return result;
        }
    }
}
