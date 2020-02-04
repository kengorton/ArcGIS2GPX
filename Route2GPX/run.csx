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
    serviceUrl = serviceUrl.Substring(0,serviceUrl.LastIndexOf("FeatureServer"))+"FeatureServer";
    string serviceInfoUrl = serviceUrl+"/info/itemInfo?f=pjson";
    string serviceQueryUrl = serviceUrl+"?f=pjson";
    string where = req.Query["where"];
    where = where ?? "1=1";
    string pointQueryUrl = serviceUrl+"/0/query?where="+where+"&outSR=4326&outFields=*&f=pjson";
    string routeQueryUrl = serviceUrl+"/2/query?where="+where+"&outSR=4326&outFields=*&f=pjson";
    
    HttpClient client = new HttpClient();
        // get the name of the route
    var result = await client.GetAsync(serviceInfoUrl);
    string resultContent = await result.Content.ReadAsStringAsync();
    var details = JObject.Parse(resultContent);
    string title =  (string)details["title"];
    //String uri = serviceQueryUrl;

    result = await client.GetAsync(serviceQueryUrl);
    resultContent = await result.Content.ReadAsStringAsync();
    details = JObject.Parse(resultContent);
    JArray layers =  (JArray)details["layers"];
    if(layers.Count != 4){
        return new BadRequestObjectResult(String.Format("{0} is not an ArcGIS route feature service.",title));
    }

    string capabilities = (string)details["capabilities"];
    if (!capabilities.Contains("Query")){
        return new BadRequestObjectResult(String.Format("{0} does not allow Querying.",title));
    }
    JObject pointLayer = (JObject)layers[0];
    if((string)pointLayer["geometryType"] != "esriGeometryPoint"){
        return new BadRequestObjectResult(String.Format("Layer {1} of {0} is not a point layer.",title,(string)pointLayer["name"]));
    }
    JObject routeLayer = (JObject)layers[2];
    if((string)routeLayer["geometryType"] != "esriGeometryPolyline"){
        return new BadRequestObjectResult(String.Format("Layer {1} of {0} is not a polyline.",title,(string)routeLayer["name"]));
    }

    // get the points
    result = await client.GetAsync(pointQueryUrl);
    resultContent = await result.Content.ReadAsStringAsync();
    details = JObject.Parse(resultContent);
    JArray points =  (JArray)details["features"]; 
    

    // get the route
    result = await client.GetAsync(routeQueryUrl);
    resultContent = await result.Content.ReadAsStringAsync();
    details = JObject.Parse(resultContent);
    JArray lines =  (JArray)details["features"]; 
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

    foreach (JObject f in points.Children<JObject>()){
        string wptName = (string)f["attributes"]["Name"];
        if(string.IsNullOrEmpty(wptName)||string.IsNullOrWhiteSpace(wptName)||wptName=="null"){
            wptName = title + " Wpt " + (int)f["attributes"]["Sequence"];
        }
        XElement w = new XElement("wpt",
                    new XAttribute("lat",(double)f["geometry"]["y"]),
                    new XAttribute("lon",(double)f["geometry"]["x"]),
            new XElement("name",wptName),
            new XElement("desc",(string)f["attributes"]["DisplayText"])                                    
        );
        g.Add(w);
        

        //routeGeom = (JObject)f["geometry"];                              
    }
            

    foreach (JObject f in lines.Children<JObject>()){
        
        XElement t = new XElement("trk",
            new XElement("name",title),
            new XElement("desc",(string)f["attributes"]["RouteName"])                                      
        );
        double totalMinutes = (double)f["attributes"]["TotalMinutes"];
        double meters = (double)f["attributes"]["TotalMeters"];
        int miles = Convert.ToInt32(meters/1609);
        int kilometers = Convert.ToInt32(meters/1000);
        int hours = (int)(totalMinutes/60);
        int minutes = Convert.ToInt32(totalMinutes-(hours*60));
        string costs =string.Format("Route length: {0} mi ({1} km); {2} hrs, {3} min.",miles,kilometers,hours,minutes);
        t.Add(new XElement("cmt",costs));


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
    
    byte[] dataBytes = Encoding.ASCII.GetBytes(d.ToString());

    client = null;  
    
    FileResult file = new FileContentResult(dataBytes,"application/octet-stream");
    file.FileDownloadName = title+".gpx";
    return (ActionResult)file;
        
}

