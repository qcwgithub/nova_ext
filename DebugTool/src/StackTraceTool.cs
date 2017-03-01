using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugTool
{
    class StackTraceTool
    {
        public static Dictionary<int, string[]> GetStackTraces(Process pro)
        {
            string exePath = pro.MainModule.FileName.Replace('/','\\');
            Extensions.FunTryFindPdbPath = (string pdbPath) =>
            {
                pdbPath = pdbPath.Replace('/', '\\');
                int i = pdbPath.LastIndexOf('\\');
                if (i < 0)
                    return null;
                string name = pdbPath.Substring(i+1);

                i = exePath.LastIndexOf('\\');
                if (i < 0)
                    return null;

                string re = exePath.Substring(0, i + 1) + name;

                if (System.IO.File.Exists(re))
                {
                    Console.WriteLine(string.Format("{0} -> {1}", pdbPath, re));
                    return re;
                }
                else
                    return null;
            };
            var result = new Dictionary<int, string[]>();

            using (DataTarget dataTarget = DataTarget.AttachToProcess(pro.Id, 5000, AttachFlag.NonInvasive))
            {
                try
                {
                    ClrRuntime runtime = dataTarget.ClrVersions.First().CreateRuntime();
                    foreach (ClrThread thread in runtime.Threads)
                    {
                        if (thread.StackTrace != null)
                        {
                            var stacks = thread.StackTrace.Select((frame) =>
                            {
                                if (frame.Kind == ClrStackFrameType.Runtime)
                                {
                                    return string.Format("{0,12:x} {1,12:x} {2}", frame.InstructionPointer, frame.StackPointer, frame.DisplayString);
                                }
                                else
                                {
                                    FileAndLineNumber info = frame.GetSourceLocation();
                                    return string.Format("{0,12:x} {1,12:x} {2} [{3} @ {4}]", frame.InstructionPointer, frame.StackPointer, frame.DisplayString, info.File, info.Line);
                                }
                            }).ToArray();

                            result.Add(thread.ManagedThreadId, stacks);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("exception occurred");
                }
            }

            return result;
        }
    }
}
