using System.Collections.Generic;
using System.Linq;
partial class EntityRegistry
{
public delegate void ProcessComponent<T1>(int entIdx, ref T1 component1);
public delegate void ProcessComponent<T1, T2>(int entIdx, ref T1 component1, ref T2 component2);
public delegate void ProcessComponent<T1, T2, T3>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3);
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
    if (sortMapDense.SequenceEqual(new[] { 0 }))
      Loop0Dense0Sparse1(loopAction,
        (ComponentBufferSparse<T1>)denseBuffersSorted[0]);
  }
  else if (denseCount == 1 && sparseCount == 0){
    if (sortMapDense.SequenceEqual(new[] { 0 }))
      Loop0Dense1Sparse0(loopAction,
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
    if (sortMapDense.SequenceEqual(new[] { 0,1 }))
      Loop01Dense0Sparse2(loopAction,
        (ComponentBufferSparse<T1>)denseBuffersSorted[0],
        (ComponentBufferSparse<T2>)denseBuffersSorted[1]);
    else if (sortMapDense.SequenceEqual(new[] { 1,0 }))
      Loop10Dense0Sparse2(loopAction,
        (ComponentBufferSparse<T2>)denseBuffersSorted[0],
        (ComponentBufferSparse<T1>)denseBuffersSorted[1]);
  }
  else if (denseCount == 1 && sparseCount == 1){
    if (sortMapDense.SequenceEqual(new[] { 0,1 }))
      Loop01Dense1Sparse1(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferSparse<T2>)denseBuffersSorted[1]);
    else if (sortMapDense.SequenceEqual(new[] { 1,0 }))
      Loop10Dense1Sparse1(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferSparse<T1>)denseBuffersSorted[1]);
  }
  else if (denseCount == 2 && sparseCount == 0){
    if (sortMapDense.SequenceEqual(new[] { 0,1 }))
      Loop01Dense2Sparse0(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1]);
    else if (sortMapDense.SequenceEqual(new[] { 1,0 }))
      Loop10Dense2Sparse0(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferDense<T1>)denseBuffersSorted[1]);
  }
}

