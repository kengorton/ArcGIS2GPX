#r "Newtonsoft.Json"

//using System;
//using System.IO;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Net;
using Microsoft.Extensions.Primitives;
//using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml.Linq;
using Esri.ArcGISRuntime.Geometry;



public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");
    string serviceUrl = "";
    string content = "";
    string serviceInfoUrl = "";
    string serviceQueryUrl = "";
    string featureQueryUrl = "";
    string where = "";
    string name = "";
    string desc = "";
    string cmt = "";
    string sym = "";
    string ele = "";
    string time = "";
    string magvar = "";
    string height = "";
    string src = "";
    string type = "";
    string fix = "";
    string sat = "";
    string hdop = "";
    string vdop = "";
    string pdop = "";
    string age = "";
    string dgpsid = "";
    string number = "";
    string returnFile = "";
    bool sendFile = true;
    string filename = "";
    
    string method = req.Method;
    HttpClient client = new HttpClient();
    
    
    

    XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
    XNamespace gpx1 = "http://www.topografix.com/GPX/1/1/";
    XNamespace gpx2 = "http://www.topografix.com/GPX/1/1/gpx.xsd";
    XElement g = new XElement(gpx1+"gpx", 
        new XAttribute("creator","http://www.cagegis.com/"),
        new XAttribute("version","1.1"),
        new XAttribute(XNamespace.Xmlns+"xsi",xsi.NamespaceName),
        new XAttribute(xsi+"schemaLocation",gpx1.NamespaceName + " " + gpx2.NamespaceName));

    XDocument d = new XDocument(new XDeclaration("1.0", "utf-8", "true"),g);
    

    string errorHelp = "\n\n\n\nUsage:\n\n";
    string reqUrl = string.Format("{0}://{1}{2}",req.Scheme,req.Host,req.Path);
    errorHelp += string.Format("{0} converts ArcGIS features to GPX elements and returns a file of the data for use in GPS devices. The GPX data follows the spec detailed at {1}.{2}",reqUrl,gpx1,"\n\n");
    errorHelp += string.Format("Function url: {0}{1}",reqUrl,"\n\n");
    errorHelp += "GET requests will accept one value for each of the parameters except the 'content' parameter which will accept a comma-separated list of route layer itemIds or the url to a single feature layer.\n";
    errorHelp += "POST requests will accept an array of JSON objects each containing a value for each included parameter. This allows you to combine multiple point and/or polyline feature layers into a single output GPX file.\n\n";
    errorHelp += "Parameters:\n";
    errorHelp += "content: required. In a GET request can be a comma-separated list of itemIds for one or more ArcGIS Online Route Layers or the url to a portal feature layer of point or polyline geometry. in a POST request, this may only be the url to a feature layer. This string should begin with 'https://' and end in 'FeatureService/n' where n is the layer index.\n\n";
    //errorHelp += "url: required if no routes. The url to a portal feature layer. This should end in 'FeatureService/n' where n is the layer index. Ignored if converting Route Layers.\n";
    errorHelp += "title: The name of the GPX file to be returned. Required if processing multiple routes or feature layers.\n";
    errorHelp += "returnFile: optional. If true, returns a GPX file. If false, returns a string. Defaults to true.\n";
    errorHelp += "where: optional. The where clause to limit the features included in the GPX data. Defaults '1=1'. Ignored if converting Route Layers.\n";
    errorHelp += "name: optional. The name of the field in the feature layer that should provide the name values for each track or waypoint element. Ignored if converting Route Layers.\n";
    errorHelp += "cmt: optional. The name of the field in the feature layer that should provide the cmt values for each track or waypoint element. Ignored if converting Route Layers.\n";
    errorHelp += "desc: optional. The name of the field in the feature layer that should provide the desc values for each track or waypoint element. Ignored if converting Route Layers.\n";
    errorHelp += "src: optional. The name of the field in the feature layer that should provide the src values for each track or waypoint element. Ignored if converting Route Layers.\n";
    errorHelp += "type: optional. The name of the field in the feature layer that should provide the type values for each track or waypoint element. Ignored if converting Route Layers.\n";
    errorHelp += "sym: optional. The name of the field in the feature layer that should provide the sym values for each waypoint element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "ele: optional. The name of the field in the feature layer that should provide the ele values for each waypoint element. If not present and if the point geometries have z values, the function will attempt to use feature geometry z values. If this parameter is included in the request the values from this field will supercede geometry z values. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "time: optional. The name of the field in the feature layer that should provide the time values for each waypoint element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "magvar: optional. The name of the field in the feature layer that should provide the magvar values for each waypoint element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "geoidheight: optional. The name of the field in the feature layer that should provide the geoidheight values for each waypoint element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "fix: optional. The name of the field in the feature layer that should provide the fix values for each wptType element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "sat: optional. The name of the field in the feature layer that should provide the sat values for each wptType element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "hdop: optional. The name of the field in the feature layer that should provide the hdop values for each wptType element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "vdop: optional. The name of the field in the feature layer that should provide the vdop values for each wptType element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "pdop: optional. The name of the field in the feature layer that should provide the pdop values for each wptType element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "ageofgpsdata: optional. The name of the field in the feature layer that should provide the ageofgpsdata values for each wptType element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "dgpsid: optional. The name of the field in the feature layer that should provide the dgpsid values for each wptType element. Ignored for polyline layers. Ignored if converting Route Layers.\n";
    errorHelp += "number: optional. The name of the field in the feature layer that should provide the number values for each trkType element. Ignored for point layers. Ignored if converting Route Layers.\n";
    errorHelp += string.Format("\n\nExample GET request:\n");
    errorHelp += string.Format("{0}{1}{2}\n\n",reqUrl,"?","content=b8ca350685f046f6b70f2fecc6b23f9c,e8e4304938d74919b71ed0a64e6ddf2b&title=NewRouteLayerGPXFile");
    errorHelp += string.Format("{0}{1}{2}\n\n",reqUrl,"?","content=https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>&returnFile=false&name=featureNameField&title=NewFeatureLayerGPXFile");
    errorHelp += string.Format("\n\nExample POST request payload:\n");
    //string payload = "{\"title\":\"A new GPX file\",\"returnFile\":false,\"layers\":[{\"url\":\"https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>\",\"name\":\"myNameField1\",\"desc\":\"myDescField1\",\"cmt\":\"myCmtField1\",\"sym\":\"mySymField1\"},{\"url\":\"https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>\",\"name\":\"myNameField2\",\"desc\":\"myDescField2\"},{\"url\":\"https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>\",\"name\":\"myNameField3\",\"desc\":\"myDescField3\",\"cmt\":\"myCmtField3\"}]}";
    string payload2 = "{\"title\":\"Some Trails\",\"returnFile\":\"true\",\"layers\":[{\"content\":\"https://services1.arcgis.com/1YRV70GwTj9GYxWK/arcgis/rest/services/Waterfalls/FeatureServer/1\",\"name\":\"TITLE\"},{\"url\":\"https://services1.arcgis.com/EvDRLcHhbHG5BnwT/arcgis/rest/services/Hiking_Routes/FeatureServer/0\",\"name\":\"Name\",\"desc\":\"Description\",\"where\":\"AscentFT>3600\"},{\"url\":\"https://services1.arcgis.com/1YRV70GwTj9GYxWK/arcgis/rest/services/BMW_RA_Rally_Fuel_and_Food/FeatureServer/0\",\"name\":\"name\",\"desc\":\"Address\"}]}";
    string jsonFormatted = JValue.Parse(payload2).ToString(Formatting.Indented);
    errorHelp += jsonFormatted;

    HttpResponseMessage result;
    string resultContent = "";
    JObject details;
    string title = "";

    if (method == "GET"){



        content = req.Query["content"];
        if (string.IsNullOrEmpty(content)){
            return new BadRequestObjectResult("Missing required parameter 'content'. This can be a comma-separated list of itemIds to ArcGIS route layers or the url to a single point or polyline feature layer."+errorHelp);
        }

        string[] itemIds = content.Split(",");   
           
        if (itemIds.Length > 1){
            List<string> items = new List<string>();            
            foreach (string itemId in itemIds){
                if (itemId.Length != 32){
                    continue;
                }
                if (!items.Contains(itemId)){
                    items.Add(itemId);
                }
            }
            itemIds = items.ToArray();          
        }
        else {
            string[] splitContent = content.Split("/");
            
            if(splitContent.Length > 1 && (splitContent[0] == "http:"||splitContent[0] == "https:") & Int32.TryParse(splitContent[splitContent.Length - 1],out int n)){
                serviceUrl = content;
                itemIds = new string[0]; 
            }
        }
        filename = req.Query["title"];
        
        if (string.IsNullOrEmpty(filename) && itemIds.Length > 1){
            return new BadRequestObjectResult("Missing required parameter 'Title'. When processing multiple routes or layers this value will be used to name the GPX file to be downloaded."+errorHelp);
        }        
        
        returnFile = req.Query["returnFile"];
        sendFile = string.IsNullOrEmpty(returnFile) ? true : Convert.ToBoolean(returnFile);
 
        
        //is it a Route Layer?

        if (itemIds.Length > 0){
            //process route layers
            //log.LogInformation("Routes");

        
            for (int i=0;i<2;i++){
                foreach (string itemId in itemIds){
                    string agolSharingUrl = "https://www.arcgis.com/sharing/rest/content/items/";
                    string routeLayerInfoUrl = agolSharingUrl + itemId + "?f=json";
                    string routeLayerDataUrl = agolSharingUrl + itemId + "/data?f=json";
                    try{
                        result = await client.GetAsync(routeLayerInfoUrl);
                        resultContent = await result.Content.ReadAsStringAsync();
                        details = JObject.Parse(resultContent);
                        if (details["error"] != null){
                            continue;
                        }
                    }
                    catch (WebException e)
                    {
                        continue;
                    }
                    
                    
                    
                    title =  (string)details["title"]; 
                    if (string.IsNullOrEmpty(filename)){
                        filename = title;
                    }  
                    g.Add(new XElement("metadata",  
                        new XElement("name", filename)  
                    ));                 
                    JArray typeKeywords =  (JArray)details["typeKeywords"];
                    log.LogInformation(typeKeywords.ToString());

                    if (!typeKeywords.ToString().Contains("Route Layer")){
                        continue;
                        //return new BadRequestObjectResult(String.Format("{0} is not an ArcGIS route layer.{1}",title,errorHelp));
                    }
                    String uri = serviceQueryUrl;
                        
                    result = await client.GetAsync(routeLayerDataUrl);
                    resultContent = await result.Content.ReadAsStringAsync();
                    details = JObject.Parse(resultContent);
                    JArray layers =  (JArray)details["layers"];


                    foreach (JObject layer in layers){
                        if ((string)layer["layerDefinition"]["name"] == "DirectionPoints" && i==0){
                            
                            //TODO process Stops data
                            JArray points = (JArray)layer["featureSet"]["features"];
                            int count = 1;
                            
                            foreach (JObject f in points.Children<JObject>()){
                                
                                
                                string wptName = string.IsNullOrEmpty(name)?(string)f["attributes"]["Name"]:(string)f["attributes"][name];
                                
                                

                                if(string.IsNullOrEmpty(wptName)||string.IsNullOrWhiteSpace(wptName)||wptName=="null"){
                                    wptName = String.Format("{0} Wpt {1}",title,count);
                                }

                                //project the point to WGS84 4326                                
                                double x = (double)f["geometry"]["x"];
                                double y = (double)f["geometry"]["y"];

                                double num3 = x / 6378137.0;
                                double num4 = num3 * 57.295779513082323;
                                double num5 = Math.Floor((double)((num4 + 180.0) / 360.0));
                                double num6 = num4 - (num5 * 360.0);
                                double num7 = 1.5707963267948966 - (2.0 * Math.Atan(Math.Exp((-1.0 * y) / 6378137.0)));
                                x = num6;
                                y = num7 * 57.295779513082323;


                                XElement w = new XElement("wpt",
                                            new XAttribute("lat",y),
                                            new XAttribute("lon",x),
                                    new XElement("name",wptName),
                                    new XElement("desc",(string)f["attributes"]["DisplayText"])
                                                                        
                                );
                                if(itemIds.Length>1){
                                    w.Add(new XElement("cmt",String.Format("From route: {0}",title)));
                                }
                                g.Add(w); 
                                count++;
                            }
                        }
                        
                        if ((string)layer["layerDefinition"]["name"] == "DirectionLines" && i==1){
                            
                            //TODO process DirectionLines data
                            JArray lines = (JArray)layer["featureSet"]["features"];
                            //log.LogInformation("Made it this far");
                            if (lines.Count > 0){
                                XElement t = new XElement("trk",new XElement("name",title));
                                g.Add(t);
                                foreach (JObject f in lines.Children<JObject>()){                                   
                                    
                                    JArray routeGeom = (JArray)f["geometry"]["paths"];                
                                    foreach (JArray path in routeGeom.Children<JArray>()){
                                        XElement s = new XElement("trkseg");
                                        
                                        foreach (JArray point in path.Children<JArray>()){

                                            //TODO project the point to WGS84 4326
                                                                           
                                            double x = (double)point[0];
                                            double y = (double)point[1];

                                            double num3 = x / 6378137.0;
                                            double num4 = num3 * 57.295779513082323;
                                            double num5 = Math.Floor((double)((num4 + 180.0) / 360.0));
                                            double num6 = num4 - (num5 * 360.0);
                                            double num7 = 1.5707963267948966 - (2.0 * Math.Atan(Math.Exp((-1.0 * y) / 6378137.0)));
                                            x = num6;
                                            y = num7 * 57.295779513082323;

                                            s.Add(new XElement("trkpt",
                                                new XAttribute("lat",y),
                                                new XAttribute("lon",x)
                                            ));
                                        }
                                        t.Add(s);
                                    }                              
                                }
                            }
                        }
                    }
                }
            }         
        }

        //otherwise process it as afeature layer

        else {
            //log.LogInformation("Feature layer");   
        
            if (string.IsNullOrEmpty(serviceUrl)){
                return new BadRequestObjectResult(String.Format("No ArcGIS Online route layer url.\n\n{0}",errorHelp));
            }

            if (!serviceUrl.Contains("FeatureServer")){
                return new BadRequestObjectResult(String.Format("The url {0} is not to an ArcGIS feature service.\n\n{1}",serviceUrl,errorHelp));
            }

            serviceInfoUrl = serviceUrl+"/info/itemInfo?f=pjson";
            serviceQueryUrl = serviceUrl+"?f=pjson";
            where = req.Query["where"];
            where = where ?? "1=1";
            featureQueryUrl = serviceUrl+"/query?where="+where+"&outSR=4326&outFields=*&f=pjson";
            name = req.Query["name"];
            desc = req.Query["desc"];
            cmt = req.Query["cmt"];
            sym = req.Query["sym"];
            ele = req.Query["ele"];
            time = req.Query["time"];
            magvar = req.Query["magvar"];
            height = req.Query["geoidheight"];
            src = req.Query["src"];
            type = req.Query["type"];
            fix = req.Query["fix"];
            sat = req.Query["sat"];
            hdop = req.Query["hdop"];
            vdop = req.Query["vdop"];
            pdop = req.Query["pdop"];
            age = req.Query["ageofgpsdata"];
            dgpsid = req.Query["dgpsid"];
            number = req.Query["number"];

            result = await client.GetAsync(serviceQueryUrl);
            resultContent = await result.Content.ReadAsStringAsync();
            details = JObject.Parse(resultContent);
            title =  (string)details["name"];            
            if (string.IsNullOrEmpty(filename)){
                filename = title;
            }      
              
            g.Add(new XElement("metadata",  
                new XElement("name", filename)  
            ));             

            string capabilities = (string)details["capabilities"];
            if (!capabilities.Contains("Query")){
                return new BadRequestObjectResult(String.Format("{0} does not allow Querying.",title));
            }

            string geometryType = (string)details["geometryType"];
            if(geometryType != "esriGeometryPoint" && geometryType != "esriGeometryPolyline"){
                return new BadRequestObjectResult(String.Format("Layer {0} is neither a point nor a line layer.",title));
            }

        

            // get the features
            result = await client.GetAsync(featureQueryUrl);
            resultContent = await result.Content.ReadAsStringAsync();
            details = JObject.Parse(resultContent);
            JArray features =  (JArray)details["features"];    



                                

            if(geometryType == "esriGeometryPoint"){
                int count = 1;
                foreach (JObject f in features.Children<JObject>()){
                    string wptName = "";
                    if (name != null){
                        wptName = (string)f["attributes"][name];
                    }
                    
                    if(string.IsNullOrEmpty(wptName)||string.IsNullOrWhiteSpace(wptName)||wptName=="null"){
                        wptName = String.Format("{0} Wpt {1}",title,count);
                    }
                    

                    XElement w = new XElement("wpt",
                                new XAttribute("lat",(double)f["geometry"]["y"]),
                                new XAttribute("lon",(double)f["geometry"]["x"]),
                        new XElement("name",wptName)                                   
                    );
                    
                    if (desc != null){w.Add(new XElement("desc",(string)f["attributes"][desc]));} 
                    if (cmt != null){w.Add(new XElement("cmt",(string)f["attributes"][cmt]));}
                    if (sym != null){w.Add(new XElement("sym",(string)f["attributes"][sym]));}
                    if (ele != null){
                        w.Add(new XElement("ele",(string)f["attributes"][ele]));
                    }
                    else if(f["geometry"]["z"] != null){
                        w.Add(new XElement("ele",(double)f["geometry"]["z"]));
                    }
                    if (time != null){w.Add(new XElement("time",(string)f["attributes"][time]));}
                    if (magvar != null){w.Add(new XElement("magvar",(string)f["attributes"][magvar]));}
                    if (height != null){w.Add(new XElement("geoidheight",(string)f["attributes"][height]));}
                    if (src != null){w.Add(new XElement("src",(string)f["attributes"][src]));}
                    if (type != null){w.Add(new XElement("type",(string)f["attributes"][type]));}
                    if (fix != null){w.Add(new XElement("fix",(string)f["attributes"][fix]));}
                    if (sat != null){w.Add(new XElement("sat",(string)f["attributes"][sat]));}
                    if (hdop != null){w.Add(new XElement("hdop",(string)f["attributes"][hdop]));}
                    if (vdop != null){w.Add(new XElement("vdop",(string)f["attributes"][vdop]));}
                    if (pdop != null){w.Add(new XElement("pdop",(string)f["attributes"][pdop]));}
                    if (age != null){w.Add(new XElement("age",(string)f["attributes"][age]));}
                    if (dgpsid != null){w.Add(new XElement("dgpsid",(string)f["attributes"][dgpsid]));}

                    g.Add(w);   
                    count++;                                                   
                }
            }

                    
            if(geometryType == "esriGeometryPolyline"){
                foreach (JObject f in features.Children<JObject>()){
                    
                    XElement t = new XElement("trk");
                    
                    if (name != null){t.Add(new XElement("name",(string)f["attributes"][name]));}
                    if (desc != null){t.Add(new XElement("desc",(string)f["attributes"][desc]));} 
                    if (cmt != null){t.Add(new XElement("cmt",(string)f["attributes"][cmt]));}
                    if (src != null){t.Add(new XElement("src",(string)f["attributes"][src]));}
                    if (type != null){t.Add(new XElement("type",(string)f["attributes"][type]));}
                    if (number != null){t.Add(new XElement("number",(string)f["attributes"][number]));}

                    g.Add(t);

                    JArray routeGeom = (JArray)f["geometry"]["paths"];                
                    foreach (JArray path in routeGeom.Children<JArray>()){
                        XElement s = new XElement("trkseg");
                        foreach (JArray point in path.Children<JArray>()){
                            s.Add(new XElement("trkpt",
                                new XAttribute("lat",point[1].ToString()),
                                new XAttribute("lon",point[0].ToString())
                            ));
                        }
                        t.Add(s);
                    }                              
                }
            }
        }
    } 
    else //method is POST
    {   
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);  

        JArray layers = (JArray)data["layers"];              
        title = (string)data["title"]; 
        bool hasMetadata = false;
        if (!string.IsNullOrEmpty(title)){
            filename = title;
            g.Add(new XElement("metadata",  
                new XElement("name", filename)  
            ));
            hasMetadata = true;
            //log.LogInformation(g.ToString());
        }
        if(layers.Count > 1 && string.IsNullOrEmpty(title)){                    
            return new BadRequestObjectResult(String.Format("Missing required parameter 'Title'. When processing multiple routes or layers this value will be used to name the GPX file to be downloaded.\n\n{0}",errorHelp));
        }
        returnFile = (string)data["returnFile"];
        sendFile = returnFile == null ? true : Convert.ToBoolean(returnFile);



                          
          

        //loop through the feature layer and convert them to GPX. 
        //Do all the point layers first, then loop through againanad do the polyline layers.
        for (int i=0;i<2;i++){ 
            foreach (JObject layer in layers.Children<JObject>()) {
                serviceUrl = (string)layer["url"];
                serviceInfoUrl = serviceUrl.Substring(0,serviceUrl.LastIndexOf("/"))+"/info/itemInfo?f=pjson";
                //log.LogInformation(serviceInfoUrl);
                serviceQueryUrl = serviceUrl+"?f=pjson";
                where = (string)layer["where"];
                where = where ?? "1=1";
                featureQueryUrl = serviceUrl+"/query?where="+where+"&outSR=4326&outFields=*&f=pjson";
                name = (string)layer["name"];
                desc = (string)layer["desc"];
                cmt = (string)layer["cmt"];
                sym = (string)layer["sym"];
                ele = (string)layer["ele"];
                time = (string)layer["time"];
                magvar = (string)layer["magvar"];
                height = (string)layer["geoidheight"];
                src = (string)layer["src"];
                type = (string)layer["type"];
                fix = (string)layer["fix"];
                sat = (string)layer["sat"];
                hdop = (string)layer["hdop"];
                vdop = (string)layer["vdop"];
                pdop = (string)layer["pdop"];
                age = (string)layer["ageofgpsdata"];
                dgpsid = (string)layer["dgpsid"];
                number = (string)layer["number"];

                result = await client.GetAsync(serviceInfoUrl);
                resultContent = await result.Content.ReadAsStringAsync();
                details = JObject.Parse(resultContent);
                
                title =  (string)details["title"];            
                if (string.IsNullOrEmpty(filename)){
                    filename = title;                    
                    if (!hasMetadata){
                        g.Add(new XElement("metadata",  
                            new XElement("name", filename)  
                        ));
                        hasMetadata = true;
                        //log.LogInformation(g.ToString());
                    }
                }                 
                JArray typeKeywords =  (JArray)details["typeKeywords"];
                //if (!typeKeywords.Contains("Route Layer")){
                //    return new BadRequestObjectResult(String.Format("{0} is not an ArcGIS route layer.{1}",title,errorHelp));
                //} 
                
        
                result = await client.GetAsync(serviceQueryUrl);
                resultContent = await result.Content.ReadAsStringAsync();
                details = JObject.Parse(resultContent);

                string geometryType = (string)details["geometryType"];
                if(geometryType != "esriGeometryPoint" && geometryType != "esriGeometryPolyline"){
                    continue;
                } 
                if(i==0 && geometryType != "esriGeometryPoint"){
                    continue;
                }
                if(i==1 && geometryType != "esriGeometryPolyline"){
                    continue;
                }    
                
                string capabilities = (string)details["capabilities"];
                if (!capabilities.Contains("Query")){
                    continue;
                }

                // get the features
                result = await client.GetAsync(featureQueryUrl);
                resultContent = await result.Content.ReadAsStringAsync();
                details = JObject.Parse(resultContent);
                JArray features =  (JArray)details["features"];                      

                if(geometryType == "esriGeometryPoint"){
                    int count = 1;
                    foreach (JObject f in features.Children<JObject>()){
                        
                        string wptName = "";
                        if (name != null){
                            wptName = (string)f["attributes"][name];
                        }
                        
                        if(string.IsNullOrEmpty(wptName)||string.IsNullOrWhiteSpace(wptName)||wptName=="null"){
                            wptName = String.Format("{0} Wpt {1}",title,count);
                        }
                        
                        XElement w = new XElement("wpt",
                                    new XAttribute("lat",(double)f["geometry"]["y"]),
                                    new XAttribute("lon",(double)f["geometry"]["x"]),
                            new XElement("name",wptName)                                   
                        );
                        if (desc != null){w.Add(new XElement("desc",(string)f["attributes"][desc]));} 
                        if (cmt != null){w.Add(new XElement("cmt",(string)f["attributes"][cmt]));}
                        if (sym != null){w.Add(new XElement("sym",(string)f["attributes"][sym]));}
                        if (ele != null){
                            w.Add(new XElement("ele",(string)f["attributes"][ele]));
                        }
                        else if(f["geometry"]["z"] != null){
                            w.Add(new XElement("ele",(double)f["geometry"]["z"]));
                        }
                        if (time != null){w.Add(new XElement("time",(string)f["attributes"][time]));}
                        if (magvar != null){w.Add(new XElement("magvar",(string)f["attributes"][magvar]));}
                        if (height != null){w.Add(new XElement("geoidheight",(string)f["attributes"][height]));}
                        if (src != null){w.Add(new XElement("src",(string)f["attributes"][src]));}
                        if (type != null){w.Add(new XElement("type",(string)f["attributes"][type]));}
                        if (fix != null){w.Add(new XElement("fix",(string)f["attributes"][fix]));}
                        if (sat != null){w.Add(new XElement("sat",(string)f["attributes"][sat]));}
                        if (hdop != null){w.Add(new XElement("hdop",(string)f["attributes"][hdop]));}
                        if (vdop != null){w.Add(new XElement("vdop",(string)f["attributes"][vdop]));}
                        if (pdop != null){w.Add(new XElement("pdop",(string)f["attributes"][pdop]));}
                        if (age != null){w.Add(new XElement("age",(string)f["attributes"][age]));}
                        if (dgpsid != null){w.Add(new XElement("dgpsid",(string)f["attributes"][dgpsid]));}
                        g.Add(w);   
                        count++;                                                   
                    }
                }
                        
                if(geometryType == "esriGeometryPolyline"){
                    foreach (JObject f in features.Children<JObject>()){
                        
                        XElement t = new XElement("trk");
                    
                        if (name != null){t.Add(new XElement("name",(string)f["attributes"][name]));}
                        if (desc != null){t.Add(new XElement("desc",(string)f["attributes"][desc]));} 
                        if (cmt != null){t.Add(new XElement("cmt",(string)f["attributes"][cmt]));}
                        if (src != null){t.Add(new XElement("src",(string)f["attributes"][src]));}
                        if (type != null){t.Add(new XElement("type",(string)f["attributes"][type]));}
                        if (number != null){t.Add(new XElement("number",(string)f["attributes"][number]));}

                        g.Add(t);
                        JArray routeGeom = (JArray)f["geometry"]["paths"];                
                        foreach (JArray path in routeGeom.Children<JArray>()){
                            XElement s = new XElement("trkseg");
                            foreach (JArray point in path.Children<JArray>()){
                                s.Add(new XElement("trkpt",
                                    new XAttribute("lat",point[1].ToString()),
                                    new XAttribute("lon",point[0].ToString())
                                ));
                            }
                            t.Add(s);
                        }                              
                    }
                }
            } 
        }      
    }      
          

    client = null;        
    if(sendFile && !string.IsNullOrEmpty(filename)) {            
        byte[] dataBytes = Encoding.ASCII.GetBytes(d.ToString());                
        FileResult file = new FileContentResult(dataBytes,"application/octet-stream");
        //log.LogInformation("Got this far.");
        file.FileDownloadName = filename.EndsWith(".gpx")?filename:filename+".gpx";
        //log.LogInformation("But not this far.");
        return (ActionResult)file;
    }
    else{
        return (ActionResult)new OkObjectResult(d.ToString());
    }
}


