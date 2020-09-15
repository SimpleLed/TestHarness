using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
//you can either use nuget to get simple led or add the project to this solution. Solution will be in GIT as nuget.
using SimpleLed;

namespace MadLedSDK
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Directory.CreateDirectory("SLSConfigs");
            }
            catch
            {
            }

            SLSManager ledManager = new SLSManager("SLSConfigs");
            //Add drivers manually like the example below.
            //you wll need to add the driver csproj too.
            //you will need to add at LEAST two - one for source, one for dest
            //ledManager.Drivers.Add(new DriverHyperXAlloyRGB());
            ledManager.Init();
            Console.WriteLine("Getting devices");
            List<ControlDevice> devices = ledManager.GetDevices();

            Dictionary<int, ControlDevice> driv = new Dictionary<int, ControlDevice>();
            int ct = 1;
            foreach (var controlDevice in devices)
            {
                Console.WriteLine(ct + ": " + controlDevice.Driver.Name() + "-" + controlDevice.Name + " - " + controlDevice.DeviceType + ", " + controlDevice.Driver.GetProperties().Author);
                driv.Add(ct, controlDevice);

                ct++;
            }

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

    }
}
