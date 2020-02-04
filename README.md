# ArcGIS2GPX
Azure serverless compute functions to convert ArcGIS feature layers to GPX files


I recently did a search for tools that will convert feature layers to GPX format. Not finding anything, I ended up writing one. I deployed it to an Azure portal as a severless compute function.

 

You can see the results in this storymap:

The 2020 BMW RA Rally (https://storymaps.arcgis.com/stories/75ef83f01c424d95bf75a7e28d4d1608)

The routes in this storymap were created using the Directions capability in the standard ArcGIS Online web map viewer. The resulting route feature collections were published as hosted feature layers and shared publicly. The URLs to those feature layers are configured as parameters in the links in each of the 'Download GPX file' buttons in the storymap.

 

If anyone is interested you can leverage a similar capability in your own storymaps or other applications using this function:

https://arcgis2gpx.azurewebsites.net/api/Layers2GPX

 

It takes GET or POST requests and accepts point or polyline feature layers. The features in point feature layers become GPX waypoints while the features in polyline feature layers become GPX tracks.

 

The minimum parameters required for GET requests is the url to a point or polyline feature layer. Thus you can create a URL for a hyperlink in a web page, storymap or other document as follows: 

https://arcgis2gpx.azurewebsites.net/api/Layers2GPX?url=https://services1.arcgis.com/EvDRLcHhbHG5BnwT/arcgis/rest/services/Hiking_Routes/FeatureServer/0&name=Name&desc=Description

or

https://arcgis2gpx.azurewebsites.net/api/Layers2GPX?url=https://services1.arcgis.com/fBc8EJBxQRMcHlei/arcgis/rest/services/Appalachian_National_Scenic_Trail/FeatureServer/0 

 

The minimum payload for a POST request is a json object with a title element (which will become the output GPX filename) and an array of feature layer urls. Thus you can combine multiple point and/or polyline feature layers into a single GPX file. Example:

{
 "title":"Some Trails",
 "returnFile":"true",
 "layers":[
 {
 "url":"https://services1.arcgis.com/EvDRLcHhbHG5BnwT/arcgis/rest/services/Hiking_Routes/FeatureServer/0",
 "name":"Name",
 "desc":"Description",
 "where":"AscentFT>3600"
 },
 {
 "url":"https://services1.arcgis.com/fBc8EJBxQRMcHlei/arcgis/rest/services/Appalachian_National_Scenic_Trail/FeatureServer/0",
 "name":"Name",
 "desc":"Description",
 "where":"Length_Ft>40000"
 }
 ]
}

Additionally, both methods allow you to include optional parameters for each url to indicate wich fields from the source feature layers to utilize for the different GPX elements such as
name, desc, cmt, etc
Click the function URL above for a complete list of optional parameters.

 

I cannot guarantee how long the function will remain up. If you want the code let me know.

 

If you find an error in functinality or behavior, please also let me know.


#  Usage:

https://arcgis2gpx.azurewebsites.net/api/Layers2GPX converts ArcGIS features to GPX elements and returns a file of the data for use in GPS devices. THe GPX data follows the spec detailed at http://www.topografix.com/GPX/1/1/.

Function url: https://arcgis2gpx.azurewebsites.net/api/Layers2GPX

GET requests will accept one value for each of the parameters.
POST requests will accept an array of JSON objects each containing one value for each of the parameters. This allows you to combine multiple point and/or polyline feature layers into a single output GPX file.

## Parameters:

**url:** required. The url to a portal feature layer. This should end in 'FeatureService/n' where n is the layer index.

**returnFile:**  optional. If true, returns a GPX file. If false, returns a string. Defaults to true.

**where:** optional. The where clause to limit the features included in the GPX data. Defaults '1=1'.

**name:** optional. The name of the field in the feature layer that should provide the name values for each track or waypoint element.

**cmt:** optional. The name of the field in the feature layer that should provide the cmt values for each track or waypoint element.

**desc:** optional. The name of the field in the feature layer that should provide the desc values for each track or waypoint element.

**src:** optional. The name of the field in the feature layer that should provide the src values for each track or waypoint element.

**type:** optional. The name of the field in the feature layer that should provide the type values for each track or waypoint element.

**sym:** optional. The name of the field in the feature layer that should provide the sym values for each waypoint element. Ignored for polyline layers.

**ele:** optional. The name of the field in the feature layer that should provide the ele values for each waypoint element. If not present and if the point geometries have z values, the function will attempt to use feature geometry z values. If this parameter is included in the request the values from this field will supercede geometry z values. Ignored for polyline layers.

**time:** optional. The name of the field in the feature layer that should provide the time values for each waypoint element. Ignored for polyline layers.

**magvar:** optional. The name of the field in the feature layer that should provide the magvar values for each waypoint element. Ignored for polyline layers.

**geoidheight:** optional. The name of the field in the feature layer that should provide the geoidheight values for each waypoint element. Ignored for polyline layers.

**fix:** optional. The name of the field in the feature layer that should provide the fix values for each wptType element. Ignored for polyline layers.

**sat:** optional. The name of the field in the feature layer that should provide the sat values for each wptType element. Ignored for polyline layers.

**hdop:** optional. The name of the field in the feature layer that should provide the hdop values for each wptType element. Ignored for polyline layers.

**vdop:** optional. The name of the field in the feature layer that should provide the vdop values for each wptType element. Ignored for polyline layers.

**pdop:** optional. The name of the field in the feature layer that should provide the pdop values for each wptType element. Ignored for polyline layers.

**ageofgpsdata:** optional. The name of the field in the feature layer that should provide the ageofgpsdata values for each wptType element. Ignored for polyline layers.

**dgpsid:** optional. The name of the field in the feature layer that should provide the dgpsid values for each wptType element. Ignored for polyline layers.

**number:** optional. The name of the field in the feature layer that should provide the number values for each trkType element. Ignored for point layers.

example GET request: https://arcgis2gpx.azurewebsites.net/api/Layers2GPX?url=https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>&returnFile=true&name=featureNameField

Additionally, POST requests include the following:
**title:** required. The name of the GPX file to be returned.
example POST request payload:
{
  "title": "Some Trails",
  "returnFile": "true",
  "layers": [
    {
      "url": "https://services1.arcgis.com/1YRV70GwTj9GYxWK/arcgis/rest/services/Waterfalls/FeatureServer/1",
      "name": "TITLE"
    },
    {
      "url": "https://services1.arcgis.com/EvDRLcHhbHG5BnwT/arcgis/rest/services/Hiking_Routes/FeatureServer/0",
      "name": "Name",
      "desc": "Description",
      "where": "AscentFT>3600"
    },
    {
      "url": "https://services1.arcgis.com/1YRV70GwTj9GYxWK/arcgis/rest/services/BMW_RA_Rally_Fuel_and_Food/FeatureServer/0",
      "name": "name",
      "desc": "Address"
    }
  ]
}
