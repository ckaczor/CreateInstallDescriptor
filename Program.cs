using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using WindowsInstaller;

namespace CreateInstallDescriptor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Make sure we received two parameters
            if (args.Length != 2)
            {
                Console.WriteLine("Must supply two parameters: <input file> <output XML file>");
                return;
            }

            // Extract the parameters
            var inputFileName = args[0];
            var outputFileName = args[1];

            // Get the info for the input file
            var inputFile = new FileInfo(inputFileName);

            var extension = inputFile.Extension;

            string version;

            if (extension.Equals(".msi", StringComparison.InvariantCultureIgnoreCase))
            {
                // Get the type of the Windows Installer object
                var installerType = Type.GetTypeFromProgID("WindowsInstaller.Installer");

                // Create the Windows Installer object
                var installer = (Installer) Activator.CreateInstance(installerType);

                // Open the MSI database in the input file
                var database = installer.OpenDatabase(inputFileName, MsiOpenDatabaseMode.msiOpenDatabaseModeReadOnly);

                // Open a view on the Property table for the version property
                var view = database.OpenView("SELECT * FROM Property WHERE Property = 'ProductVersion'");

                // Execute the view query
                view.Execute();

                // Get the record from the view
                var record = view.Fetch();

                // Get the version from the data
                version = record.StringData[2];
            }
            else if (extension.Equals(".exe", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                // Just use the version info from the file
                version = FileVersionInfo.GetVersionInfo(inputFileName).FileVersion;
            }
            else
            {
                throw new Exception("Unsupported file extension");
            }

            // -----

            // Create the XML writer
            var xmlWriter = new XmlTextWriter(outputFileName, null) { Formatting = Formatting.Indented };

            // Write the root tag
            xmlWriter.WriteStartElement("versionInformation");

            // Write the version tag
            xmlWriter.WriteStartElement("version");
            xmlWriter.WriteValue(version);
            xmlWriter.WriteEndElement();

            // Write the installer name tag
            xmlWriter.WriteStartElement("installFile");
            xmlWriter.WriteValue(Path.GetFileName(inputFileName));
            xmlWriter.WriteEndElement();

            // Write the installer date tag
            xmlWriter.WriteStartElement("installCreated");
            xmlWriter.WriteValue(DateTime.Now);
            xmlWriter.WriteEndElement();

            // End the root tag
            xmlWriter.WriteEndElement();

            // Close out the writer
            xmlWriter.Flush();
            xmlWriter.Close();
        }
    }
}
