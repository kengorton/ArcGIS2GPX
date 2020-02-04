#r "Newtonsoft.Json"

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Net;
using Microsoft.Extensions.Primitives;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml.Linq;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");
    string serviceUrl = "";
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
    string filename = "";
    

    string errorHelp = "Usage:\n\n";
    string reqUrl = string.Format("{0}://{1}{2}",req.Scheme,req.Host,req.Path);
    errorHelp += string.Format("{0} converts ArcGIS features to GPX elements and returns a file of the data for use in GPS devices. THe GPX data follows the spec detailed at {1}.{2}",reqUrl,gpx1,"\n\n");
    errorHelp += string.Format("Function url: {0}{1}",reqUrl,"\n\n");
    errorHelp += "GET requests will accept one value for each of the parameters.\n";
    errorHelp += "POST requests will accept an array of JSON objects each containing one value for each of the parameters. This allows you to combine multiple point and/or polyline feature layers into a single output GPX file.\n\n";
    errorHelp += "Parameters:\n";
    errorHelp += "url: required. The url to a portal feature layer. This should end in 'FeatureService/n' where n is the layer index.\n";
    errorHelp += "returnFile: optional. If true, returns a GPX file. If false, returns a string. Defaults to true.\n";
    errorHelp += "where: optional. The where clause to limit the features included in the GPX data. Defaults '1=1'.\n";
    errorHelp += "name: optional. The name of the field in the feature layer that should provide the name values for each track or waypoint element.\n";
    errorHelp += "cmt: optional. The name of the field in the feature layer that should provide the cmt values for each track or waypoint element.\n";
    errorHelp += "desc: optional. The name of the field in the feature layer that should provide the desc values for each track or waypoint element.\n";
    errorHelp += "src: optional. The name of the field in the feature layer that should provide the src values for each track or waypoint element.\n";
    errorHelp += "type: optional. The name of the field in the feature layer that should provide the type values for each track or waypoint element.\n";
    errorHelp += "sym: optional. The name of the field in the feature layer that should provide the sym values for each waypoint element. Ignored for polyline layers.\n";
    errorHelp += "ele: optional. The name of the field in the feature layer that should provide the ele values for each waypoint element. If not present and if the point geometries have z values, the function will attempt to use feature geometry z values. If this parameter is included in the request the values from this field will supercede geometry z values. Ignored for polyline layers.\n";
    errorHelp += "time: optional. The name of the field in the feature layer that should provide the time values for each waypoint element. Ignored for polyline layers.\n";
    errorHelp += "magvar: optional. The name of the field in the feature layer that should provide the magvar values for each waypoint element. Ignored for polyline layers.\n";
    errorHelp += "geoidheight: optional. The name of the field in the feature layer that should provide the geoidheight values for each waypoint element. Ignored for polyline layers.\n";
    errorHelp += "fix: optional. The name of the field in the feature layer that should provide the fix values for each wptType element. Ignored for polyline layers.\n";
    errorHelp += "sat: optional. The name of the field in the feature layer that should provide the sat values for each wptType element. Ignored for polyline layers.\n";
    errorHelp += "hdop: optional. The name of the field in the feature layer that should provide the hdop values for each wptType element. Ignored for polyline layers.\n";
    errorHelp += "vdop: optional. The name of the field in the feature layer that should provide the vdop values for each wptType element. Ignored for polyline layers.\n";
    errorHelp += "pdop: optional. The name of the field in the feature layer that should provide the pdop values for each wptType element. Ignored for polyline layers.\n";
    errorHelp += "ageofgpsdata: optional. The name of the field in the feature layer that should provide the ageofgpsdata values for each wptType element. Ignored for polyline layers.\n";
    errorHelp += "dgpsid: optional. The name of the field in the feature layer that should provide the dgpsid values for each wptType element. Ignored for polyline layers.\n";
    errorHelp += "number: optional. The name of the field in the feature layer that should provide the number values for each trkType element. Ignored for point layers.\n\n";
    errorHelp += string.Format("example GET request: {0}{1}{2}\n\n",reqUrl,"?","url=https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>&returnFile=true&name=featureNameField");
    errorHelp += "Additionally, POST requests include the following:\n";
    errorHelp += "title: required. The name of the GPX file to be returned.\n";
    errorHelp += string.Format("example POST request payload:\n");
    //string payload = "{\"title\":\"A new GPX file\",\"returnFile\":false,\"layers\":[{\"url\":\"https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>\",\"name\":\"myNameField1\",\"desc\":\"myDescField1\",\"cmt\":\"myCmtField1\",\"sym\":\"mySymField1\"},{\"url\":\"https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>\",\"name\":\"myNameField2\",\"desc\":\"myDescField2\"},{\"url\":\"https://maps.arcgis.com/ArcGIS/rest/services/<<service name>>/FeatureServer/<<layer index>>\",\"name\":\"myNameField3\",\"desc\":\"myDescField3\",\"cmt\":\"myCmtField3\"}]}";
    string payload2 = "{\"title\":\"Some Trails\",\"returnFile\":\"true\",\"layers\":[{\"url\":\"https://services1.arcgis.com/1YRV70GwTj9GYxWK/arcgis/rest/services/Waterfalls/FeatureServer/1\",\"name\":\"TITLE\"},{\"url\":\"https://services1.arcgis.com/EvDRLcHhbHG5BnwT/arcgis/rest/services/Hiking_Routes/FeatureServer/0\",\"name\":\"Name\",\"desc\":\"Description\",\"where\":\"AscentFT>3600\"},{\"url\":\"https://services1.arcgis.com/1YRV70GwTj9GYxWK/arcgis/rest/services/BMW_RA_Rally_Fuel_and_Food/FeatureServer/0\",\"name\":\"name\",\"desc\":\"Address\"}]}";
    string jsonFormatted = JValue.Parse(payload2).ToString(Formatting.Indented);
    errorHelp += jsonFormatted;



    if (method == "GET"){


        serviceUrl = req.Query["url"];                      

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
        returnFile = req.Query["returnFile"];
        sendFile = returnFile == null ? true : Convert.ToBoolean(returnFile);

        var result = await client.GetAsync(serviceQueryUrl);
        var resultContent = await result.Content.ReadAsStringAsync();
        var details = JObject.Parse(resultContent);

        string title =  (string)details["name"];
        filename = title;

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


        g.Add(new XElement("metadata",  
            new XElement("name", title)  
            ));
                            

        if(geometryType == "esriGeometryPoint"){
            int count = 1;
            foreach (JObject f in features.Children<JObject>()){
                string wptName = (string)f["attributes"][name];
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
    else //method is POST
    {   
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);               
        string title = (string)data["title"]; 
        if(title == null){                    
            return new BadRequestObjectResult(String.Format("You must include a title.\n\n{0}",errorHelp));
        }
        returnFile = (string)data["returnFile"];
        sendFile = returnFile == null ? true : Convert.ToBoolean(returnFile);

        g.Add(new XElement("metadata",  
            new XElement("name", title)  
            ));

        filename = title;                    
        JArray layers = (JArray)data["layers"];   

        //loop through the feature layer and convert them to GPX. 
        //Do all the point layers first, then loop through againanad do the polyline layers.
        for (int i=0;i<2;i++){ 
            foreach (JObject layer in layers.Children<JObject>()) {
                serviceUrl = (string)layer["url"];
                serviceInfoUrl = serviceUrl+"/info/itemInfo?f=pjson";
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



                var result = await client.GetAsync(serviceQueryUrl);
                var resultContent = await result.Content.ReadAsStringAsync();
                var details = JObject.Parse(resultContent);

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
                        string wptName = (string)f["attributes"][name];
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
    if(sendFile) {            
        byte[] dataBytes = Encoding.ASCII.GetBytes(d.ToString());                
        FileResult file = new FileContentResult(dataBytes,"application/octet-stream");
        file.FileDownloadName = filename.EndsWith(".gpx")?filename:filename+".gpx";
        return (ActionResult)file;
    }
    else{
        return (ActionResult)new OkObjectResult(d.ToString());
    }
}
