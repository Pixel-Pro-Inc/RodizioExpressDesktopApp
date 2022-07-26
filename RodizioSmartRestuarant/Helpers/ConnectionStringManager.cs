using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using RodizioSmartRestuarant.Exceptions;

namespace RodizioSmartRestuarant.Helpers
{
    /// <summary>
    /// This is supposedd to be a class that will give in the application settings for both prod and dev environments, ( database and SMS api keys and such)s
    /// </summary>
    public static class ConnectionStringManager
    {

        /// <summary>
        /// This takes in the variable name we are looking for in the config file and returns the value
        /// <para>
        /// Retrieve a connection string by specifying the name.
        /// </para>
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns> The setting value stored in the config file. Eg, the basepath of the firebaseDatabaseSettings </returns>
        public static string GetConnectionString(string variableName)
        {
            // This is so the data is unencrypted
            ToggleConectionStringConfigEncryption();

            // Return null on failure.
            string returnValue = null;

            // FIXME: Change all the default paths to the path of the exe file that the user will be using
            // Get the collection of connection strings.
            ConnectionStringsSection  section= GetConnectionStringSection();

            if (section == null) throw new NoConnectionStringSectionFound();
            foreach (ConnectionStringSettings cs in section.ConnectionStrings)
            {
                //Checks if the connectionString exists
                if (cs.Name == variableName)
                {
                    return returnValue = cs.ConnectionString;
                }
            }
            // This is so the data is re encrypted
            ToggleConectionStringConfigEncryption();

            return returnValue;
        }
        /// <summary>
        /// This gets the connectionstring section in the config file of the exe found in the default <paramref name="path"/>. You can then search through the results for what you are looking for
        /// </summary>
        /// <param name="path"> Wher the application exe is stored</param>
        /// <returns></returns>
        public static ConnectionStringsSection GetConnectionStringSection(string path= "C:/Users/cash/source/repos/Pixel-Pro-Inc/RodizioExpressDesktopApp/RodizioSmartRestuarant/bin/Debug/RodizioSmartRestuarant.exe")
        {
            // Manually finds the exe we need
            System.Configuration.Configuration config = GetConfigfile(path);
            if (config == null) throw new NullReferenceException("There isn't an exe in the path you defined or at least one with a usable config file");
            return FindSection(config, "connectionStrings");
        }
        /// <summary>
        /// An overload that takes in the config file to get the connectionString
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ConnectionStringsSection GetConnectionStringSection(System.Configuration.Configuration config)
        {
            if (config == null) throw new NullReferenceException("You have to provide a nonNull configuration argument or at least one a usable config file");
            return FindSection(config, "connectionStrings");
        }

        // NOTE: Apparently, .NET automatically decrypts the data for you so you don't have to decrypt or test the decryption of data, check description for more
        // NOTE: This works the way its supposed to. I know this cause it moves between each conditional block one after another with each time it passes through it
        /// <summary>
        /// This encrypts/unencrypts the connectionstrings in its configuration file where the settings are be stored
        /// 
        /// <para>Apparently, .NET automatically decrypts the data for you so you don't have to decrypt or test the decryption of data, check the following link: <href>https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/connection-strings-and-configuration-files#encrypting-configuration-file-sections-using-protected-configuration</href> </para>
        /// </summary>
        private static void ToggleConectionStringConfigEncryption()
        {
            try
            {
                System.Configuration.Configuration config = GetConfigfile();
                // NOTE: We don't use the path overload method here cause we need to save the config details
                ConnectionStringsSection section = GetConnectionStringSection(config);

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
                // Save the current configuration. DO NOT REMOVE THIS LINE unless you are sure you can save the work someother way
                config.Save();
            }
            catch (Exception ex)
            {
                // REFACTOR: Add a logger here
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Open the configuration file and retrieve
        /// the specified section.
        /// </summary>
        /// <param name="config"> The Configuration file of the exe you are looking for</param>
        /// <param name="sectionName"> eg appsettings or connectionStrings</param>
        /// <returns></returns>
        private static ConnectionStringsSection FindSection(System.Configuration.Configuration config, string sectionName)
        {
            ConnectionStringsSection section =
                config.GetSection(sectionName)
                as ConnectionStringsSection;
            return section;
        }
        private static System.Configuration.Configuration GetConfigfile(string path= "C:/Users/cash/source/repos/Pixel-Pro-Inc/RodizioExpressDesktopApp/RodizioSmartRestuarant/bin/Debug/RodizioSmartRestuarant.exe")
        {
            return ConfigurationManager.OpenExeConfiguration(path);
        }

    }
}
