# PowerToys Tools built on Azure Functions

Some tools built on Azure Functions, include:
- Remote file downloader

## Built from source

1. Install Visual Studio 2022 with Azure development workload

2. Get source code
``` bash
git clone https://github.com/heavenwing/PowerToysOnAzureFunctions
```

3. Create "local.settings.json" file in root directory with below content:
``` json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FileStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"
    }
}
```

4. Open solution with VS2022, run it locally or publish it into your Azure subscription

## Tools List

### Remote file downloader

Use function download file from url and save into Azure Blob Storage, so that you can download file from Azure Storage.
It is useful for downloading file which you cannot download or download too slowly via your network.

**Usage:**

Open your function url in web browser, ex: http://localhost:7071/api/Downloader?url=[replace_with_your_file_url]