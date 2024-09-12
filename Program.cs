using DocuWare.Platform.ServerClient;
using Microsoft.Extensions.Configuration;

namespace ConsoleAppDwSimple
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //load settings from appsettings.json
            IConfigurationRoot config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddUserSecrets<Program>()
                        .Build();


            string mydwPlatformUrl = config.GetSection("DocuWare")["PlatformUrl"] ?? "";
            string mydwUsername = config.GetSection("DocuWare")["Username"] ?? "";
            string mydwPassword = config.GetSection("DocuWare")["Password"] ?? "";
            string mydwFileCabinetGuid = config.GetSection("DocuWare")["FileCabinetGuid"] ?? "";


            byte[] simpleDoc;

            bool useStream = false;

            // could load from GetBytes since we are create text file
            if (useStream == false)
            {
                simpleDoc = System.Text.Encoding.UTF8.GetBytes($"Hello DocuWare - From text on this fine day {DateTime.Now.ToLongDateString()}");
            }
            else
            {
                //load from stream ... could be file stream etc.
                using (StreamWriter sw = new StreamWriter(new MemoryStream()))
                {
                    sw.WriteLine($"Hello DocuWare - From stream on this fine day {DateTime.Now.ToLongDateString()}");
                    sw.Flush();
                    simpleDoc = ((MemoryStream)sw.BaseStream).ToArray();
                }
            }

            //docid result placeholder
            int storedDocid = -1;

           await SimpleDw.WithParameters(mydwPlatformUrl, mydwUsername, mydwPassword, mydwFileCabinetGuid)
                .UploadToDocuWare((details) => Console.WriteLine(details), simpleDoc, "text/plain", () =>
                {
                    //passing index fields in this function
                    return new List<DocumentIndexField>() 
                    { 
                        DocumentIndexField.Create("DOCUMENT_TYPE", "External Document") 
                    };
                }).ContinueWith((t) =>
           {
                 if (t.IsFaulted) Console.WriteLine("Upload failed with exception " + t.Exception.Message);
                 else if (t.IsCompleted) Console.WriteLine("Upload complete with document id " + (storedDocid = t.Result));
                 else Console.WriteLine("Upload failed");
           });

            if (storedDocid >= 0)
            {
                // create quick and dirty url
                DocuWare.WebIntegration.DWIntegrationUrl dWIntegrationUrl = new DocuWare.WebIntegration.DWIntegrationUrl(new DocuWare.WebIntegration.DWIntegrationInfo(mydwPlatformUrl + "/WebClient", false)
                {
                    Scheme = "https",
                }, DocuWare.WebIntegration.IntegrationType.ResultlistAndViewer);

                //create the parameters for the url
                dWIntegrationUrl.Parameters = new DocuWare.WebIntegration.DWIntegrationUrlParameters(DocuWare.WebIntegration.IntegrationType.ResultlistAndViewer)
                {
                    FileCabinetGuid = new Guid(mydwFileCabinetGuid),
                    DocId = storedDocid.ToString()
                };

                Console.WriteLine("Document URL: " + dWIntegrationUrl.Url);
            }
            else
            {
                Console.WriteLine("Document not stored in DocuWare");
            }

            Console.ReadLine();

        }
    }
}
