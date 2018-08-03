using System.Collections.Generic;
using System.Linq;
partial class EntityRegistry
{
public delegate void ProcessComponent<T1>(int entIdx, ref T1 component1);
public delegate void ProcessComponent<T1, T2>(int entIdx, ref T1 component1, ref T2 component2);
public void Loop<T1>(ProcessComponent<T1> loopAction)
where T1 : struct
{
  List<ComponentBufferBase> denseBuffers = new List<ComponentBufferBase>();
  List<ComponentBufferBase> sparseBuffers = new List<ComponentBufferBase>();
  var t1Base = componentsManager_.GetBufferSlow<T1>();
  if (t1Base.Sparse) sparseBuffers.Add(t1Base);
  else denseBuffers.Add(t1Base);
  var denseBuffersSorted = denseBuffers.OrderBy(x => x.ComponentCount).ToArray();
  var sparseBuffersSorted = sparseBuffers.OrderBy(x => x.ComponentCount).ToArray();
  int[] sortMapDense = MiscUtils.GetSortMap(denseBuffers, denseBuffersSorted);
  int[] sortMapSparse = MiscUtils.GetSortMap(sparseBuffers, sparseBuffersSorted);
  int denseCount = sortMapDense.Length;
  int sparseCount = sortMapSparse.Length;
  if (denseCount == 0 && sparseCount == 1){
    if (sortMapDense.SequenceEqual(new[] { 0, 1 }))
      Loop01Dense0Sparse1(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0]);
  }
  if (denseCount == 1 && sparseCount == 0){
    if (sortMapDense.SequenceEqual(new[] { 0, 1 }))
      Loop01Dense1Sparse0(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0]);
  }
}

public void Loop<T1, T2>(ProcessComponent<T1, T2> loopAction)
where T1 : struct where T2 : struct
{
  List<ComponentBufferBase> denseBuffers = new List<ComponentBufferBase>();
  List<ComponentBufferBase> sparseBuffers = new List<ComponentBufferBase>();
  var t1Base = componentsManager_.GetBufferSlow<T1>();
  if (t1Base.Sparse) sparseBuffers.Add(t1Base);
  else denseBuffers.Add(t1Base);
  var t2Base = componentsManager_.GetBufferSlow<T2>();
  if (t2Base.Sparse) sparseBuffers.Add(t2Base);
  else denseBuffers.Add(t2Base);
  var denseBuffersSorted = denseBuffers.OrderBy(x => x.ComponentCount).ToArray();
  var sparseBuffersSorted = sparseBuffers.OrderBy(x => x.ComponentCount).ToArray();
  int[] sortMapDense = MiscUtils.GetSortMap(denseBuffers, denseBuffersSorted);
  int[] sortMapSparse = MiscUtils.GetSortMap(sparseBuffers, sparseBuffersSorted);
  int denseCount = sortMapDense.Length;
  int sparseCount = sortMapSparse.Length;
  if (denseCount == 0 && sparseCount == 2){
    if (sortMapDense.SequenceEqual(new[] { 0, 1 }))
      Loop01Dense0Sparse2(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1]);
  }
  if (denseCount == 1 && sparseCount == 1){
    if (sortMapDense.SequenceEqual(new[] { 0, 1 }))
      Loop01Dense1Sparse1(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1]);
  }
  if (denseCount == 2 && sparseCount == 0){
    if (sortMapDense.SequenceEqual(new[] { 0, 1 }))
      Loop01Dense2Sparse0(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1]);
  }
}


}
