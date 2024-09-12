using DocuWare.Platform.ServerClient;

namespace ConsoleAppDwSimple
{

    //  add nuget package DocuWare.Platform.ServerClient.Extensions



    internal class SimpleDw
    {

        private SimpleDw(params string[] parameters)
        {
            _url = (parameters.Length > 0) ? parameters[0] : "http://localhost:9000/DocuWare/Platform";
            _username = (parameters.Length > 1) ? parameters[1] : "admin";
            _password = (parameters.Length > 2) ? parameters[2] : "admin";
            _filecabinetid = (parameters.Length > 3) ? parameters[3] : "00000000-0000-0000-0000-000000000000";
        }

        public static SimpleDw WithParameters(params string[] parameters)
        {
            return new SimpleDw(parameters);
        }

        private string _url;
        private string _username;
        private string _password;
        private string _filecabinetid;

        //non-production example of uploading a document to DocuWare
        public async Task<int> UploadToDocuWare(Action<string> logger, byte[] filecontents, string contentType, Func<List<DocumentIndexField>> getindexFields)
        {
            if (logger == null) logger = (details) => Console.WriteLine(details);
            int storedDocid = -1;

            try
            {
                //create a service connection - this is authenticating to DocuWare and returning a service connection if succeeds
                //in production you will want to track the token and reuse it if possible to avoid license consumption issues
                ServiceConnection serviceConnection = await ServiceConnection.CreateAsync(new Uri(_url), _username, _password, licenseType: DWProductTypes.PlatformService);

                string platform_token = await serviceConnection.Organizations[0].PostToLoginTokenRelationForStringAsync(new TokenDescription() { 
                    Lifetime = TimeSpan.FromMinutes(20).ToString(), 
                    TargetProducts = new List<DWProductTypes>() {  DWProductTypes.PlatformService }, 
                    Usage = TokenUsage.Multi });

                Console.WriteLine("Token: " + platform_token);

                //create a filecabinet reference
                FileCabinet fileCabinet = serviceConnection.GetFileCabinet(_filecabinetid);

                //create a document
                Document newDocument;

                //upload the document and get a dwdocument result
                using (MemoryStream documentStream = new MemoryStream(filecontents))
                {
                    newDocument = await fileCabinet.PostToAdvancedDocumentUploadRelationForDocumentAsync(contentType, documentStream);
                }

                //get the document from self relation to edit the index fields
                newDocument = await newDocument.GetDocumentFromSelfRelationAsync();

                //for an example of adding some metadata to the document
                //the field name should be the DB name of the field.
                await newDocument.PutToFieldsRelationForDocumentIndexFieldsAsync(new DocumentIndexFields() { Field = getindexFields() });

                storedDocid = newDocument.Id;
            }
            catch (Exception ex)
            {
                logger(ex.Message);
                throw;
            }

            return storedDocid;
        }

    }
}
