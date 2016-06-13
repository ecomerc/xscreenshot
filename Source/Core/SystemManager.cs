// 
// SystemManager.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
// 
// Copyright (c) 2010 Jonathan Pobst
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using System;
using System.IO;
using System.Runtime.InteropServices;

namespace xscreenshot.Core {


    public static class SystemManager {
        private static OS operating_system;

        public static int RenderThreads { get; set; }
        public static OS OperatingSystem { get { return operating_system; } }

        static SystemManager() {
            if (Path.DirectorySeparatorChar == '\\')
                operating_system = OS.Windows;
            else if (IsRunningOnMac())
                operating_system = OS.Mac;
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
                operating_system = OS.X11;
            else
                operating_system = OS.Other;


            RenderThreads = Environment.ProcessorCount;
        }




        public static string GetExecutablePathName() {
            string executablePathName = System.Environment.GetCommandLineArgs()[0];
            executablePathName = System.IO.Path.GetFullPath(executablePathName);

            return executablePathName;
        }

        public static string GetExecutableDirectory() {
            return Path.GetDirectoryName(GetExecutablePathName());
        }

        public static OS GetOperatingSystem() {
            return operating_system;
        }


        [DllImport("glib-2.0.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr g_get_language_names();

        //From Managed.Windows.Forms/XplatUI
        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        static bool IsRunningOnMac() {
            IntPtr buf = IntPtr.Zero;
            try {
                buf = Marshal.AllocHGlobal(8192);
                // This is a hacktastic way of getting sysname from uname ()
                if (uname(buf) == 0) {
                    string os = Marshal.PtrToStringAnsi(buf);
                    if (os == "Darwin")
                        return true;
                }
            } catch {
            } finally {
                if (buf != IntPtr.Zero)
                    Marshal.FreeHGlobal(buf);
            }
            return false;
        }
    }

    public enum OS {
        Windows,
        Mac,
        X11,
        Other
    }
}