using Mono.Cecil;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CecilLoadType
{
    class Program
    {
        static bool TypeIsSubclassOfMonoBehaviour(TypeDefinition type)
        {
            if (type.FullName == "UnityEngine.MonoBehaviour")
                return false;

            try
            {
                TypeDefinition t = type;
                while (t != null)
                {
                    if (t.BaseType != null && t.BaseType.FullName == "UnityEngine.MonoBehaviour")
                        return true;
                    if (t.BaseType == null)
                        return false;
                    t = t.BaseType.Resolve();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return false;
        }

        static bool FieldTypeIsSupported(TypeReference fieldType)
        {
            return fieldType.FullName == "System.Int32" ||
                    fieldType.FullName == "System.String" ||
                    fieldType.FullName == "System.Boolean" ||
                    //fieldType.FullName == "UnityEngine.GameObject";
                    fieldType.FullName.StartsWith("UnityEngine.");
        }

        static bool CheckFieldAttr(FieldDefinition field)
        {
            if (0 != (field.Attributes & FieldAttributes.NotSerialized))
            {
                return false;
            }

            if (field.HasCustomAttributes)
            {
                foreach (var attr in field.CustomAttributes)
                {
                    if (attr.GetType().Name == "NonSerialized" ||
                        attr.GetType().Name == "HideInInspector")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        class X
        {
            public string fieldName;
            public string fieldTypeName;
        }
        static void GetTypeOkFields(TypeDefinition type, List<X> lst)
        {
            foreach (var field in type.Fields)
            {
                if (!field.IsPublic || field.HasConstant || field.IsInitOnly || field.IsStatic)
                {
                    continue;
                }

                if (!FieldTypeIsSupported(field.FieldType))
                {
                    continue;
                }
                if (!CheckFieldAttr(field))
                {
                    continue;
                }
                lst.Add(new X{ fieldName = field.Name, fieldTypeName = field.FieldType.FullName });
            }
        }

        static void FindTypePublicVars_impl(TypeDefinition type, string outputPath)
        {
            if (!TypeIsSubclassOfMonoBehaviour(type))
            {
                Console.WriteLine("type is not a MonoBehaviour");
                return;
            }

            List<TypeDefinition> lstTypes = new List<TypeDefinition>();
            TypeDefinition t = type;
            while (t != null)
            {
                lstTypes.Add(t);

                if (t.BaseType == null || 
                    t.BaseType.FullName == "UnityEngine.MonoBehaviour")
                {
                    break;
                }
                t = t.BaseType.Resolve();
            }

            List<X> lst = new List<X>();
            for (int i = lstTypes.Count - 1; i >= 0; i--)
            {
                GetTypeOkFields(lstTypes[i], lst);
            }

            var sb = new StringBuilder();
            for (int i = 0; i < lst.Count; i++)
            {
                sb.AppendFormat("{0} {1}\n", lst[i].fieldTypeName, lst[i].fieldName);
            }

            Console.Write(sb.ToString());
            System.IO.File.WriteAllText(outputPath, sb.ToString());
        }

        static void FindTypePublicVars(string dllPath, string typeName, string outputPath)
        {
            //Creates an AssemblyDefinition from the "MyLibrary.dll" assembly
            AssemblyDefinition myLibrary = AssemblyDefinition.ReadAssembly(dllPath);

            //Gets all types which are declared in the Main Module of "MyLibrary.dll"
            foreach (TypeDefinition type in myLibrary.MainModule.Types)
            {
                if (type.FullName == typeName)
                {
                    FindTypePublicVars_impl(type, outputPath);
                }
            }
        }

        static void CheckTypeClickMethod_impl(TypeDefinition type, string methodName, string outputPath)
        {
            if (!TypeIsSubclassOfMonoBehaviour(type))
            {
                Console.WriteLine("type is not a MonoBehaviour");
                return;
            }

            foreach (var method in type.Methods)
            {
                if (method.Name == methodName && method.IsPublic && !method.IsStatic && method.Parameters.Count == 0)
                {
                    System.IO.File.WriteAllText(outputPath, "1");
                    return;
                }
            }
        }

        static void CheckTypeClickMethod(string dllPath, string typeName, string methodName, string outputPath)
        {
            //Creates an AssemblyDefinition from the "MyLibrary.dll" assembly
            AssemblyDefinition myLibrary = AssemblyDefinition.ReadAssembly(dllPath);

            //Gets all types which are declared in the Main Module of "MyLibrary.dll"
            foreach (TypeDefinition type in myLibrary.MainModule.Types)
            {
                if (type.FullName == typeName)
                {
                    CheckTypeClickMethod_impl(type, methodName, outputPath);
                }
            }
        }

        static List<char> temp = new List<char>();
        static string n2GTs(int n)
        {
            temp.Clear();
            for (int i=0;i<n;i++)
            {
                temp.Add((char)((int)'A' + i));
            }
            return string.Join(",", temp.Select(_c => _c.ToString()).ToArray());
        }
        static void FindAllBridgeTypes(string dllPath, string outputPath)
        {
            //Creates an AssemblyDefinition from the "MyLibrary.dll" assembly
            AssemblyDefinition myLibrary = AssemblyDefinition.ReadAssembly(dllPath);

            Dictionary<string, List<string>> d = new Dictionary<string, List<string>>();
            //Gets all types which are declared in the Main Module of "MyLibrary.dll"
            foreach (TypeDefinition type in myLibrary.MainModule.Types)
            {
                string ns = type.Namespace;
                if (string.IsNullOrEmpty(ns) || (ns!="System"&&!ns.StartsWith("System.")))
                    continue;
                if (!d.ContainsKey(ns))
                    d.Add(ns, new List<string>());

                string fn = type.FullName.Substring(ns.Length+1);
                if (fn.Contains('`'))
                {
                    int i=fn.LastIndexOf('`');
                    string cstr = fn.Substring(i + 1);
                    int c = int.Parse(cstr);
                    fn = fn.Substring(0, i) + "<" + n2GTs(c) + ">";
                }

                d[ns].Add(fn);
            }

            StringBuilder sb = new StringBuilder();
            foreach(var kv in d)
            {
                sb.AppendLine("[" + kv.Key + "]");

                kv.Value.Sort((_a, _b) => string.Compare(_a, _b));

                foreach(var fn in kv.Value)
                    sb.AppendLine(fn);
                sb.AppendLine();
            }

            System.IO.File.WriteAllText(outputPath, sb.ToString());
        }

        static void Main(string[] args)
        {
            string op = args[0];
            if (op == "FindTypePublicVars")
            {
                FindTypePublicVars(args[1], args[2], args[3]);
            }
            else if (op == "CheckTypePublicMethod")
            {
                CheckTypeClickMethod(args[1], args[2], args[3], args[4]);
            }
            else if (op == "FindAllBridgeTypes")
            {
                FindAllBridgeTypes(args[1], args[2]);
            }
        }
    }
}
