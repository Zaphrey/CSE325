using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

var currentDirectory = Directory.GetCurrentDirectory();
var storesDirectory = Path.Combine(currentDirectory, "stores");

var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
Directory.CreateDirectory(salesTotalDir);

var salesFiles = FindFiles(storesDirectory);
// var salesTotal = CalculateSalesTotal(salesFiles);

// File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");
GenerateSalesReport(salesFiles);

IEnumerable<string> FindFiles(string folderName) 
{
    var salesFiles = new List<string>();
    var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);

    foreach (string file in foundFiles)
    {
        string extension = Path.GetExtension(file);

        if (extension == ".json")
        {
            salesFiles.Add(file);
        }
    }

    return salesFiles;
}

double CalculateSalesTotal(IEnumerable<string> salesFiles) {
    double salesTotal = 0;

    foreach (var file in salesFiles) {
        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
        salesTotal += data?.Total ?? 0;
    }

    return salesTotal;
}

void GenerateSalesReport(IEnumerable<string> salesFiles)
{
    double completeTotal = 0;
    string date = DateTime.Now.ToString("MM/dd/yyyy");
    File.WriteAllText(Path.Combine(salesTotalDir, "totals.txt"), ($"Sales Summary {date}"));
    // Dividing line
    File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{Environment.NewLine}-----------------------------------------------------{Environment.NewLine}");
    
    // Calculate OverallTotal summary
    foreach (var file in salesFiles)
    {
        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
        
        completeTotal += data?.OverallTotal ?? 0;
    }

    // Write total summary line along with the details starting line
    File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), ($" Total: {completeTotal.ToString("C")}{Environment.NewLine}"));
    File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $" {Environment.NewLine} Details:{Environment.NewLine}");

    // Write each detail line to the file
    foreach (var file in salesFiles) {
        // We could cache this in the earlier loop for later use, but I wanted to keep it simple for now. 
        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);

        double total = data?.OverallTotal ?? 0;
        
        // Only count the store file if the file's OverallTotal contributes to the total
        if (total > 0)
        {
            // Split the path from the stores file, and add stores back into the path. This gives us our local path to the store file.
            List<string> pathParts = new List<string>(file.Split("stores"));

            // Get the substring which cuts off the first character. 
            // This needs to be done since Path doesn't combine if there's a separator character at the start of the path.
            string secondPath = pathParts[1].Substring(1);
            string storePath = $"{Path.DirectorySeparatorChar}{Path.Combine("stores", secondPath)}";
            string formattedTotal = total.ToString("C");
            File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"   {storePath}:{formattedTotal}{Environment.NewLine}");
        }
    }
}

record SalesData (double Total, double OverallTotal);