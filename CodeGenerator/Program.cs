using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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


        static List<List<T>> GetPermutations<T>(List<T> list, int length)
        {
            if (length == 1) return list.Select(t => new List<T> { t }).ToList();
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new List<T> { t2 }).ToList()).ToList();
        }

        static List<List<int>> GetAllPermutationsForRange(int range)
        {
            List<int> ints = Enumerable.Range(0, range).ToList();
            return GetPermutations(ints, range);
        }

        static void Main(string[] args)
        {
            int depth = 3;

            var dels = new List<string>();
            var defs = new List<string>();
            for (int i = 2; i < depth+2; i++)
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

                StringBuilder sb = new StringBuilder();

                sb.Append("public void Loop<");
                sb.Append(String.Join(", ", typeList));
                sb.Append(">(ProcessComponent<");
                sb.Append(String.Join(", ", typeList));
                sb.Append("> loopAction)\r\n");
                sb.Append(String.Join(" ", typeList.Select(x => "where "+x+" : struct"))+"\r\n");
                sb.Append("{\r\n");
                
                sb.AppendLine("  List<ComponentBufferBase> denseBuffers = new List<ComponentBufferBase>();");
                sb.AppendLine("  List<ComponentBufferBase> sparseBuffers = new List<ComponentBufferBase>();");

                for (int j = 1; j < i; j++)
                {
                    sb.AppendLine($"  var t{j}Base = componentsManager_.GetBufferSlow<T{j}>();");
                    sb.AppendLine($"  if (t{j}Base.Sparse) sparseBuffers.Add(t{j}Base);");
                    sb.AppendLine($"  else denseBuffers.Add(t{j}Base);");
                }

                sb.AppendLine("  var denseBuffersSorted = denseBuffers.OrderBy(x => x.ComponentCount).ToArray();");
                sb.AppendLine("  var sparseBuffersSorted = sparseBuffers.OrderBy(x => x.ComponentCount).ToArray();");
                sb.AppendLine("  int[] sortMapDense = MiscUtils.GetSortMap(denseBuffers, denseBuffersSorted);");
                sb.AppendLine("  int[] sortMapSparse = MiscUtils.GetSortMap(sparseBuffers, sparseBuffersSorted);");
                sb.AppendLine("  int denseCount = sortMapDense.Length;");
                sb.AppendLine("  int sparseCount = sortMapSparse.Length;");



                string ifelse = "";
                for (int j = 0; j < i; j++)
                {
                    int dense = j;
                    int sparse = (i-1)-dense;

                    sb.AppendLine($"  {ifelse}if (denseCount == {dense} && sparseCount == {sparse}){{");
                    ifelse = "else ";

                    var permutations = GetAllPermutationsForRange(i-1);
                    string ifelse2 = "";
                    foreach (List<int> permutation in permutations)
                    {
                        sb.AppendLine($"    {ifelse2}if (sortMapDense.SequenceEqual(new[] {{ {string.Join(',',permutation)} }}))");
                        sb.AppendLine($"      Loop{string.Join("", permutation)}Dense{dense}Sparse{sparse}(loopAction,");
                        ifelse2 = "else ";

                        for (int k = 1; k < i; k++)
                        {
                            string type = k > dense ? "Sparse" : "Dense";
                            sb.AppendLine($"        (ComponentBuffer{type}<T{permutation[k-1]+1}>)denseBuffersSorted[{k-1}]"+
                                          ((k != i - 1) ? "," : ");"));
                        }
                    }

                    sb.AppendLine($"  }}");
                }






                sb.AppendLine("}");



//                var nestedLines = new List<string>();
//                
//                nestedLines.Add("for (var i = components.Length - 1; i >= 0; i--){");
//
//                nestedLines.Add($"ref T1 component = ref components[i];");
//                nestedLines.Add($"int entIdx = entIdxs[i];");
//                nestedLines.Add($"ref EntityData entityData = ref GetDataFromIndex(entIdx);");
//
//                GenerateNestedSelectors(nestedLines, i-1);
//
//                nestedLines.Add("}//end for components");
//                nestedLines.Add("}//end function");

                defs.Add(sb.ToString());
//                defs.AddRange(nestedLines);
            }








            var file = new List<string>();
            
            file.Add("using System.Collections.Generic;\r\n" +
                     "using System.Linq;\r\n" +
                     "partial class EntityRegistry\r\n{");
            
            file.AddRange(dels);
            file.AddRange(defs);

            file.Add("\r\n}");

            File.WriteAllLines("../../../../minECS/EntityRegistryGenerated.cs", file);


            
        }
    }
}
