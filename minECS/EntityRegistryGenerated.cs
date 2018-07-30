partial class EntityRegistry
{
    public delegate void ProcessComponent<T1>(int entIdx, ref T1 component1);
    public delegate void ProcessComponent<T1, T2>(int entIdx, ref T1 component1, ref T2 component2);
    public delegate void ProcessComponent<T1, T2, T3>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3);
    public delegate void ProcessComponent<T1, T2, T3, T4>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10, ref T11 component11);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10, ref T11 component11, ref T12 component12);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10, ref T11 component11, ref T12 component12, ref T13 component13);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10, ref T11 component11, ref T12 component12, ref T13 component13, ref T14 component14);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10, ref T11 component11, ref T12 component12, ref T13 component13, ref T14 component14, ref T15 component15);
    public delegate void ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(int entIdx, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10, ref T11 component11, ref T12 component12, ref T13 component13, ref T14 component14, ref T15 component15, ref T16 component16);
    public void Loop<T1>(ProcessComponent<T1> loopAction)
    where T1 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            loopAction(entIdx, ref component);
        }//end for components
    }//end function
    public void Loop<T1, T2>(ProcessComponent<T1, T2> loopAction)
    where T1 : struct where T2 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    loopAction(entIdx, ref component, ref component2);
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3>(ProcessComponent<T1, T2, T3> loopAction)
    where T1 : struct where T2 : struct where T3 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            loopAction(entIdx, ref component, ref component2, ref component3);
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4>(ProcessComponent<T1, T2, T3, T4> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    loopAction(entIdx, ref component, ref component2, ref component3, ref component4);
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5>(ProcessComponent<T1, T2, T3, T4, T5> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5);
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6>(ProcessComponent<T1, T2, T3, T4, T5, T6> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6);
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7);
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8);
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9);
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        var matcher10 = GetComponentBufferFromComponentType<T10>();
        var matcher10Buffers = matcher10.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            if (matcher10.Matches(entityData.Flags))
                                                                            {
                                                                                int indexInMatcher10 = matcher10.TryGetIndexFromKey(entIdx);
                                                                                if (indexInMatcher10 >= 0)
                                                                                {
                                                                                    ref T10 component10 = ref matcher10Buffers.data[indexInMatcher10];
                                                                                    loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9, ref component10);
                                                                                }//end if indexInMatcher10
                                                                            }//end if matcher10.Matches
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        var matcher10 = GetComponentBufferFromComponentType<T10>();
        var matcher10Buffers = matcher10.__GetBuffers();
        var matcher11 = GetComponentBufferFromComponentType<T11>();
        var matcher11Buffers = matcher11.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            if (matcher10.Matches(entityData.Flags))
                                                                            {
                                                                                int indexInMatcher10 = matcher10.TryGetIndexFromKey(entIdx);
                                                                                if (indexInMatcher10 >= 0)
                                                                                {
                                                                                    ref T10 component10 = ref matcher10Buffers.data[indexInMatcher10];
                                                                                    if (matcher11.Matches(entityData.Flags))
                                                                                    {
                                                                                        int indexInMatcher11 = matcher11.TryGetIndexFromKey(entIdx);
                                                                                        if (indexInMatcher11 >= 0)
                                                                                        {
                                                                                            ref T11 component11 = ref matcher11Buffers.data[indexInMatcher11];
                                                                                            loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9, ref component10, ref component11);
                                                                                        }//end if indexInMatcher11
                                                                                    }//end if matcher11.Matches
                                                                                }//end if indexInMatcher10
                                                                            }//end if matcher10.Matches
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct where T12 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        var matcher10 = GetComponentBufferFromComponentType<T10>();
        var matcher10Buffers = matcher10.__GetBuffers();
        var matcher11 = GetComponentBufferFromComponentType<T11>();
        var matcher11Buffers = matcher11.__GetBuffers();
        var matcher12 = GetComponentBufferFromComponentType<T12>();
        var matcher12Buffers = matcher12.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            if (matcher10.Matches(entityData.Flags))
                                                                            {
                                                                                int indexInMatcher10 = matcher10.TryGetIndexFromKey(entIdx);
                                                                                if (indexInMatcher10 >= 0)
                                                                                {
                                                                                    ref T10 component10 = ref matcher10Buffers.data[indexInMatcher10];
                                                                                    if (matcher11.Matches(entityData.Flags))
                                                                                    {
                                                                                        int indexInMatcher11 = matcher11.TryGetIndexFromKey(entIdx);
                                                                                        if (indexInMatcher11 >= 0)
                                                                                        {
                                                                                            ref T11 component11 = ref matcher11Buffers.data[indexInMatcher11];
                                                                                            if (matcher12.Matches(entityData.Flags))
                                                                                            {
                                                                                                int indexInMatcher12 = matcher12.TryGetIndexFromKey(entIdx);
                                                                                                if (indexInMatcher12 >= 0)
                                                                                                {
                                                                                                    ref T12 component12 = ref matcher12Buffers.data[indexInMatcher12];
                                                                                                    loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9, ref component10, ref component11, ref component12);
                                                                                                }//end if indexInMatcher12
                                                                                            }//end if matcher12.Matches
                                                                                        }//end if indexInMatcher11
                                                                                    }//end if matcher11.Matches
                                                                                }//end if indexInMatcher10
                                                                            }//end if matcher10.Matches
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct where T12 : struct where T13 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        var matcher10 = GetComponentBufferFromComponentType<T10>();
        var matcher10Buffers = matcher10.__GetBuffers();
        var matcher11 = GetComponentBufferFromComponentType<T11>();
        var matcher11Buffers = matcher11.__GetBuffers();
        var matcher12 = GetComponentBufferFromComponentType<T12>();
        var matcher12Buffers = matcher12.__GetBuffers();
        var matcher13 = GetComponentBufferFromComponentType<T13>();
        var matcher13Buffers = matcher13.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            if (matcher10.Matches(entityData.Flags))
                                                                            {
                                                                                int indexInMatcher10 = matcher10.TryGetIndexFromKey(entIdx);
                                                                                if (indexInMatcher10 >= 0)
                                                                                {
                                                                                    ref T10 component10 = ref matcher10Buffers.data[indexInMatcher10];
                                                                                    if (matcher11.Matches(entityData.Flags))
                                                                                    {
                                                                                        int indexInMatcher11 = matcher11.TryGetIndexFromKey(entIdx);
                                                                                        if (indexInMatcher11 >= 0)
                                                                                        {
                                                                                            ref T11 component11 = ref matcher11Buffers.data[indexInMatcher11];
                                                                                            if (matcher12.Matches(entityData.Flags))
                                                                                            {
                                                                                                int indexInMatcher12 = matcher12.TryGetIndexFromKey(entIdx);
                                                                                                if (indexInMatcher12 >= 0)
                                                                                                {
                                                                                                    ref T12 component12 = ref matcher12Buffers.data[indexInMatcher12];
                                                                                                    if (matcher13.Matches(entityData.Flags))
                                                                                                    {
                                                                                                        int indexInMatcher13 = matcher13.TryGetIndexFromKey(entIdx);
                                                                                                        if (indexInMatcher13 >= 0)
                                                                                                        {
                                                                                                            ref T13 component13 = ref matcher13Buffers.data[indexInMatcher13];
                                                                                                            loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9, ref component10, ref component11, ref component12, ref component13);
                                                                                                        }//end if indexInMatcher13
                                                                                                    }//end if matcher13.Matches
                                                                                                }//end if indexInMatcher12
                                                                                            }//end if matcher12.Matches
                                                                                        }//end if indexInMatcher11
                                                                                    }//end if matcher11.Matches
                                                                                }//end if indexInMatcher10
                                                                            }//end if matcher10.Matches
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct where T12 : struct where T13 : struct where T14 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        var matcher10 = GetComponentBufferFromComponentType<T10>();
        var matcher10Buffers = matcher10.__GetBuffers();
        var matcher11 = GetComponentBufferFromComponentType<T11>();
        var matcher11Buffers = matcher11.__GetBuffers();
        var matcher12 = GetComponentBufferFromComponentType<T12>();
        var matcher12Buffers = matcher12.__GetBuffers();
        var matcher13 = GetComponentBufferFromComponentType<T13>();
        var matcher13Buffers = matcher13.__GetBuffers();
        var matcher14 = GetComponentBufferFromComponentType<T14>();
        var matcher14Buffers = matcher14.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            if (matcher10.Matches(entityData.Flags))
                                                                            {
                                                                                int indexInMatcher10 = matcher10.TryGetIndexFromKey(entIdx);
                                                                                if (indexInMatcher10 >= 0)
                                                                                {
                                                                                    ref T10 component10 = ref matcher10Buffers.data[indexInMatcher10];
                                                                                    if (matcher11.Matches(entityData.Flags))
                                                                                    {
                                                                                        int indexInMatcher11 = matcher11.TryGetIndexFromKey(entIdx);
                                                                                        if (indexInMatcher11 >= 0)
                                                                                        {
                                                                                            ref T11 component11 = ref matcher11Buffers.data[indexInMatcher11];
                                                                                            if (matcher12.Matches(entityData.Flags))
                                                                                            {
                                                                                                int indexInMatcher12 = matcher12.TryGetIndexFromKey(entIdx);
                                                                                                if (indexInMatcher12 >= 0)
                                                                                                {
                                                                                                    ref T12 component12 = ref matcher12Buffers.data[indexInMatcher12];
                                                                                                    if (matcher13.Matches(entityData.Flags))
                                                                                                    {
                                                                                                        int indexInMatcher13 = matcher13.TryGetIndexFromKey(entIdx);
                                                                                                        if (indexInMatcher13 >= 0)
                                                                                                        {
                                                                                                            ref T13 component13 = ref matcher13Buffers.data[indexInMatcher13];
                                                                                                            if (matcher14.Matches(entityData.Flags))
                                                                                                            {
                                                                                                                int indexInMatcher14 = matcher14.TryGetIndexFromKey(entIdx);
                                                                                                                if (indexInMatcher14 >= 0)
                                                                                                                {
                                                                                                                    ref T14 component14 = ref matcher14Buffers.data[indexInMatcher14];
                                                                                                                    loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9, ref component10, ref component11, ref component12, ref component13, ref component14);
                                                                                                                }//end if indexInMatcher14
                                                                                                            }//end if matcher14.Matches
                                                                                                        }//end if indexInMatcher13
                                                                                                    }//end if matcher13.Matches
                                                                                                }//end if indexInMatcher12
                                                                                            }//end if matcher12.Matches
                                                                                        }//end if indexInMatcher11
                                                                                    }//end if matcher11.Matches
                                                                                }//end if indexInMatcher10
                                                                            }//end if matcher10.Matches
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct where T12 : struct where T13 : struct where T14 : struct where T15 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        var matcher10 = GetComponentBufferFromComponentType<T10>();
        var matcher10Buffers = matcher10.__GetBuffers();
        var matcher11 = GetComponentBufferFromComponentType<T11>();
        var matcher11Buffers = matcher11.__GetBuffers();
        var matcher12 = GetComponentBufferFromComponentType<T12>();
        var matcher12Buffers = matcher12.__GetBuffers();
        var matcher13 = GetComponentBufferFromComponentType<T13>();
        var matcher13Buffers = matcher13.__GetBuffers();
        var matcher14 = GetComponentBufferFromComponentType<T14>();
        var matcher14Buffers = matcher14.__GetBuffers();
        var matcher15 = GetComponentBufferFromComponentType<T15>();
        var matcher15Buffers = matcher15.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            if (matcher10.Matches(entityData.Flags))
                                                                            {
                                                                                int indexInMatcher10 = matcher10.TryGetIndexFromKey(entIdx);
                                                                                if (indexInMatcher10 >= 0)
                                                                                {
                                                                                    ref T10 component10 = ref matcher10Buffers.data[indexInMatcher10];
                                                                                    if (matcher11.Matches(entityData.Flags))
                                                                                    {
                                                                                        int indexInMatcher11 = matcher11.TryGetIndexFromKey(entIdx);
                                                                                        if (indexInMatcher11 >= 0)
                                                                                        {
                                                                                            ref T11 component11 = ref matcher11Buffers.data[indexInMatcher11];
                                                                                            if (matcher12.Matches(entityData.Flags))
                                                                                            {
                                                                                                int indexInMatcher12 = matcher12.TryGetIndexFromKey(entIdx);
                                                                                                if (indexInMatcher12 >= 0)
                                                                                                {
                                                                                                    ref T12 component12 = ref matcher12Buffers.data[indexInMatcher12];
                                                                                                    if (matcher13.Matches(entityData.Flags))
                                                                                                    {
                                                                                                        int indexInMatcher13 = matcher13.TryGetIndexFromKey(entIdx);
                                                                                                        if (indexInMatcher13 >= 0)
                                                                                                        {
                                                                                                            ref T13 component13 = ref matcher13Buffers.data[indexInMatcher13];
                                                                                                            if (matcher14.Matches(entityData.Flags))
                                                                                                            {
                                                                                                                int indexInMatcher14 = matcher14.TryGetIndexFromKey(entIdx);
                                                                                                                if (indexInMatcher14 >= 0)
                                                                                                                {
                                                                                                                    ref T14 component14 = ref matcher14Buffers.data[indexInMatcher14];
                                                                                                                    if (matcher15.Matches(entityData.Flags))
                                                                                                                    {
                                                                                                                        int indexInMatcher15 = matcher15.TryGetIndexFromKey(entIdx);
                                                                                                                        if (indexInMatcher15 >= 0)
                                                                                                                        {
                                                                                                                            ref T15 component15 = ref matcher15Buffers.data[indexInMatcher15];
                                                                                                                            loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9, ref component10, ref component11, ref component12, ref component13, ref component14, ref component15);
                                                                                                                        }//end if indexInMatcher15
                                                                                                                    }//end if matcher15.Matches
                                                                                                                }//end if indexInMatcher14
                                                                                                            }//end if matcher14.Matches
                                                                                                        }//end if indexInMatcher13
                                                                                                    }//end if matcher13.Matches
                                                                                                }//end if indexInMatcher12
                                                                                            }//end if matcher12.Matches
                                                                                        }//end if indexInMatcher11
                                                                                    }//end if matcher11.Matches
                                                                                }//end if indexInMatcher10
                                                                            }//end if matcher10.Matches
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
    public void Loop<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(ProcessComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> loopAction)
    where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct where T12 : struct where T13 : struct where T14 : struct where T15 : struct where T16 : struct
    {
        var componentBuffer = GetComponentBufferFromComponentType<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBufferFromComponentType<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher3 = GetComponentBufferFromComponentType<T3>();
        var matcher3Buffers = matcher3.__GetBuffers();
        var matcher4 = GetComponentBufferFromComponentType<T4>();
        var matcher4Buffers = matcher4.__GetBuffers();
        var matcher5 = GetComponentBufferFromComponentType<T5>();
        var matcher5Buffers = matcher5.__GetBuffers();
        var matcher6 = GetComponentBufferFromComponentType<T6>();
        var matcher6Buffers = matcher6.__GetBuffers();
        var matcher7 = GetComponentBufferFromComponentType<T7>();
        var matcher7Buffers = matcher7.__GetBuffers();
        var matcher8 = GetComponentBufferFromComponentType<T8>();
        var matcher8Buffers = matcher8.__GetBuffers();
        var matcher9 = GetComponentBufferFromComponentType<T9>();
        var matcher9Buffers = matcher9.__GetBuffers();
        var matcher10 = GetComponentBufferFromComponentType<T10>();
        var matcher10Buffers = matcher10.__GetBuffers();
        var matcher11 = GetComponentBufferFromComponentType<T11>();
        var matcher11Buffers = matcher11.__GetBuffers();
        var matcher12 = GetComponentBufferFromComponentType<T12>();
        var matcher12Buffers = matcher12.__GetBuffers();
        var matcher13 = GetComponentBufferFromComponentType<T13>();
        var matcher13Buffers = matcher13.__GetBuffers();
        var matcher14 = GetComponentBufferFromComponentType<T14>();
        var matcher14Buffers = matcher14.__GetBuffers();
        var matcher15 = GetComponentBufferFromComponentType<T15>();
        var matcher15Buffers = matcher15.__GetBuffers();
        var matcher16 = GetComponentBufferFromComponentType<T16>();
        var matcher16Buffers = matcher16.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    if (matcher3.Matches(entityData.Flags))
                    {
                        int indexInMatcher3 = matcher3.TryGetIndexFromKey(entIdx);
                        if (indexInMatcher3 >= 0)
                        {
                            ref T3 component3 = ref matcher3Buffers.data[indexInMatcher3];
                            if (matcher4.Matches(entityData.Flags))
                            {
                                int indexInMatcher4 = matcher4.TryGetIndexFromKey(entIdx);
                                if (indexInMatcher4 >= 0)
                                {
                                    ref T4 component4 = ref matcher4Buffers.data[indexInMatcher4];
                                    if (matcher5.Matches(entityData.Flags))
                                    {
                                        int indexInMatcher5 = matcher5.TryGetIndexFromKey(entIdx);
                                        if (indexInMatcher5 >= 0)
                                        {
                                            ref T5 component5 = ref matcher5Buffers.data[indexInMatcher5];
                                            if (matcher6.Matches(entityData.Flags))
                                            {
                                                int indexInMatcher6 = matcher6.TryGetIndexFromKey(entIdx);
                                                if (indexInMatcher6 >= 0)
                                                {
                                                    ref T6 component6 = ref matcher6Buffers.data[indexInMatcher6];
                                                    if (matcher7.Matches(entityData.Flags))
                                                    {
                                                        int indexInMatcher7 = matcher7.TryGetIndexFromKey(entIdx);
                                                        if (indexInMatcher7 >= 0)
                                                        {
                                                            ref T7 component7 = ref matcher7Buffers.data[indexInMatcher7];
                                                            if (matcher8.Matches(entityData.Flags))
                                                            {
                                                                int indexInMatcher8 = matcher8.TryGetIndexFromKey(entIdx);
                                                                if (indexInMatcher8 >= 0)
                                                                {
                                                                    ref T8 component8 = ref matcher8Buffers.data[indexInMatcher8];
                                                                    if (matcher9.Matches(entityData.Flags))
                                                                    {
                                                                        int indexInMatcher9 = matcher9.TryGetIndexFromKey(entIdx);
                                                                        if (indexInMatcher9 >= 0)
                                                                        {
                                                                            ref T9 component9 = ref matcher9Buffers.data[indexInMatcher9];
                                                                            if (matcher10.Matches(entityData.Flags))
                                                                            {
                                                                                int indexInMatcher10 = matcher10.TryGetIndexFromKey(entIdx);
                                                                                if (indexInMatcher10 >= 0)
                                                                                {
                                                                                    ref T10 component10 = ref matcher10Buffers.data[indexInMatcher10];
                                                                                    if (matcher11.Matches(entityData.Flags))
                                                                                    {
                                                                                        int indexInMatcher11 = matcher11.TryGetIndexFromKey(entIdx);
                                                                                        if (indexInMatcher11 >= 0)
                                                                                        {
                                                                                            ref T11 component11 = ref matcher11Buffers.data[indexInMatcher11];
                                                                                            if (matcher12.Matches(entityData.Flags))
                                                                                            {
                                                                                                int indexInMatcher12 = matcher12.TryGetIndexFromKey(entIdx);
                                                                                                if (indexInMatcher12 >= 0)
                                                                                                {
                                                                                                    ref T12 component12 = ref matcher12Buffers.data[indexInMatcher12];
                                                                                                    if (matcher13.Matches(entityData.Flags))
                                                                                                    {
                                                                                                        int indexInMatcher13 = matcher13.TryGetIndexFromKey(entIdx);
                                                                                                        if (indexInMatcher13 >= 0)
                                                                                                        {
                                                                                                            ref T13 component13 = ref matcher13Buffers.data[indexInMatcher13];
                                                                                                            if (matcher14.Matches(entityData.Flags))
                                                                                                            {
                                                                                                                int indexInMatcher14 = matcher14.TryGetIndexFromKey(entIdx);
                                                                                                                if (indexInMatcher14 >= 0)
                                                                                                                {
                                                                                                                    ref T14 component14 = ref matcher14Buffers.data[indexInMatcher14];
                                                                                                                    if (matcher15.Matches(entityData.Flags))
                                                                                                                    {
                                                                                                                        int indexInMatcher15 = matcher15.TryGetIndexFromKey(entIdx);
                                                                                                                        if (indexInMatcher15 >= 0)
                                                                                                                        {
                                                                                                                            ref T15 component15 = ref matcher15Buffers.data[indexInMatcher15];
                                                                                                                            if (matcher16.Matches(entityData.Flags))
                                                                                                                            {
                                                                                                                                int indexInMatcher16 = matcher16.TryGetIndexFromKey(entIdx);
                                                                                                                                if (indexInMatcher16 >= 0)
                                                                                                                                {
                                                                                                                                    ref T16 component16 = ref matcher16Buffers.data[indexInMatcher16];
                                                                                                                                    loopAction(entIdx, ref component, ref component2, ref component3, ref component4, ref component5, ref component6, ref component7, ref component8, ref component9, ref component10, ref component11, ref component12, ref component13, ref component14, ref component15, ref component16);
                                                                                                                                }//end if indexInMatcher16
                                                                                                                            }//end if matcher16.Matches
                                                                                                                        }//end if indexInMatcher15
                                                                                                                    }//end if matcher15.Matches
                                                                                                                }//end if indexInMatcher14
                                                                                                            }//end if matcher14.Matches
                                                                                                        }//end if indexInMatcher13
                                                                                                    }//end if matcher13.Matches
                                                                                                }//end if indexInMatcher12
                                                                                            }//end if matcher12.Matches
                                                                                        }//end if indexInMatcher11
                                                                                    }//end if matcher11.Matches
                                                                                }//end if indexInMatcher10
                                                                            }//end if matcher10.Matches
                                                                        }//end if indexInMatcher9
                                                                    }//end if matcher9.Matches
                                                                }//end if indexInMatcher8
                                                            }//end if matcher8.Matches
                                                        }//end if indexInMatcher7
                                                    }//end if matcher7.Matches
                                                }//end if indexInMatcher6
                                            }//end if matcher6.Matches
                                        }//end if indexInMatcher5
                                    }//end if matcher5.Matches
                                }//end if indexInMatcher4
                            }//end if matcher4.Matches
                        }//end if indexInMatcher3
                    }//end if matcher3.Matches
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function

}
