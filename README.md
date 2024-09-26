# DocuWare-Simple
Simplistic examples utilizing DocuWare platform api

`DocuWare Platform Api`
[https://developer.docuware.com/](https://developer.docuware.com/)

## Creates a simple text file and uploads it to DocuWare
```csharp
byte[] simpleDoc = System.Text.Encoding.UTF8.GetBytes($"Hello DocuWare - From text on this fine day {DateTime.Now.ToLongDateString()}");

```

> see `SimpleDw.cs` class and method `UploadToDocuWare` for example calls to upload a simple text file to DocuWare


## Use document id from result to create a url link to view the document
```csharp

//create the integration url class to generate the url
DocuWare.WebIntegration.DWIntegrationUrl dWIntegrationUrl = 
new DocuWare.WebIntegration.DWIntegrationUrl(new DocuWare.WebIntegration.DWIntegrationInfo(mydwPlatformUrl + "/WebClient", false)
{
    Scheme = "https",
}, DocuWare.WebIntegration.IntegrationType.ResultlistAndViewer);


//create the parameters for the url we are keeping it simple here and using the file cabinet guid and the doc id
dWIntegrationUrl.Parameters = new DocuWare.WebIntegration.DWIntegrationUrlParameters(DocuWare.WebIntegration.IntegrationType.ResultlistAndViewer)
{
    FileCabinetGuid = new Guid(mydwFileCabinetGuid),
    DocId = storedDocid.ToString()
};

//output the url to the console
Console.WriteLine(dWIntegrationUrl.Url);

```

## Get organization and user info for current connection
```csharp
                Organization organization = serviceConnection.Organizations.FirstOrDefault();
                if (organization != null)
                {
                    Console.WriteLine($"Current organization: {organization.Name}");
                    UserInfo userInfo = await organization.GetUserInfoFromUserInfoRelationAsync();
                    if (userInfo != null)
                    {
                        if (userInfo.User != null)
                        {
                            Console.WriteLine($"Authenticated user: {userInfo.User.Name}");
                            Console.WriteLine($"Last Name: {userInfo.User.LastName}");
                            Console.WriteLine($"First Name: {userInfo.User.FirstName}");
                            Console.WriteLine($"DB Name: {userInfo.User.DBName}");
                            Console.WriteLine($"Email: {userInfo.User.EMail}");
                            Console.WriteLine($"Id: {userInfo.User.Id}");
                            Console.WriteLine($"Network Id: {userInfo.User.NetworkId}");
                        }
                    }
                }
```

