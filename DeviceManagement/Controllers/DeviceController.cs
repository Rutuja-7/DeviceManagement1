using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using DeviceManagement.Models;
using DeviceManagement.DAL.Repositories;
using Microsoft.Azure.Devices;

namespace DeviceManagement.Controllers
{
    [ApiController]
    [Route("Controller")]
    public class DeviceController : Controller
    {
        static RegistryManager registryManager;
        private const string IOT_Hub_Conn_String = "HostName=demoiothubrutuja.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VibIqKxix5SDrH0OSSrs/AMsc4ihGGafWPKZIXA4C6o=";
        
        public DeviceController()
        {
            registryManager = RegistryManager.CreateFromConnectionString(IOT_Hub_Conn_String);
        }

            

        /// <summary>
        /// Add device with deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="reportedProperties"></param>
        /// <returns> Task<string> </returns>
        [HttpPost]
        [Route("AddDevice")]
        public async Task<string> AddDevice(string deviceId)
        {
            var result = await DeviceRepository.AddDeviceAsync(deviceId);
            if(result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Update the device(deviceTwin) with deviceId 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="reportedProperties"></param>
        /// <returns> Task<string> </returns>        
        [HttpPut]
        [Route("UpdateReportedProperties")]
        public async Task<string> UpdateReportedProperties(string deviceId, ReportedProperties reportedProperties)
        {
            var result = await DeviceRepository.UpdateReportedProperties(deviceId, reportedProperties);
            return result;
        }


        [HttpPut]
        [Route("UpdateDesiredProperties")]
        public async Task<string> UpdateDesiredProperties(string deviceId, DesiredProperties desiredProperties)
        {
            var result = await DeviceRepository.UpdateDesiredProperties(deviceId, desiredProperties);
            return result;
        }

        /// <summary>
        /// Send Message from device to IoT Hub with deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="reportedProperties"></param>
        [HttpPut]
        [Route("sendmessage/{deviceId}")]
        public async Task<bool> sendMessage(string deviceId, ReportedProperties reportedProperties)
        {
            var result = await DeviceRepository.SendMessageToIoTHub(deviceId, reportedProperties);
            return result;
        }


        /// <summary>
        /// Get Device Twin of a particular device with deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns> Task<string> </returns>
        [HttpGet]
        [Route("GetDevice")]
        public async Task<string> GetDeviceTwin(string deviceId)
        {
            var result = await DeviceRepository.GetDeviceTwinAsync(deviceId);
            return result;
        }


        /// <summary>
        /// List all available device twin 
        /// </summary>
        /// <returns> Task<List<DeviceViewModel>> </returns>
        [HttpGet]
        [Route("GetAllDevices")]
        public async Task<List<DeviceViewModel>> GetAllDevicesTwin()
        {
            var result = await DeviceRepository.GetAllDeviceTwinAsync();
            return result;
        }


        /// <summary>
        /// Enable a device with deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns> Task<Device> </returns>
        [HttpPatch]
        [Route("enable/{deviceId}")]
        public async Task<Device> EnableDevice(string deviceId)
        {
            var result = await DeviceRepository.EnableDeviceAsync(deviceId);
            return result;
        }


        /// <summary>
        /// Disable a device with deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns> Task<Device> </returns>
        [HttpPatch]
        [Route("disable/{deviceId}")]
        public async Task<Device> DisableDevice(string deviceId)
        {
            var result = await DeviceRepository.DisablDveiceAsync(deviceId);
            return result;
        }
        /// <summary>
        /// Deletes a particular device with deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns> Task<bool> </returns>
        [HttpDelete]
        [Route("DeleteDevice")]
        public async Task<bool> DeleteDevice(string deviceId)
        {
            var result = await DeviceRepository.DeleteDeviceAsync(deviceId);
            if(result == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }        
    }
}
