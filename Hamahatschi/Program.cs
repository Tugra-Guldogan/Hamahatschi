/**
 * The MIT License (MIT)
 * 
 * Copyright (c) 2013
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Management;
using System.Reflection;
using System.IO;
using System.Media;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace Hamahatschi
{
    class Program
    {
        /// <summary>
        /// Name of the hamachi network adapter, default would be "Hamachi". This value has to be escaped.
        /// (I couldn't really find any .NET function that would do this and in all likelyhood, they'll
        /// never call it "Häm'atchì")
        /// </summary>
        const string NetworkAdapterName = "Hamachi";

        /// <summary>
        /// Name of the hamachi service.
        /// </summary>
        const string ServiceName = "LogMeIn Hamachi Tunneling Engine";

        /// <summary>
        /// Outputs an error message, waits for user input and then exits the program.
        /// This function can be used just like Console.WriteLine/String.Format.
        /// </summary>
        /// <param name="message">Message to be written in the console.</param>
        /// <param name="args">Any arguments to format the message with.</param>
        static void Error(string message, params object[] args)
        {
            Console.Error.WriteLine(String.Format(message, args));
            Console.ReadKey();
            Environment.Exit(-1);
        }

        static void Main(string[] args)
        {
            // Search the adapter
            Console.WriteLine("Searching adapter...");
            SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL AND NetConnectionId = '" + NetworkAdapterName + "'");
            ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);

            ManagementObjectCollection results = searchProcedure.Get();

            if (results.Count != 1)
                Error("FATAL ERROR: Cannot find proper Hamachi network adapter of type '{1}' (got {0} results; expected 1)", results.Count, NetworkAdapterName);

            // Indicators whether Hamachi is currently on or off
            int ons = 0, offs = 0;

            // Toggle the adapter
            ManagementObject adapter = results.Cast<ManagementObject>().First();
            bool adapterOn = (bool)adapter["NetEnabled"];

            if (adapterOn)
            {
                ons++;
                Console.WriteLine("Adapter is currently on...");
            }
            else
            {
                Console.WriteLine("Adapter is currently off...");
                offs++;
            }

            // Search the service
            Console.WriteLine("Searching service...");

            ServiceController service = ServiceController.GetServices().FirstOrDefault(s => s.DisplayName == ServiceName);

            if (service == null)
                Error("FATAL ERROR: Cannot find service '{0}'!", ServiceName);

            if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
            {
                ons++;
                Console.WriteLine("Service currently running.");
            }
            else
            {
                offs++;
                Console.WriteLine("Service currently stopped.");
            }

            // Whether we have to turn hamachi on or off
            bool turnOn = offs > ons;

            // If we have a pat
            if (offs != 0 && ons != 0)
            {
                Console.WriteLine("PROMPT: Cannot decide whether to start or stop hamachi. Type 'stop' or 'start'.");
                string input;
                do
                {
                    Console.Write("Command (start|stop): ");
                    input = Console.ReadLine().ToLower();
                } while (input != "start" && input != "stop");

                turnOn = input == "start";
            }

            // Turn hamachi on?
            if (turnOn)
            {
                // Turn the adapter on?
                if (!adapterOn)
                {
                    Console.WriteLine("Enabling Hamachi adapter...");
                    UInt32 ret = (UInt32)adapter.InvokeMethod("Enable", null);
                    if (ret != 0)
                        Error("FATAL ERROR: Cannot enable adapter (error code {0}).\nPlease check http://msdn.microsoft.com/en-us/library/aa394559%28v=vs.85%29.aspx", ret);
                    Console.WriteLine("Enabled.");
                }
                else
                    Console.WriteLine("Adapter already enabled.");

                // Turn the service on if it isn't running already
                if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)
                {
                    Console.WriteLine("Starting the service...");
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                    Console.WriteLine("Started.");
                }
            }
            // Turn hamachi off.
            else
            {
                // Stop the service if necessary
                if (service.Status != ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine("Stopping the service...");

                    if (!service.CanStop)
                        Error("FATAL ERROR: Service cannot be stopped!");

                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);

                    Console.WriteLine("Service disabled ({0}).", service.Status);
                }
                else
                    Console.WriteLine("Service currently disabled.");

                // Disable the adapter if necessary
                if (adapterOn)
                {
                    Console.WriteLine("Disabling Hamachi adapter...");
                    UInt32 ret = (UInt32)adapter.InvokeMethod("Disable", null);
                    if (ret != 0)
                        Error("FATAL ERROR: Cannot disable adapter (error code {0}).\nPlease check http://msdn.microsoft.com/en-us/library/aa394559%28v=vs.85%29.aspx", ret);
                    Console.WriteLine("Disabled.");
                }
                else
                    Console.WriteLine("Adapter already disabled.");
            }

            Console.WriteLine("Thank you for flying with Hamahatschi.");
        }
    }
}
