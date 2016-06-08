# BLIP .NET SDK
A .NET SDK for interacting with the Balihoo Local Information Platform (BLIP).

## Requirements
- The BLIP .NET SDK targets version 4.5.2 of the .NET Framework.
- Using this SDK requires that you already have API keys and are interacting on behalf of a brand that already exists in Balihoo's system. Please contact Balihoo if you require API keys and/or would like to add a new brand to our system.


## Installation
[![NuGet version](https://badge.fury.io/nu/BalihooBlipDotNet.svg)](https://badge.fury.io/nu/BalihooBlipDotNet)

The SDK is available as a [NuGet package](https://www.nuget.org/packages/BalihooBlipDotNet/).
To install BalihooBlipDotNet, run the following command in the Package Manager Console:
```powershell
PM> Install-Package BalihooBlipDotNet
```

## Usage
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var brandResponse = blip.getBrandKeys();

    // Status Code (e.g. 200, 204, 404, etc.)
    var responseCode = brandResponse.StatusCode;

    // Response Content
    var myBrands = brandResponse.Body;
```

The Blip constructor has an optional "endpoint" parameter to specify the environment URL. The endpoint defaults to Balihoo's production environment but you can optionally pass the dev or stage URL.
### Example
```csharp
	// dev: https://blip.dev.balihoo-cloud.com
	// stage: https://blip.stage.balihoo-cloud.com
	// production: https://blip.balihoo-cloud.com
	var blip = new Blip("<Your API Key>", "<Your Secret Key>", "https://blip.dev.balihoo-cloud.com");
```

## Methods
All methods return a **BlipResponse** object with two properties:
- **StatusCode**
  - The HTTP response code (e.g. 200, 204, 404) as an integer.
- **Body**
  - The HTTP response content as a string.
  - For calls that return brand or location data this property will contain stringified JSON objects.

### **Ping**
Ping the BLIP API.

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var blipResponse = blip.Ping();

    if (blipResponse.StatusCode == 200)
    {
      // Success!
    }
```
---
### **GetBrandKeys**
Get a list of brandKeys that the API user is authorized to access.

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var blipResponse = blip.GetBrandKeys();

    var myBrandKeys = blipResponse.Body;
```
---
### **GetBrandSources**
Get a list of data sources available for an individual brand.

#### Parameters
- brandKey: The unique identifier for a single brand.

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var blipResponse = blip.GetBrandSources("mybrand");

    var sources = blipResponse.Body;
```
---
### **GetBrandProjections**
Get a list of data projections available for an individual brand.

#### Parameters
- brandKey: The unique identifier for a single brand.

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var blipResponse = blip.GetBrandProjections("mybrand");

    var projections = blipResponse.Body;
```
---
### **GetLocationKeys**
Get a list of locationKeys for all locations belonging to the specified brand.

#### Parameters
- brandKey: The unique identifier for a single brand.
- projection: Optionally filter data in a single projection. Defaults to "universal".

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var blipResponse = blip.GetLocationKeys("mybrand");

    var locationKeys = blipResponse.Body;
```
---
### **GetLocation**
Get data for an individual location within the specified brand.

#### Parameters
- brandKey: The unique identifier for a single brand.
- locationKey: The unique identifier for a single location within the brand.
- projection: Optionally filter data in a single projection. Defaults to "universal".
- includeRefs: Optionally include objects referenced by the location in its data. Defaults to false.

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var blipResponse = blip.GetLocation("mybrand", "mylocation");

    var locationData = blipResponse.Body;
```
---
### **QueryLocations**
Get data for locations in a single brand filtered by the specified BLIP query.

#### Parameters
- brandKey: The unique identifier for a single brand.
- query: A stringified JSON query used to filter locations.
- view: Optionally specify the view returned. Defaults to "full".
- pageSize: Optionally specify the number of results to include in each page of results.
- pageNumber: Optionally specify the page index to return.

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var query = "{\"address.state\":{\"equals\":\"ID\"}}";
    var blipResponse = blip.QueryLocations("mybrand", query);

    var matchingLocations = blipResponse.Body;
```
---
### **PutLocation**
Add a new location or update an existing location's data.

#### Parameters
- brandKey: The unique identifier for a single brand.
- locationKey: The unique identifier for a single location within the brand.
- source: The name of the data source being used to add/update the location
- locationData: The stringified JSON location document.

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var locationDocument = "{\"document\":{\"name\":\"Balihoo, Inc.\",\"address\":{\"city\":\"Boise\",\"state\":\"ID\"}}}";
    var blipResponse = blip.PutLocation("mybrand", "mylocation", "mysource", locationDocument);

    if (blipResponse.StatusCode == 204)
    {
        // location was successfully added/updated
    }
```
---
### **DeleteLocation**
Delete an individual location.

#### Parameters
- brandKey: The unique identifier for a single brand.
- locationKey: The unique identifier for a single location within the brand.
- source: The name of the data source being used to delete the location

#### Example
```csharp
    var blip = new Blip("<Your API Key>", "<Your Secret Key>");
    var blipResponse = blip.DeleteLocation("mybrand", "mylocation", "mysource");

    if (blipResponse.StatusCode == 204)
    {
        // location was successfully deleted
    }
```
---
### **BulkLoad**
Load a single file containing multiple locations.
 
#### Parameters
- brandKey: The unique identifier for a single brand.
- source: The unique identifier for the data source being used to add/update the location.
- filePath: The full path to the bulk location file. See note below on the bulk load file format.
- implicitDelete: Whether or not to delete locations from BLIP if they're missing from the file.
- expectedRecordCount: The number of location records to expect in the file.
- successEmail: An optional email address to notify upon success. Can be a comma-delimited list. Defaults to null.
- failEmail: An optional email address to notify upon failure. Can be a comma-delimited list. Defaults to null.
- successCallbackUrl: An optional URL to call upon success. Defaults to null.
- failCallbackUrl: An optional URL to call upon failure. Defaults to null.

#### Example
```csharp
    Blip blip = new Blip("<Your API Key>", "<Your Secret Key>");
    BlipResponse blipResponse = blip.BulkLoad("mybrand", "mysource", "/tmp/myfile.json", true, 50, 
                                              "success@mycompany.com", "error@mycompany.com,me@mycompany.com",
                                              "http://mycompany.com/api?success=true", "http://mycompany.com/api?error=true");

    if (blipResponse.STATUS_CODE == 204)
    {
        // File load has been successfully initiated.
        // Optional success and failure notifications will be made once the load process completes.
    }
```

#### Bulk Load File Format
The file for the bulkLoad process should contain each location's data on a single line and each line delimited by a line-feed character (\n). If a location is omitted from the file and implicitDelete is set to true, the location will be deleted.

##### Simplified Example
```
{"brandKey":"mybrand","locationKey":"ABC123","document":{...}}\n
{"brandKey":"mybrand","locationKey":"ABC124","document":{...}}\n
{"brandKey":"mybrand","locationKey":"ABC125","document":{...}}\n
```