public void Loop<T1, T2, T3>(ProcessComponent<T1, T2, T3> loopAction)
where T1 : struct where T2 : struct where T3 : struct
{
  List<ComponentBufferBase> denseBuffers = new List<ComponentBufferBase>();
  List<ComponentBufferBase> sparseBuffers = new List<ComponentBufferBase>();
  var t1Base = componentsManager_.GetBufferSlow<T1>();
  if (t1Base.Sparse) sparseBuffers.Add(t1Base);
  else denseBuffers.Add(t1Base);
  var t2Base = componentsManager_.GetBufferSlow<T2>();
  if (t2Base.Sparse) sparseBuffers.Add(t2Base);
  else denseBuffers.Add(t2Base);
  var t3Base = componentsManager_.GetBufferSlow<T3>();
  if (t3Base.Sparse) sparseBuffers.Add(t3Base);
  else denseBuffers.Add(t3Base);
  var denseBuffersSorted = denseBuffers.OrderBy(x => x.ComponentCount).ToArray();
  var sparseBuffersSorted = sparseBuffers.OrderBy(x => x.ComponentCount).ToArray();
  int[] sortMapDense = MiscUtils.GetSortMap(denseBuffers, denseBuffersSorted);
  int[] sortMapSparse = MiscUtils.GetSortMap(sparseBuffers, sparseBuffersSorted);
  int denseCount = sortMapDense.Length;
  int sparseCount = sortMapSparse.Length;
  if (denseCount == 0 && sparseCount == 3){
    if (sortMapDense.SequenceEqual(new[] { 0,1,2 }))
      Loop012Dense0Sparse3(loopAction,
        (ComponentBufferSparse<T1>)denseBuffersSorted[0],
        (ComponentBufferSparse<T2>)denseBuffersSorted[1],
        (ComponentBufferSparse<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 0,2,1 }))
      Loop021Dense0Sparse3(loopAction,
        (ComponentBufferSparse<T1>)denseBuffersSorted[0],
        (ComponentBufferSparse<T3>)denseBuffersSorted[1],
        (ComponentBufferSparse<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,0,2 }))
      Loop102Dense0Sparse3(loopAction,
        (ComponentBufferSparse<T2>)denseBuffersSorted[0],
        (ComponentBufferSparse<T1>)denseBuffersSorted[1],
        (ComponentBufferSparse<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,2,0 }))
      Loop120Dense0Sparse3(loopAction,
        (ComponentBufferSparse<T2>)denseBuffersSorted[0],
        (ComponentBufferSparse<T3>)denseBuffersSorted[1],
        (ComponentBufferSparse<T1>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,0,1 }))
      Loop201Dense0Sparse3(loopAction,
        (ComponentBufferSparse<T3>)denseBuffersSorted[0],
        (ComponentBufferSparse<T1>)denseBuffersSorted[1],
        (ComponentBufferSparse<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,1,0 }))
      Loop210Dense0Sparse3(loopAction,
        (ComponentBufferSparse<T3>)denseBuffersSorted[0],
        (ComponentBufferSparse<T2>)denseBuffersSorted[1],
        (ComponentBufferSparse<T1>)denseBuffersSorted[2]);
  }
  else if (denseCount == 1 && sparseCount == 2){
    if (sortMapDense.SequenceEqual(new[] { 0,1,2 }))
      Loop012Dense1Sparse2(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferSparse<T2>)denseBuffersSorted[1],
        (ComponentBufferSparse<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 0,2,1 }))
      Loop021Dense1Sparse2(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferSparse<T3>)denseBuffersSorted[1],
        (ComponentBufferSparse<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,0,2 }))
      Loop102Dense1Sparse2(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferSparse<T1>)denseBuffersSorted[1],
        (ComponentBufferSparse<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,2,0 }))
      Loop120Dense1Sparse2(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferSparse<T3>)denseBuffersSorted[1],
        (ComponentBufferSparse<T1>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,0,1 }))
      Loop201Dense1Sparse2(loopAction,
        (ComponentBufferDense<T3>)denseBuffersSorted[0],
        (ComponentBufferSparse<T1>)denseBuffersSorted[1],
        (ComponentBufferSparse<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,1,0 }))
      Loop210Dense1Sparse2(loopAction,
        (ComponentBufferDense<T3>)denseBuffersSorted[0],
        (ComponentBufferSparse<T2>)denseBuffersSorted[1],
        (ComponentBufferSparse<T1>)denseBuffersSorted[2]);
  }
  else if (denseCount == 2 && sparseCount == 1){
    if (sortMapDense.SequenceEqual(new[] { 0,1,2 }))
      Loop012Dense2Sparse1(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1],
        (ComponentBufferSparse<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 0,2,1 }))
      Loop021Dense2Sparse1(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T3>)denseBuffersSorted[1],
        (ComponentBufferSparse<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,0,2 }))
      Loop102Dense2Sparse1(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferDense<T1>)denseBuffersSorted[1],
        (ComponentBufferSparse<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,2,0 }))
      Loop120Dense2Sparse1(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferDense<T3>)denseBuffersSorted[1],
        (ComponentBufferSparse<T1>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,0,1 }))
      Loop201Dense2Sparse1(loopAction,
        (ComponentBufferDense<T3>)denseBuffersSorted[0],
        (ComponentBufferDense<T1>)denseBuffersSorted[1],
        (ComponentBufferSparse<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,1,0 }))
      Loop210Dense2Sparse1(loopAction,
        (ComponentBufferDense<T3>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1],
        (ComponentBufferSparse<T1>)denseBuffersSorted[2]);
  }
  else if (denseCount == 3 && sparseCount == 0){
    if (sortMapDense.SequenceEqual(new[] { 0,1,2 }))
      Loop012Dense3Sparse0(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1],
        (ComponentBufferDense<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 0,2,1 }))
      Loop021Dense3Sparse0(loopAction,
        (ComponentBufferDense<T1>)denseBuffersSorted[0],
        (ComponentBufferDense<T3>)denseBuffersSorted[1],
        (ComponentBufferDense<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,0,2 }))
      Loop102Dense3Sparse0(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferDense<T1>)denseBuffersSorted[1],
        (ComponentBufferDense<T3>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 1,2,0 }))
      Loop120Dense3Sparse0(loopAction,
        (ComponentBufferDense<T2>)denseBuffersSorted[0],
        (ComponentBufferDense<T3>)denseBuffersSorted[1],
        (ComponentBufferDense<T1>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,0,1 }))
      Loop201Dense3Sparse0(loopAction,
        (ComponentBufferDense<T3>)denseBuffersSorted[0],
        (ComponentBufferDense<T1>)denseBuffersSorted[1],
        (ComponentBufferDense<T2>)denseBuffersSorted[2]);
    else if (sortMapDense.SequenceEqual(new[] { 2,1,0 }))
      Loop210Dense3Sparse0(loopAction,
        (ComponentBufferDense<T3>)denseBuffersSorted[0],
        (ComponentBufferDense<T2>)denseBuffersSorted[1],
        (ComponentBufferDense<T1>)denseBuffersSorted[2]);
  }
}


}
