using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using JsonConfig;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Diagnostics;
using xscreenshot.Extensions;
using Xamarin.UITest;

namespace xscreenshot {
    class Program {




        static void Main(string[] args) {

            if (args.Count() > 0 && File.Exists(args[0])) {
                Utilities.TryLoadConfig(args[0]);
                Utilities.TryLoadScript(args[0]);
            }

            if (args.Count() > 1 && File.Exists(args[1])) {
                Utilities.TryLoadConfig(args[1]);
                Utilities.TryLoadScript(args[1]);
            }


            iOS.iOSHelpers.RunSimulatorsForAction(TakeScreenshots);


            Console.WriteLine("All done");

        }

        static void TakeScreenshots(IApp app, string outputPath) {

            var screenShotIndex = 0;

            Action<string> screenshotAction = (string title) => {
                try {
                    screenShotIndex++;
                    var fi = app.Screenshot(title);
                    var filename = outputPath + "." + screenShotIndex.ToString() + ".png";
                    Console.WriteLine(filename);

                    fi.MoveTo(filename);
                } catch (Exception ex) {
                    Console.WriteLine(string.Format("Could not capture screenshot: {0}", ex.Message));
                }
            };

            if (Utilities.ScriptObject != null) {
                MethodInfo o = Utilities.ScriptObject.GetType().GetMethod("c");

                o.Invoke(o, new object[] { app, screenshotAction });
            } else {
                screenshotAction("single");
            }

        }


    }
}
