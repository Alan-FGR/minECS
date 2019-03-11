using System;
using System.Collections.Generic;
public unsafe partial class ArchetypePool {
    public int Add<T0, T1>(ulong UID,  Flags* flags,  T0 t0,  T1 t1)
        where T0 : unmanaged 
        where T1 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8,  T9 t9)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 
        var p9 = componentBuffers_[flags[9]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p9.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 
        p9.Set(ref t9, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8,  T9 t9,  T10 t10)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 
        var p9 = componentBuffers_[flags[9]]; 
        var p10 = componentBuffers_[flags[10]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p9.AssureRoomForMore(Count, one); 
        p10.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 
        p9.Set(ref t9, Count); 
        p10.Set(ref t10, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8,  T9 t9,  T10 t10,  T11 t11)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 
        var p9 = componentBuffers_[flags[9]]; 
        var p10 = componentBuffers_[flags[10]]; 
        var p11 = componentBuffers_[flags[11]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p9.AssureRoomForMore(Count, one); 
        p10.AssureRoomForMore(Count, one); 
        p11.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 
        p9.Set(ref t9, Count); 
        p10.Set(ref t10, Count); 
        p11.Set(ref t11, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8,  T9 t9,  T10 t10,  T11 t11,  T12 t12)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 
        var p9 = componentBuffers_[flags[9]]; 
        var p10 = componentBuffers_[flags[10]]; 
        var p11 = componentBuffers_[flags[11]]; 
        var p12 = componentBuffers_[flags[12]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p9.AssureRoomForMore(Count, one); 
        p10.AssureRoomForMore(Count, one); 
        p11.AssureRoomForMore(Count, one); 
        p12.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 
        p9.Set(ref t9, Count); 
        p10.Set(ref t10, Count); 
        p11.Set(ref t11, Count); 
        p12.Set(ref t12, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8,  T9 t9,  T10 t10,  T11 t11,  T12 t12,  T13 t13)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 
        var p9 = componentBuffers_[flags[9]]; 
        var p10 = componentBuffers_[flags[10]]; 
        var p11 = componentBuffers_[flags[11]]; 
        var p12 = componentBuffers_[flags[12]]; 
        var p13 = componentBuffers_[flags[13]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p9.AssureRoomForMore(Count, one); 
        p10.AssureRoomForMore(Count, one); 
        p11.AssureRoomForMore(Count, one); 
        p12.AssureRoomForMore(Count, one); 
        p13.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 
        p9.Set(ref t9, Count); 
        p10.Set(ref t10, Count); 
        p11.Set(ref t11, Count); 
        p12.Set(ref t12, Count); 
        p13.Set(ref t13, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8,  T9 t9,  T10 t10,  T11 t11,  T12 t12,  T13 t13,  T14 t14)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
        where T14 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 
        var p9 = componentBuffers_[flags[9]]; 
        var p10 = componentBuffers_[flags[10]]; 
        var p11 = componentBuffers_[flags[11]]; 
        var p12 = componentBuffers_[flags[12]]; 
        var p13 = componentBuffers_[flags[13]]; 
        var p14 = componentBuffers_[flags[14]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p9.AssureRoomForMore(Count, one); 
        p10.AssureRoomForMore(Count, one); 
        p11.AssureRoomForMore(Count, one); 
        p12.AssureRoomForMore(Count, one); 
        p13.AssureRoomForMore(Count, one); 
        p14.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 
        p9.Set(ref t9, Count); 
        p10.Set(ref t10, Count); 
        p11.Set(ref t11, Count); 
        p12.Set(ref t12, Count); 
        p13.Set(ref t13, Count); 
        p14.Set(ref t14, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }


    public int Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ulong UID,  Flags* flags,  T0 t0,  T1 t1,  T2 t2,  T3 t3,  T4 t4,  T5 t5,  T6 t6,  T7 t7,  T8 t8,  T9 t9,  T10 t10,  T11 t11,  T12 t12,  T13 t13,  T14 t14,  T15 t15)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
        where T14 : unmanaged 
        where T15 : unmanaged 
    {
        var p0 = componentBuffers_[flags[0]]; 
        var p1 = componentBuffers_[flags[1]]; 
        var p2 = componentBuffers_[flags[2]]; 
        var p3 = componentBuffers_[flags[3]]; 
        var p4 = componentBuffers_[flags[4]]; 
        var p5 = componentBuffers_[flags[5]]; 
        var p6 = componentBuffers_[flags[6]]; 
        var p7 = componentBuffers_[flags[7]]; 
        var p8 = componentBuffers_[flags[8]]; 
        var p9 = componentBuffers_[flags[9]]; 
        var p10 = componentBuffers_[flags[10]]; 
        var p11 = componentBuffers_[flags[11]]; 
        var p12 = componentBuffers_[flags[12]]; 
        var p13 = componentBuffers_[flags[13]]; 
        var p14 = componentBuffers_[flags[14]]; 
        var p15 = componentBuffers_[flags[15]]; 

        var one = 1;
        p0.AssureRoomForMore(Count, one); 
        p1.AssureRoomForMore(Count, one); 
        p2.AssureRoomForMore(Count, one); 
        p3.AssureRoomForMore(Count, one); 
        p4.AssureRoomForMore(Count, one); 
        p5.AssureRoomForMore(Count, one); 
        p6.AssureRoomForMore(Count, one); 
        p7.AssureRoomForMore(Count, one); 
        p8.AssureRoomForMore(Count, one); 
        p9.AssureRoomForMore(Count, one); 
        p10.AssureRoomForMore(Count, one); 
        p11.AssureRoomForMore(Count, one); 
        p12.AssureRoomForMore(Count, one); 
        p13.AssureRoomForMore(Count, one); 
        p14.AssureRoomForMore(Count, one); 
        p15.AssureRoomForMore(Count, one); 
        p0.Set(ref t0, Count); 
        p1.Set(ref t1, Count); 
        p2.Set(ref t2, Count); 
        p3.Set(ref t3, Count); 
        p4.Set(ref t4, Count); 
        p5.Set(ref t5, Count); 
        p6.Set(ref t6, Count); 
        p7.Set(ref t7, Count); 
        p8.Set(ref t8, Count); 
        p9.Set(ref t9, Count); 
        p10.Set(ref t10, Count); 
        p11.Set(ref t11, Count); 
        p12.Set(ref t12, Count); 
        p13.Set(ref t13, Count); 
        p14.Set(ref t14, Count); 
        p15.Set(ref t15, Count); 

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }

}