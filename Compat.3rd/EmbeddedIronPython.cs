// Copyright (c) Hin-Tak Leung

// All rights reserved.

// MIT License

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the ""Software""), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.IO;
using System.Collections.Generic;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

using System.Reflection;

namespace Compat
{
    public class EmbeddedIronPython
    {
        // Runs my_class's my_method in my_script.
        // Caller is responsible for casting the returned object.        
        static public object RunPythonMethod(string my_script,
                                             string my_class,
                                             string my_method)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            ScriptSource source = engine.CreateScriptSourceFromFile(my_script);
            ObjectOperations op = engine.Operations;
            
            var paths = engine.GetSearchPaths();
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            paths.Add(path);
            engine.SetSearchPaths(paths);

            source.Execute(scope);

            object classObject = scope.GetVariable(my_class);
            object instance = op.Invoke(classObject);
            object method = op.GetMember(instance, my_method);
            return op.Invoke(method);
        }

        // args[0] is script name, the rest are script arguments
        static public void RunScriptWithArgs(string[] args)
        {
            // C# args does not include program name itself; convenient!
            var options = new Dictionary<string, object>
                {
                    { "Arguments", args },
                };
            
            ScriptEngine engine = Python.CreateEngine(options);
            ScriptSource source = engine.CreateScriptSourceFromFile(args[0]);

            var paths = engine.GetSearchPaths();
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            paths.Add(path);
            engine.SetSearchPaths(paths);

            source.ExecuteProgram();
        }
    }
}
