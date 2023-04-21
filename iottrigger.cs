// some changes where made according to the workshop 5 https://cmp9785m-cloud-development-2023.github.io/CloudDevelopment/workshop5.html
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
 using System.Collections.Generic;

using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;


namespace iot.trigger
{
    public class UtilityItem
  {
    [JsonProperty("id")]
    public string Id {get; set;}
    public double flat_a_Cookinggas {get; set;}
    public double flat_a_Electricity {get; set;}
    public double flat_a_Water {get; set;}
    public double flat_b_Cookinggas {get; set;}
    public double flat_b_Electricity {get; set;}
    public double flat_b_Water {get; set;}
  }

    public class iottrigger
    {
        private static HttpClient client = new HttpClient();
        
        [FunctionName("iottrigger")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "AzureEventHubConnectionString")] EventData message,
        [CosmosDB(databaseName: "IoTData",
                                 collectionName: "buildingData",
                                 ConnectionStringSetting = "cosmosDBConnectionString")] out UtilityItem output,
                       ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
            var jsonBody = Encoding.UTF8.GetString(message.Body);
dynamic data = JsonConvert.DeserializeObject(jsonBody);
double flat_a_Cookinggas = data.flat_a_Cookinggas;
double flat_a_Electricity = data.flat_a_Electricity;
double flat_a_Water = data.flat_a_Water;
double flat_b_Cookinggas = data.flat_b_Cookinggas;
double flat_b_Electricity = data.flat_b_Electricity;
double flat_b_Water = data.flat_b_Water;

output = new UtilityItem
 {
    flat_a_Cookinggas = flat_a_Cookinggas,
    flat_a_Electricity = flat_a_Electricity,
    flat_a_Water = flat_a_Water,
    flat_b_Cookinggas = flat_b_Cookinggas,
    flat_b_Electricity = flat_b_Electricity,
    flat_b_Water = flat_b_Water
};

            
            }

        [FunctionName("GetUtilities")]
public static IActionResult GetUtilities(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "utilities/")] HttpRequest req,
    [CosmosDB(databaseName: "IoTData",
              collectionName: "buildingData",
              ConnectionStringSetting = "cosmosDBConnectionString",
              SqlQuery = "SELECT ROUND(c.flat_a_Cookinggas, 2) AS flat_a_Cookinggas,ROUND(c.flat_a_Electricity, 2) AS flat_a_Electricity,ROUND(c.flat_a_Water, 2) AS flat_a_Water,ROUND(c.flat_b_Cookinggas, 2) AS flat_b_Cookinggas, ROUND(c.flat_b_Electricity, 2) AS flat_b_Electricity,ROUND(c.flat_b_Water, 2) AS flat_b_Water FROM c ORDER BY c._ts DESC")] IEnumerable<UtilityItem> utilityItems,
    ILogger log)
{
    // table creation
    string table = "<table style=\"margin:0 auto;\">";

    // creation of titles of colums
    table += "<tr style=\"background: #1b4965;color: white;\"><th colspan=\"2\">Cooking Gass</th><th colspan=\"2\">Electricity</th><th colspan=\"2\">Water</th></tr>";

    table +="<tr style=\"background: #1b4965;color: white;\"><th style=\"width: 100px; text-align: center;\">Flat A</th><th style=\"width: 100px; text-align: center;\">Flat B</th><th style=\"width: 100px; text-align: center;\"> Flat A</th><th style=\"width: 100px; text-align: center;\">Flat B</th><th style=\"width: 100px; text-align: center;\">Flat A</th><th style=\"width: 100px; text-align: center;\">Flat B</th></tr>";

    // rows creation for each utility items
    foreach (var item in utilityItems)
    {
        table += $"<tr style=\"background: #edf2f4\"><td style=\"width: 100px; text-align: center;\">{item.flat_a_Cookinggas}</td><td style=\"width: 100px; text-align: center;\">{item.flat_b_Cookinggas}</td><td style=\"width: 100px; text-align: center;\">{item.flat_a_Electricity}</td><td style=\"width: 100px; text-align: center;\">{item.flat_b_Electricity}</td><td style=\"width: 100px; text-align: center;\">{item.flat_a_Water}</td><td style=\"width: 100px; text-align: center;\">{item.flat_b_Water}</td></tr>";
    }

    // closing table
    table += "</table>";

    return new ContentResult
    {
        Content = table,
        ContentType = "text/html",
        StatusCode = 200
    };
}

    }
}