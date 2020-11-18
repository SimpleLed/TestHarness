using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Driver.Corsair;
using Driver.RGBnet;
using Source.RainbowWave;

//you can either use nuget to get simple led or add the project to this solution. Solution will be in GIT as nuget.
using SimpleLed;

namespace MadLedSDK
{
    class Program
    {
        static List<ControlDevice> devices = new List<ControlDevice>();
        static Dictionary<int, ControlDevice> driv = new Dictionary<int, ControlDevice>();

        static void ShowDevices()
        {
            try
            {
                driv = new Dictionary<int, ControlDevice>();
                int ct = 1;

                foreach (var controlDevice in devices.ToArray())
                {
                    Console.WriteLine(ct + ": " + controlDevice.Driver.Name() + "-" + controlDevice.Name + " - " +
                                      controlDevice.DeviceType + ", has configUI: " +
                                      (controlDevice.Driver is ISimpleLedWithConfig));
                    driv.Add(ct, controlDevice);

                    ct++;
                }
            }
            catch
            {
            }

        }

        static void Main(string[] args)
        {

            var apiClient = new SimpleLedApiClient();
            var driverProps = apiClient.GetProductsByCategory(ProductCategory.Effect).Result;

            try
            {
                Directory.CreateDirectory("SLSConfigs");
            }
            catch
            {
            }

     
            DummyForm dummy = new DummyForm();
            
            
            SLSManager ledManager = new SLSManager("SLSConfigs");

            ledManager.DeviceAdded += LedManager_DeviceAdded;
            ledManager.DeviceRemoved += LedManager_DeviceRemoved;
            //Add drivers manually like the example below.
            //you wll need to add the driver csproj too.
            //you will need to add at LEAST two - one for source, one for dest
            //ledManager.Drivers.Add(new CUEDriver());
            
            ledManager.Drivers.Add(new RainbowWaveDriver());

            ledManager.Drivers.Add(new RGBNetDriver());

            ledManager.RescanRequired += LedManager_RescanRequired;
            
            ledManager.Init();
            Console.WriteLine("Getting devices");
            

            
           
            string derp = "";
            ControlDevice cycleFan = null;

            var timer = new Timer((state) =>
            {
                if (cycleFan != null)
                {
                    foreach (var t in devices.Where(xx =>
                        xx.Driver.GetProperties().SupportsPush && xx.LEDs != null && xx.LEDs.Length > 0))
                    {
                        if (cycleFan.Driver.GetProperties().SupportsPull)
                        {
                            cycleFan.Pull();
                        }

                        t.MapLEDs(cycleFan);
                        t.Push();
                    }
                }

            }, null, 0, 33);

            while (true)
            {
                Console.WriteLine("Type Source Number (Q TO QUIT, S TO SAVE CFG, L TO LOAD CFG)");
                derp = Console.ReadLine();
                if (derp.ToUpper() == "Q")
                {
                    return;
                }

                if (derp.ToUpper() == "S")
                {
                    ledManager.SaveConfigs();
                }

                else if (derp.ToUpper() == "L")
                {
                    ledManager.LoadConfigs();
                }
                else
                {
                    cycleFan = driv[int.Parse(derp)];
                }

            }
            
        }

        private static void LedManager_DeviceRemoved(object sender, Events.DeviceChangeEventArgs e)
        {
            Console.WriteLine("Device Removed: " + e.ControlDevice.Name);
            devices.Remove(e.ControlDevice);
            ShowDevices();
        }

        private static void LedManager_DeviceAdded(object sender, Events.DeviceChangeEventArgs e)
        {
            Console.WriteLine("Device Added: "+e.ControlDevice.Name);
            devices.Add(e.ControlDevice);
            ShowDevices();
        }

        private static void LedManager_RescanRequired(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
