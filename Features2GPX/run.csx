#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml.Linq;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");
    

    string serviceUrl = req.Query["url"];
    if (string.IsNullOrEmpty(serviceUrl)){
        return new BadRequestObjectResult(String.Format("No ArcGIS Online route layer url."));
    }
    if (!serviceUrl.Contains("FeatureServer")){
        return new BadRequestObjectResult(String.Format("The url {0} is not to an ArcGIS feature service.",serviceUrl));
    }
    //serviceUrl = url;
    string serviceInfoUrl = serviceUrl+"/info/itemInfo?f=pjson";
    string serviceQueryUrl = serviceUrl+"?f=pjson";
    string where = req.Query["where"];
    where = where ?? "1=1";
    string nameF = req.Query["nameField"];
    string descF = req.Query["descField"];
    string cmtF = req.Query["cmtField"];
    string symF = req.Query["symField"];
    //string pointQueryUrl = serviceUrl+"/0/query?where="+where+"&outSR=4326&outFields=*&f=pjson";
    string featureQueryUrl = serviceUrl+"/query?where="+where+"&outSR=4326&outFields=*&f=pjson";
    
    HttpClient client = new HttpClient();
        // get the name of the route
    //var result = await client.GetAsync(serviceInfoUrl);
    //string resultContent = await result.Content.ReadAsStringAsync();
    //var details = JObject.Parse(resultContent);
    //string title =  (string)details["title"];
    //String uri = serviceQueryUrl;

    var result = await client.GetAsync(serviceQueryUrl);
    var resultContent = await result.Content.ReadAsStringAsync();
    var details = JObject.Parse(resultContent);
    //JArray layers =  (JArray)details["layers"];
    //if(layers.Count != 4){
    //    return new BadRequestObjectResult(String.Format("{0} is not an ArcGIS route feature service.",title));
    //}
    string title =  (string)details["name"];

    string capabilities = (string)details["capabilities"];
    if (!capabilities.Contains("Query")){
        return new BadRequestObjectResult(String.Format("{0} does not allow Querying.",title));
    }
    //JObject pointLayer = (JObject)layers[0];
    //if((string)pointLayer["geometryType"] != "esriGeometryPoint"){
    //    return new BadRequestObjectResult(String.Format("Layer {1} of {0} is not a point layer.",title,(string)pointLayer["name"]));
    //}
    string geometryType = (string)details["geometryType"];
    if(geometryType != "esriGeometryPoint" && geometryType != "esriGeometryPolyline"){
        return new BadRequestObjectResult(String.Format("Layer {0} is neither a point nor a line layer.",title));
    }

    // get the points
    //result = await client.GetAsync(pointQueryUrl);
    //resultContent = await result.Content.ReadAsStringAsync();
    //details = JObject.Parse(resultContent);
    //JArray points =  (JArray)details["features"]; 
    

    // get the features
    result = await client.GetAsync(featureQueryUrl);
    resultContent = await result.Content.ReadAsStringAsync();
    details = JObject.Parse(resultContent);
    JArray features =  (JArray)details["features"]; 
    //string routeName = title;

        
    
    //XElement g = new XElement("gpx");
    XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
    XNamespace gpx1 = "http://www.topografix.com/GPX/1/1/";
    XNamespace gpx2 = "http://www.topografix.com/GPX/1/1/gpx.xsd";
    XElement g = new XElement(gpx1+"gpx", 
        new XAttribute("creator","http://www.cagegis.com/"),
        new XAttribute("version","1.1"),
        new XAttribute(XNamespace.Xmlns+"xsi",xsi.NamespaceName),
        new XAttribute(xsi+"schemaLocation",gpx1.NamespaceName + " " + gpx2.NamespaceName));
    g.Add(new XElement("metadata",  
        new XElement("name", title)  
        ));
    XDocument d = new XDocument(new XDeclaration("1.0", "utf-8", "true"),g);
    //d.Add(g);

    if(geometryType == "esriGeometryPoint"){
        int count = 1;
        foreach (JObject f in features.Children<JObject>()){
            string wptName = (string)f["attributes"][nameF];
            if(string.IsNullOrEmpty(wptName)||string.IsNullOrWhiteSpace(wptName)||wptName=="null"){
                wptName = String.Format("{0} Wpt {1}",title,count);
            }
            XElement w = new XElement("wpt",
                        new XAttribute("lat",(double)f["geometry"]["y"]),
                        new XAttribute("lon",(double)f["geometry"]["x"]),
                new XElement("name",wptName),
                new XElement("desc",(string)f["attributes"][descF]),
                new XElement("cmt",(string)f["attributes"][cmtF]),
                new XElement("sym",(string)f["attributes"][symF])                                   
            );
            g.Add(w);   
            count++;         

            //routeGeom = (JObject)f["geometry"];                              
        }
    }
            
    if(geometryType == "esriGeometryPolyline"){
        foreach (JObject f in features.Children<JObject>()){
            //string name = ;
            //string desc = (string)f["attributes"][descF];
            //string cmt = (string)f["attributes"][cmtF];
            
            XElement t = new XElement("trk",
                new XElement("name",(string)f["attributes"][nameF]),
                new XElement("desc",(string)f["attributes"][descF]),
                new XElement("cmt",(string)f["attributes"][cmtF])                                      
            );

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
    
    byte[] dataBytes = Encoding.ASCII.GetBytes(d.ToString());

    client = null;  
    
    FileResult file = new FileContentResult(dataBytes,"application/octet-stream");
    file.FileDownloadName = title+".gpx";
    return (ActionResult)file;

        
}


