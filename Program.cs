using System;
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
                Console.WriteLine("Must supply two parameters: <input MSI file> <output XML file>");
                return;
            }

            // Extract the parameters
            string inputFile = args[0];
            string outputFile = args[1];

            // Get the type of the Windows Installer object
            Type installerType = Type.GetTypeFromProgID("WindowsInstaller.Installer");

            // Create the Windows Installer object
            Installer installer = (Installer)Activator.CreateInstance(installerType);

            // Open the MSI database in the input file
            Database database = installer.OpenDatabase(inputFile, MsiOpenDatabaseMode.msiOpenDatabaseModeReadOnly);

            // Open a view on the Property table for the version property
            View view = database.OpenView("SELECT * FROM Property WHERE Property = 'ProductVersion'");

            // Execute the view query
            view.Execute(null);

            // Get the record from the view
            Record record = view.Fetch();

            // Get the version from the data
            string version = record.get_StringData(2);

            // -----

            // Create the XML writer
            XmlTextWriter xmlWriter = new XmlTextWriter(outputFile, null) { Formatting = Formatting.Indented };

            // Write the root tag
            xmlWriter.WriteStartElement("versionInformation");

            // Write the version tag
            xmlWriter.WriteStartElement("version");
            xmlWriter.WriteValue(version);
            xmlWriter.WriteEndElement();

            // Write the installer name tag
            xmlWriter.WriteStartElement("installFile");
            xmlWriter.WriteValue(Path.GetFileName(inputFile));
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
