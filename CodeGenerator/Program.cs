using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGenerator
{
    class Program
    {
        static void GenerateNestedSelectors(StringBuilder sb, int totalDepth, uint bits, int currentDepth = 1)
        {
            var nextDepth = currentDepth + 1;
            
            if (totalDepth > currentDepth && nextDepth != totalDepth)
            {
                string type = (bits & (1 << currentDepth)) == 0 ? "Dense" : "Sparse";
                sb.AppendLine($"  if ((entityData.Flags{type} & matcher{currentDepth}Flag) != 0){{");
                sb.AppendLine($"    int indexInMatcher{currentDepth} = matcher{currentDepth}Buffers.entIdx2i[entIdx];");
                sb.AppendLine($"    ref T{currentDepth} component{currentDepth} = ref matcher{currentDepth}Buffers.data[indexInMatcher{currentDepth}];");

                if(currentDepth+1 < totalDepth)
                    GenerateNestedSelectors(sb, totalDepth, bits, ++currentDepth);
            }
            if (totalDepth == 1 || nextDepth == totalDepth)
            {
                var comps = new List<string>();
                for (int i = 0; i < totalDepth-1; i++)
                    comps.Add("    ref component"+i);
                sb.AppendLine($"     loopAction(entIdx, {string.Join(", ", comps)});");
            }

            if (totalDepth > currentDepth && nextDepth != totalDepth)
            {
                sb.AppendLine($"}}//end if flags test {currentDepth}");
            }
        }

        static void Main(string[] args)
        {
            int depth = 4;

            var dels = new List<string>();
            var defs = new List<string>();
            for (int i = 2; i < depth+2; i++)
            {
                var typeList = new List<string>();
                var parList = new List<string>();

                for (int j = 0; j < i-1; j++)
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

                StringBuilder sb = new StringBuilder();

                sb.Append("public void Loop<");
                sb.Append(String.Join(", ", typeList));
                sb.Append(">(ProcessComponent<");
                sb.Append(String.Join(", ", typeList));
                sb.Append("> loopAction)\r\n");
                sb.Append(String.Join(" ", typeList.Select(x => "where "+x+" : struct"))+"\r\n");
                sb.Append("{\r\n");
                
                sb.AppendLine("  ushort typeMask = 0;");

                for (int j = 0; j < i-1; j++)
                {
                    sb.AppendLine($"  var t{j}Base = componentsManager_.GetBufferSlow<T{j}>();");
                    sb.AppendLine($"  if (t{j}Base.Sparse) typeMask |= 1 << {j};");
                }

                sb.AppendLine("  switch (typeMask) {");

                uint bits = 0;

                var fsb = new StringBuilder();

                while ((bits & (1 << i-1)) == 0)
                {
                    string binrepr = Convert.ToString(bits, 2).PadLeft(i-1, '0');

                    sb.AppendLine($"    case 0b{binrepr}:");
                    sb.AppendLine($"      Loop{binrepr}(loopAction,");

                    fsb.AppendLine($"private void Loop{binrepr}<{String.Join(", ", typeList)}>(");
                    fsb.AppendLine($"ProcessComponent<{String.Join(", ", typeList)}> loopAction,");

                    for (int j = 0; j < i - 1; j++)
                    {
                        bool dense = (bits & (1 << j)) == 0;
                        string type = dense ? "Dense" : "Sparse";
                        sb.AppendLine($"        (ComponentBuffer{type}<T{j}>)t{j}Base" + ( j == i-2 ? "" : ","));

                        fsb.AppendLine($"ComponentBuffer{type}<T{j}> t{j}B" + (j == i - 2 ? ")" : ","));
                    }

                    sb.AppendLine($"      ); return;");

                    for (int j = 0; j < i - 1; j++)
                    {
                        fsb.AppendLine($"where T{j} : struct");
                    }

                    fsb.AppendLine("{");

                    fsb.AppendLine($"  var compBuffers = t0B.__GetBuffers();");
                    fsb.AppendLine($"  var compIdx2EntIdx = compBuffers.i2EntIdx;");
                    fsb.AppendLine($"  var components = compBuffers.data;");

                    for (int j = 1; j < i - 1; j++)
                    {
                        fsb.AppendLine($"  var matcher{j}Flag = t{j}B.Matcher.Flag;");
                        fsb.AppendLine($"  var matcher{j}Buffers = t{j}B.__GetBuffers();");
                    }

                    fsb.AppendLine("  for (var i = components.Length - 1; i >= 0; i--) {");
                    fsb.AppendLine("    ref T0 component0 = ref components[i];");
                    fsb.AppendLine("    EntIdx entIdx = compIdx2EntIdx[i];");
                    fsb.AppendLine("    ref EntityData entityData = ref data_[entIdx];");

                    GenerateNestedSelectors(fsb, i, bits);

                    fsb.AppendLine($"}} // for components");
                    fsb.AppendLine($"}} // Loop{binrepr}");

                    bits++;
                }

                sb.AppendLine("  } // end switch (typeMask)");
                sb.AppendLine("} // end function");

                sb.AppendLine(fsb.ToString());

                defs.Add(sb.ToString());

            }








            var file = new List<string>();
            
            file.Add("using System.Collections.Generic;\r\n" +
                     "using System.Linq;\r\n" +
                     "using EntIdx = System.Int32;\r\n" +
                     "partial class EntityRegistry\r\n{");
            
            file.AddRange(dels);
            file.AddRange(defs);

            file.Add("\r\n}");

            File.WriteAllLines("../../../../minECS/EntityRegistryGenerated.cs", file);


            
        }
    }
}
