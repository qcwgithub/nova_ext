

// https://www.nuget.org/packages/Microsoft.Diagnostics.Runtime
// https://github.com/Microsoft/clrmd/blob/master/src/FileAndLineNumbers/Program.cs


using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using Microsoft.Diagnostics.Runtime.Utilities.Pdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugTool
{
    static class Extensions
    {
        static Dictionary<PdbInfo, PdbReader> s_pdbReaders = new Dictionary<PdbInfo, PdbReader>();

//         public static void ClearPdbs()
//         {
//             s_pdbReaders.Clear();
//         }
        public static FileAndLineNumber GetSourceLocation(this ClrStackFrame frame)
        {
            PdbReader reader = GetReaderForFrame(frame);
            if (reader == null)
                return new FileAndLineNumber();

            PdbFunction function = reader.GetFunctionFromToken(frame.Method.MetadataToken);
            int ilOffset = FindIlOffset(frame);

            return FindNearestLine(function, ilOffset);
        }

        private static FileAndLineNumber FindNearestLine(PdbFunction function, int ilOffset)
        {
            int distance = int.MaxValue;
            FileAndLineNumber nearest = new FileAndLineNumber();

            foreach (PdbSequencePointCollection sequenceCollection in function.SequencePoints)
            {
                foreach (PdbSequencePoint point in sequenceCollection.Lines)
                {
                    int dist = (int)Math.Abs(point.Offset - ilOffset);
                    if (dist < distance)
                    {
                        nearest.File = sequenceCollection.File.Name;
                        nearest.Line = (int)point.LineBegin;
                    }
                }
            }

            return nearest;
        }

        private static int FindIlOffset(ClrStackFrame frame)
        {
            ulong ip = frame.InstructionPointer;
            int last = -1;
            foreach (ILToNativeMap item in frame.Method.ILOffsetMap)
            {
                if (item.StartAddress > ip)
                    return last;

                if (ip <= item.EndAddress)
                    return item.ILOffset;

                last = item.ILOffset;
            }

            return last;
        }

        public static Func<string, string> FunTryFindPdbPath=null;

        private static PdbReader GetReaderForFrame(ClrStackFrame frame)
        {
            ClrModule module = frame.Method?.Type?.Module;
            PdbInfo info = module?.Pdb;

            PdbReader reader = null;
            if (info != null)
            {
                if (!s_pdbReaders.TryGetValue(info, out reader))
                {
                    SymbolLocator locator = GetSymbolLocator(module);
                    string pdbPath = locator.FindPdb(info);
                    if (pdbPath == null)
                    {
                        if (FunTryFindPdbPath != null)
                        {
                            pdbPath = FunTryFindPdbPath(info.FileName);
                            if (pdbPath != null)
                                Console.WriteLine("pdb found(by redirection): " + pdbPath);
                        }
                    }
                    if (pdbPath == null)
                        Console.WriteLine("pdb not found: " + info.FileName);
                    if (pdbPath != null)
                    {
                        bool ok = true;
                        try
                        {
                            reader = new PdbReader(pdbPath); }
                        catch
                        {
                            ok = false;
                            reader = null;
                        }

                        if (ok)
                            Console.WriteLine("pdb load ok: " + pdbPath);
                        else
                            Console.WriteLine("pdb load fail: " + pdbPath);
                    }
                    s_pdbReaders[info] = reader;
                }
            }

            return reader;
        }

        private static SymbolLocator GetSymbolLocator(ClrModule module)
        {
            return module.Runtime.DataTarget.SymbolLocator;
        }
    }
}
