using System.Xml;

// Scan for current folder and get a list of tcx files
string[] filesInFolder = Directory.GetFiles(".");

var tcxFiles = filesInFolder
    .Where(file => file.EndsWith(".tcx", StringComparison.InvariantCultureIgnoreCase));

if (!tcxFiles.Any())
{
    Console.WriteLine("No .tcx file found, finishing");
    Environment.Exit(0);
}

Console.WriteLine("Found {0} .tcx files", tcxFiles.Count());

foreach (string file in tcxFiles)
{
    Console.WriteLine("-------------------");
    Console.WriteLine("Started fix of {0}", file);

    // Load the file
    XmlDocument doc = new();
    doc.Load(file);

    // Register the namescape of the tcx scheme
    XmlNamespaceManager xmlNamespace = new(doc.NameTable);
    xmlNamespace.AddNamespace("x", "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2");

    // Obtain all <Trackpoint> nodes from file
    XmlNodeList trackpoints = doc.SelectNodes("//x:Trackpoint", xmlNamespace)!;

    Console.WriteLine("{0} trackpoints found in total", trackpoints.Count);
    int nodesRemoved = 0;

    // foreach node we will check if the trackpoint is valid
    foreach (XmlNode trackpoint in trackpoints)
    {
        // Get the coordinates nodes
        XmlNode latitudeNode = trackpoint.SelectSingleNode("x:Position/x:LatitudeDegrees", xmlNamespace)!;
        XmlNode longitudeNode = trackpoint.SelectSingleNode("x:Position/x:LongitudeDegrees", xmlNamespace)!;

        // My corrupted tcx file had invalid coordinates with 'E-'
        // And I used this as an condition

        // Check if lat value contains 'E-'
        if (latitudeNode != null && latitudeNode.InnerText.Contains("E-"))
        {
            // Remove node
            trackpoint.ParentNode!.RemoveChild(trackpoint);
            nodesRemoved++;

            continue;
        }

        // Check if lon value contains 'E-'
        if (longitudeNode != null && longitudeNode.InnerText.Contains("E-"))
        {
            // Remove node
            trackpoint.ParentNode!.RemoveChild(trackpoint);
            nodesRemoved++;
        }
    }

    // Salvar as alterações de volta ao arquivo
    if (nodesRemoved > 0)
    {
        string newFileName = file + "_fixed.tcx";

        doc.Save(newFileName);
        Console.WriteLine("{0} Trackpoints successfully removed.", nodesRemoved);
    }
    else
    {
        Console.WriteLine("No Trackpoints removed");
    }
}