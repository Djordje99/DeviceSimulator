using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Text;

DeviceClient deviceClient;
string input = "";
string connectionString = "";
bool isWorking = true;
const string IOT_HUB_DEVICE_LOCATION = "West Europe";
const string IOT_HUB_DEVICE = "device-1";



Console.WriteLine("Enter a connection string for Azure IoT Hub: ");
connectionString = Console.ReadLine();

try
{
    deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
    Console.WriteLine("Successfuly connected to Azure IoT Hub.\n");
}
catch (Exception ex)
{
    Console.WriteLine($"\n[Error] {DateTime.Now} >>> {ex.Message}\n");
    return;
}

while (input != "0")
{
    Console.WriteLine("1. Send message to cloud");
    Console.WriteLine("2. Send a random message over defined period of time ");
    Console.WriteLine("3. List all desired properties");
    Console.WriteLine("4. List all reported properties");
    Console.WriteLine("5. Set reported properties");
    Console.WriteLine("6. Set callback message handler");
    Console.WriteLine("0. exit");
    Console.WriteLine("\nPlease enter a number from [0-4]:");

    input = Console.ReadLine();

    switch (input)
    {
        case "0":
            break;
        case "1":
            await SendMessageToIoTHubAsync();
            break;
        case "2":
            await SendRandomMessageOverTime();
            break;
        case "3":
            await ListAllTwinDesiredProperties();
            break;
        case "4":
            await ListAllTwinrReportedProperties();
            break;
        case "5":
            await SetReportedTwinProperties();
            break;
        case "6":
            await SetMessageHandler();
            break;
        default:
            Console.WriteLine("Please enter a valid number.");
            break;
    }
}

deviceClient.Dispose();

async Task SendMessageToIoTHubAsync()
{
    Console.WriteLine("\nPlease enter a message:");

    input = Console.ReadLine();

    var payload = new AzureIotMessage
    {
        DeviceId = IOT_HUB_DEVICE,
        Location = IOT_HUB_DEVICE_LOCATION,
        Message = input,
        Timestamp = DateTime.Now.ToString(),
    };

    string jsonPayload = JsonConvert.SerializeObject(payload);

    var msg = new Message(Encoding.UTF8.GetBytes(jsonPayload));

    Console.WriteLine($"\n[Info] {DateTime.Now} >>> Sending message: [{jsonPayload}]\n");


    try
    {
        await deviceClient.SendEventAsync(msg);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[Error] {DateTime.Now} >>> {ex.Message}\n");
    }
}

async Task SendRandomMessageOverTime()
{
    int messageNumber;
    int timePeriod;

    Console.WriteLine("\nEnter a number of messages: ");

    if(!int.TryParse(Console.ReadLine(), out messageNumber))
    {
        Console.WriteLine("\nEnter a valid int number.");
        return;
    }

    Console.WriteLine("\nEnter a time period over which will messages be sent (in miliseconds)");

    if (!int.TryParse(Console.ReadLine(), out timePeriod))
    {
        Console.WriteLine("\nEnter a valid int number.");
        return;
    }

    Random random = new Random();

    for (int i = 0; i < messageNumber; i++)
    {
        try
        {
            var payload = new AzureIotMessage
            {
                DeviceId = IOT_HUB_DEVICE,
                Location = IOT_HUB_DEVICE_LOCATION,
                Message = $"Random message {random.Next(0, messageNumber)}",
                Timestamp = DateTime.Now.ToString(),
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);

            var msg = new Message(Encoding.UTF8.GetBytes(jsonPayload));

            Console.WriteLine($"\n[Info] {DateTime.Now} >>> Sending message: [{jsonPayload}]\n");

            await deviceClient.SendEventAsync(msg);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[Error] {DateTime.Now} >>> {ex.Message}\n");
        }

        Thread.Sleep(timePeriod);
    }
}

async Task ListAllTwinDesiredProperties()
{
    Twin twin = await deviceClient.GetTwinAsync();
    TwinCollection twinDesiredCollection = twin.Properties.Desired;

    Console.WriteLine("\n----------------------------------");
    Console.WriteLine("| \t Key \t|\t Value \t |");
    Console.WriteLine("----------------------------------");

    foreach (KeyValuePair<string, object> property in twinDesiredCollection)
    {
        Console.WriteLine($"| \t {property.Key} \t|\t {property.Value} \t |");
    }

    Console.WriteLine("----------------------------------\n");
}

async Task ListAllTwinrReportedProperties()
{
    Twin twin = await deviceClient.GetTwinAsync();
    TwinCollection twinReportedCollection = twin.Properties.Reported;

    Console.WriteLine("\n----------------------------------");
    Console.WriteLine("| \t Key \t|\t Value \t |");
    Console.WriteLine("----------------------------------");

    foreach (KeyValuePair<string, object> property in twinReportedCollection)
    {
        Console.WriteLine($"| \t {property.Key} \t|\t {property.Value} \t |");
    }

    Console.WriteLine("----------------------------------\n");
}

async Task SetReportedTwinProperties()
{
    Twin twin = await deviceClient.GetTwinAsync();

    TwinCollection twinReportedCollection = new TwinCollection();
    TwinCollection twinDesiredCollection = twin.Properties.Desired;
    string input = "";

    foreach (KeyValuePair<string, object> property in twinDesiredCollection)
    {
        Console.WriteLine($"For key [{property.Key}] enter value: ");

        input = Console.ReadLine();

        twinReportedCollection[property.Key] = input;
    }

    await deviceClient.UpdateReportedPropertiesAsync(twinReportedCollection);
}

async Task SetMessageHandler()
{
    await deviceClient.SetReceiveMessageHandlerAsync(MessageReceivedAsync, null);
}

async Task MessageReceivedAsync(Message message, object userContext)
{
    string messageData = Encoding.UTF8.GetString(message.GetBytes());
    CloudMessage cloudMessage = JsonConvert.DeserializeObject<CloudMessage>(messageData);

    Console.WriteLine($"[Info] {DateTime.Now} Recieved Message >>>  {messageData}");
}

class AzureIotMessage()
{
    public string DeviceId { get; set; }
    public string Location { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
}

class CloudMessage
{
    public string Value { get; set; }
}