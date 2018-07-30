using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeGenerator
{
    class Program
    {
        static void GenerateNestedSelectors(List<string> lines, int totalDepth, int currentDepth = 1)
        {
            var nextDepth = currentDepth + 1;
            
            if (totalDepth > currentDepth)
            {
                lines.Add($"if (matcher{nextDepth}.Matches(entityData.Flags)){{");
                lines.Add($"int indexInMatcher{nextDepth} = matcher{nextDepth}.TryGetIndexFromKey(entIdx);");
                lines.Add($"if (indexInMatcher{nextDepth} >= 0){{");
                lines.Add($"ref T{nextDepth} component{nextDepth} = ref matcher{nextDepth}Buffers.data[indexInMatcher{nextDepth}];");

                if(currentDepth+1 < totalDepth)
                    GenerateNestedSelectors(lines, totalDepth, ++currentDepth);
            }
            if (totalDepth == 1 || nextDepth == totalDepth)
            {
                var comps = new List<string>{"ref component"};
                for (int i = 2; i < totalDepth+1; i++)
                    comps.Add("ref component"+i);
                lines.Add($"loopAction(entIdx, {string.Join(", ", comps)});");
            }
            if (totalDepth > currentDepth)
            {
                lines.Add($"}}//end if indexInMatcher{nextDepth}");
            }
            if (totalDepth > currentDepth)
                lines.Add($"}}//end if matcher{nextDepth}.Matches");
            //lines.Add("}");
        }

        static void Main(string[] args)
        {
            int depth = 18;

            //generate loops code since there's no variadic templates / generics :(
            var dels = new List<string>();
            var defs = new List<string>();
            for (int i = 2; i < depth; i++)
            {
                var typeList = new List<string>();
                var parList = new List<string>();

                for (int j = 1; j < i; j++)
                {
                    typeList.Add($"T{j}");
                    parList.Add($"ref T{j} component{j}"); //todo linq select
                }

                string del = "public delegate void ProcessComponent<";
                del += String.Join(", ", typeList);
                del += ">(int entIdx, ";
                del += String.Join(", ", parList);
                del += ");";
                dels.Add(del);

                string def = "public void Loop<";
                def += String.Join(", ", typeList);
                def += ">(ProcessComponent<";
                def += String.Join(", ", typeList);
                def += "> loopAction)\r\n";
                def += String.Join(" ", typeList.Select(x => "where "+x+" : struct"))+"\r\n";
                def += "{\r\n";

                def += "var componentBuffer = GetComponentBufferFromComponentType<T1>();\r\n" +
                       "var buffers = componentBuffer.__GetBuffers();\r\n" +
                       "var entIdxs = buffers.keys;\r\n" +
                       "var components = buffers.data;\r\n";
                
                var nestedLines = new List<string>();

                for (int j = 2; j < i; j++)
                {
                    nestedLines.Add($"var matcher{j} = GetComponentBufferFromComponentType<T{j}>();");
                    nestedLines.Add($"var matcher{j}Buffers = matcher{j}.__GetBuffers();");
                }


                nestedLines.Add("for (var i = components.Length - 1; i >= 0; i--){");

                nestedLines.Add($"ref T1 component = ref components[i];");
                nestedLines.Add($"int entIdx = entIdxs[i];");
                nestedLines.Add($"ref EntityData entityData = ref GetDataFromIndex(entIdx);");

                GenerateNestedSelectors(nestedLines, i-1);

                nestedLines.Add("}//end for components");
                nestedLines.Add("}//end function");

                defs.Add(def);
                defs.AddRange(nestedLines);
            }








            var file = new List<string>();
            
            file.Add("partial class EntityRegistry\r\n{");
            
            file.AddRange(dels);
            file.AddRange(defs);

            file.Add("\r\n}");

            File.WriteAllLines("../../../../minECS/EntityRegistryGenerated.cs", file);


            
        }
    }
}
