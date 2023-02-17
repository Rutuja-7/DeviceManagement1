using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Unicode;
using DeviceManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeviceManagement.DAL.Repositories
{
    public class DeviceRepository
    {
        private static DeviceClient deviceClient;
        private static RegistryManager registryManager;
        private const string IOT_HUB_CONN_STRING = "HostName=rutujaiothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=PucoHwtccB3vouKPrwwWki6C6hY+eHNyhODDSW20i+M=";

        public DeviceRepository()
        {
            registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
        }

        public static async Task<string> GetDeviceTwinAsync(string deviceId)
        {
            if(deviceId != null)
            {
                registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
                Twin twin = await registryManager.GetTwinAsync(deviceId);
                TwinCollection propReportedProperties = twin.Properties.Reported;
                TwinCollection propDesiredPr = twin.Properties.Desired;

                var result = propDesiredPr.ToString() + propReportedProperties.ToString();

                Console.WriteLine("Device Twin : {0}", result);

                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Console.WriteLine("DeviceId should not be null");
                return null;
            }
        }

        public static async void InitClient(string deviceId)
        {
            try
            {
                Console.WriteLine("Connecting to hub....");
                deviceClient = DeviceClient.CreateFromConnectionString(IOT_HUB_CONN_STRING, deviceId, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                Console.WriteLine("Retriving twin...");
                await deviceClient.GetTwinAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Sample : {0}", ex.Message);
            }         
        }

        public static async Task<Device> EnableDeviceAsync(string deviceId)
        {
            registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
            Device device = await registryManager.GetDeviceAsync(deviceId);

            if(device != null) 
            {
                device.Status = DeviceStatus.Enabled;
                Device data = await registryManager.UpdateDeviceAsync(device);
                if(data != null) 
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static async Task<bool> CheckIfDeviceIsEnable(string deviceId)
        {
            registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
            Device device= await registryManager.GetDeviceAsync(deviceId);
            if (device.Status == DeviceStatus.Enabled)
            {
                return true;
            }
            else
            {
                return false;   
            }
        }

        public static async Task<Device> DisablDveiceAsync(string deviceId)
        {
            registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
            Device device = await registryManager.GetDeviceAsync(deviceId);
            if(device != null) 
            {
                device.Status = DeviceStatus.Disabled;
                Device data = await registryManager.UpdateDeviceAsync(device);
                if(data != null)
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static async Task<bool> DeleteDeviceAsync(string deviceId)
        {
            try
            {
                registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
                await registryManager.RemoveDeviceAsync(deviceId);

                Device device = await registryManager.GetDeviceAsync(deviceId);
                if (device != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine();
                Console.WriteLine("Error in deleting the device : {0}", ex.Message);
                return false;
            }
        }


        public static async Task<List<DeviceViewModel>> GetAllDeviceTwinAsync()
        {
            registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
            var query = registryManager.CreateQuery("SELECT * FROM devices");
            var allDeviceTwin = await query.GetNextAsTwinAsync();
            var twinString = allDeviceTwin.Select(t => new DeviceViewModel
            {
                DeviceId = t.DeviceId,
                Status = t.Status.ToString(),
                LastActivityTime = t.LastActivityTime.ToString(),
                ConnectionStatus = t.ConnectionState.ToString(),
                DesiredProperties = new DesiredProperties()
                {
                    SensorType = (int)JObject.Parse(t.Properties.Desired.ToJson())["sensorType"],
                    MaximumPressure = (int)JObject.Parse(t.Properties.Desired.ToJson())["maximumPressure"],
                },
                ReportedProperties = new ReportedProperties()
                {
                    Temperature = (string)JObject.Parse(t.Properties.Reported.ToJson())["temperature"],
                    Pressure = (string)JObject.Parse(t.Properties.Reported.ToJson())["pressure"],
                    Frequency = (string)JObject.Parse(t.Properties.Reported.ToJson())["frequency"]
                }
            }).ToList();

            Console.WriteLine("All Devices : {0}", twinString);
            return twinString;
        }


        public static async Task<string> AddDeviceAsync(string deviceId, ReportedProperties reportedProperties)
        {
            if(string.IsNullOrEmpty(deviceId)) 
            {
                throw new ArgumentNullException("deviceId");        
            }

            Device device;
            registryManager = RegistryManager.CreateFromConnectionString(IOT_HUB_CONN_STRING);
            Console.WriteLine("New Device");

            device = await registryManager.AddDeviceAsync(new Device(deviceId));
            await UpdateReportedProperties(deviceId, reportedProperties);

            return await GetDeviceTwinAsync(deviceId);
        }

        public static async Task<string> UpdateReportedProperties(string deviceId, ReportedProperties reportedProperties)
        {
            if(await CheckIfDeviceIsEnable(deviceId))
            {
                try
                {
                    Console.WriteLine("Sending reported properties ");
                    InitClient(deviceId);
                    TwinCollection ReportedPropertiesColl, connectivity;
                    ReportedPropertiesColl = new TwinCollection();
                    connectivity = new TwinCollection();
                    connectivity["type"] = "cellular";
                    ReportedPropertiesColl["connectivity"] = connectivity;
                    ReportedPropertiesColl["temperature"] = reportedProperties.Temperature;
                    ReportedPropertiesColl["pressure"] = reportedProperties.Pressure;
                    ReportedPropertiesColl["frequency"] = reportedProperties.Frequency;

                    await deviceClient.UpdateReportedPropertiesAsync(ReportedPropertiesColl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error in updating reported properties : {0}", ex);
                }
            }            
            return await GetDeviceTwinAsync(deviceId);
        }


        public static async Task<string> UpdateDesiredProperties(string deviceId, DesiredProperties desiredProperties)
        {
            if(await CheckIfDeviceIsEnable(deviceId))
            {
                try
                {
                    var twin = await registryManager.GetTwinAsync(deviceId);
                    InitClient(deviceId);
                    var patch =
                        @"{
                            properties: {
                                desired: {
                                    sensorType: ""demo"",
                                    maximumPressure: ""mxq"",
                                }
                            }
                        }";
                    patch = patch.Replace("demo", desiredProperties.SensorType.ToString());
                    patch = patch.Replace("mxq", desiredProperties.MaximumPressure.ToString());

                    await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Error in updating desired properties: {ex.Message}");
                }
            }
            return await GetDeviceTwinAsync(deviceId);
        }


        public static async Task<bool> SendMessageToIoTHub(string deviceId, ReportedProperties reportedProperties)
        {
            if (await CheckIfDeviceIsEnable(deviceId))
            {
                try
                {
                    deviceClient = DeviceClient.CreateFromConnectionString(IOT_HUB_CONN_STRING, deviceId, Microsoft.Azure.Devices.Client.TransportType.Mqtt);

                    var telemetryData = new
                    {
                        deviceId = deviceId,
                        temperature = reportedProperties.Temperature,
                        pressure = reportedProperties.Pressure,
                        frequency = reportedProperties.Frequency,

                        pointInfo = "current one"
                    };

                    //serialize the telemetry data and convert it to json
                    var telemetryDataString = JsonConvert.SerializeObject(telemetryData);

                    //encode serialize object by UTF-8 so it can be parsed by IoT hub when processing messaging rules
                    using var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(telemetryDataString))
                    {
                        ContentEncoding = "utf-8",
                        ContentType = "apllication/json"
                    };

                    await UpdateReportedProperties(deviceId, reportedProperties);
                    //send the message to IoT hub
                    await deviceClient.SendEventAsync(message);
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error in sending message");
                    return false;
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Error in sending message");
                return false;   
            }                
        }
    }
}
