﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

struct Position
{
    public Vector2 pos;

    public override string ToString()
    {
        return $"{(int) pos.X}\n{(int) pos.Y}";
    }
}

struct Velocity
{
    public Vector2 vel;
    
    public override string ToString()
    {
        return $"{(int)vel.X}\n{(int)vel.Y}";
    }
}

struct Rect
{
    public Rectangle rectangle;
}

struct Name
{
    public string name;

    public override string ToString()
    {
        return name;
    }
}

struct Health
{
    public int amnt;
}

public class MinEcsTest : Game
{
    private GraphicsDeviceManager graphicsDeviceManager_;
    private GraphicsDevice gfx => graphicsDeviceManager_.GraphicsDevice;
    private static SpriteBatch sb;
    private static SpriteFont sf;
    private static Texture2D px;
    private static MouseState lastState;
    private static MouseState curState;

    static EntityRegistry registry = new EntityRegistry(8);

    static Queue<(int entIdx, Dictionary<ComponentBufferBase, int> buffersIndices)> debugLoopQueue_ =
        new Queue<(int entIdx, Dictionary<ComponentBufferBase, int> buffersIndices)>();

    public MinEcsTest()
    {
        graphicsDeviceManager_ = new GraphicsDeviceManager(this);
        graphicsDeviceManager_.PreferredBackBufferWidth = 800;
        graphicsDeviceManager_.PreferredBackBufferHeight = 700;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        registry.RegisterComponent<Position>(BufferType.Sparse,1);
        registry.RegisterComponent<Velocity>(BufferType.Sparse,1);
        registry.RegisterComponent<Rect>(BufferType.Sparse,1);
        registry.RegisterComponent<Health>(BufferType.Sparse,1);
        registry.RegisterComponent<Name>(BufferType.Dense,1);
        
        var r = new Random(42);
        var qty = 1<<17;
        //qty = 32;
        for (int i = 0; i < qty; i++)
        {
            var e = registry.CreateEntity();
            //if(i%2==0)
            //registry.DeleteEntity(e);
        }
        
        int[] shuffA = Enumerable.Range(0,qty).OrderBy(a => Guid.NewGuid()).ToArray();
        int[] shuffB = Enumerable.Range(0,qty).OrderBy(a => Guid.NewGuid()).ToArray();
        int[] shuffC = Enumerable.Range(0,qty).OrderBy(a => Guid.NewGuid()).ToArray();
        for (int i = 0; i < qty; i++)
        {
            if (r.Next(10) < 9) registry.AddComponent((ulong)shuffA[i], new Position { pos = new Vector2(shuffA[i], shuffA[i]) });
            if (r.Next(10) < 9) registry.AddComponent((ulong)shuffB[i], new Velocity { vel = new Vector2(r.Next(-1, 1), r.Next(-1, 1)) });
            if(r.Next(10)  < 9) registry.AddComponent((ulong)shuffC[i], new Rect());
            if(r.Next(10)  < 9) registry.AddComponent((ulong)shuffB[i], new Health());
            if(r.Next(10)  < 9) registry.AddComponent((ulong)shuffC[i], new Name {name = $"n:{i}"});
        }


//        var e = registry.CreateEntity();
//        registry.AddComponent(e, new Position());
//        registry.AddComponent(e, new Velocity { vel = new Vector2(1,1) });
//
//        var e1 = registry.CreateEntity();
//        registry.AddComponent(e1, new Position());
//        registry.AddComponent(e1, new Velocity { vel = new Vector2(1, 1) });

        //var e2 = registry.CreateEntity();
        //registry.AddComponent(e2, new Position());
        //registry.AddComponent(e2, new Velocity { vel = new Vector2(2,1)});

        //var e3 = registry.CreateEntity();
        //registry.AddComponent(e3, new Position());
        ////registry_.AddComponent(e3, new Velocity { x = 0, y = 1 });
    }

    protected override void LoadContent()
    {
        sb = new SpriteBatch(gfx);
        sf = Content.Load<SpriteFont>("Font.xnb");
        sf.LineSpacing = 7;
        px = new Texture2D(GraphicsDevice, 1, 1);
        px.SetData(new[] { Color.White });

        curState = Mouse.GetState();
        lastState = curState;
        
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public static void DrawLine(Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        Vector2 v = Vector2.Normalize(begin - end);
        float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        sb.Draw(px, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }

    public static void DrawString(Vector2 pos, string str, Color col)
    {
        sb.DrawString(sf, str ?? "null", pos, col);
    }
    
    Vector2 ind2pos(Vector2 left, int i)
    {
        return left + i * Vector2.UnitX * 28;
    }

    string mask2str(ulong mask)
    {
        return Convert.ToString((long) mask, 2).PadLeft(8, '0').Replace('0', '.').Replace('1', '|');
    }

    static bool imbutton(Vector2 pos, string text)
    {
        var mousepos = new Point(curState.X, curState.Y);
        var rect = new Rectangle((int) pos.X, (int) pos.Y, 28, 10);
        var hover = rect.Contains(mousepos);
        DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width-2, rect.Y), hover ? Color.White : Color.Gray);
        DrawString(new Vector2(rect.X, rect.Y-4), text, hover ? Color.White : Color.Gray);
        if (hover)
            if (curState.LeftButton == ButtonState.Pressed && lastState.LeftButton != ButtonState.Pressed)
                return true;
        return false;
    }

    public static void AddComp<T>(ulong entuid, bool remove) where T : struct
    {
        if(remove)
            registry.RemoveComponent<T>(entuid);
        else
            registry.AddComponent(entuid, new T());
    }

    private (int entIdx, Dictionary<ComponentBufferBase, int> buffersIndices)? CurrentDbgLoopEntry =>
        debugLoopQueue_.Count > 0 ? debugLoopQueue_.Peek() : ((int entIdx, Dictionary<ComponentBufferBase, int> buffersIndices)?) null;

    public static List<string> timings = new List<string>();

    void DrawBuffer(Vector2 position)
    {
        if (registry.Count > 256)
        {
            DrawString(position, $"TOO MUCH DATA TO RENDER! ({registry.Count} entities)\ntimings:\n{String.Join("\n", timings)}", Color.Red);

            return;
        }

        var buffers = registry.__GetBuffers();

        var entVals = position + Vector2.UnitY * 0;
        var keysPos = position + Vector2.UnitY * 28;
        var k2iPos = position + Vector2.UnitY * 64;

        int hspc = 28;

        DrawString(entVals+Vector2.UnitY*-6, "data:", Color.Gray);
        DrawString(entVals+Vector2.UnitY*0 , "tags:", Color.Gray);
        DrawString(entVals+Vector2.UnitY*6 , "dens:", Color.Gray);
        DrawString(entVals+Vector2.UnitY*12, "spar:", Color.Gray);
        for (var i = 0; i < registry.Count; i++)
        {
            var val = buffers.data[i];

            var entuid = buffers.i2k[i];

            Vector2 pos = ind2pos(entVals + Vector2.UnitX * hspc, i);

            DrawString(pos+Vector2.UnitY*0 , mask2str(val.Tags), Color.LightBlue);
            DrawString(pos+Vector2.UnitY*6 , mask2str(val.FlagsDense), Color.Gold);
            DrawString(pos+Vector2.UnitY*12,mask2str(val.FlagsSparse), Color.LightSeaGreen);

            if (CurrentDbgLoopEntry != null && CurrentDbgLoopEntry.Value.entIdx == i)
                sb.Draw(px, new Rectangle((int)pos.X, (int)pos.Y, 20, 30), new Color(Color.Red, 0.5f));

            if (imbutton(pos + Vector2.UnitY * -10, "REMOV"))
            {
                registry.DeleteEntity(buffers.i2k[i]);
                return;
            }

            int c2 = 2;
            foreach (ComponentBufferBase buffer in registry.GetDebugComponentBufferBases())
            {

                var hascomp = buffer.HasComponentSlow(ref val);

                string compNameStr = buffer.GetType().GenericTypeArguments[0].Name;
                var comptype = Type.GetType(compNameStr, false, true);
                MethodInfo methodInfo = GetType().GetMethod("AddComp");
                MethodInfo genericMethod = methodInfo.MakeGenericMethod(comptype);

                if (imbutton(pos+Vector2.UnitY * -10 * c2, (hascomp ? "-" : "+")+compNameStr.Substring(0, 3)))
                {
                    if (hascomp)
                        genericMethod.Invoke(this, new object[] {entuid, true});
                    else
                        genericMethod.Invoke(this, new object[] {entuid, false});
                }

                c2++;
            }
            
        }
        
        var keysRenderPos = RenderKeys(keysPos, buffers.i2k, "UIDs", registry.Count, hspc, Color.Red);
        RenderDenseMap(k2iPos, buffers.k2i, hspc, keysRenderPos);

        var sp = k2iPos + Vector2.UnitY * 64;

        int c = 0;
        var cols = new[]{Color.Red, Color.Yellow, Color.Lime, Color.HotPink, Color.CadetBlue, };

        foreach (ComponentBufferBase buffer in registry.GetDebugComponentBufferBases())
        {

            string compNameStr = buffer.GetType().GenericTypeArguments[0].Name;
            var comptype = Type.GetType(compNameStr, false, true);

            string bt = buffer.Sparse ? "Sparse" : "Dense";

            DrawString(sp, $"{compNameStr} Buffer (Type: {bt}, Count: {buffer.ComponentCount})", Color.Orange);

            DrawString(sp + Vector2.UnitY * 6, $"flag: {mask2str(buffer.Matcher.Flag)} {bt[0]}", buffer.Sparse ? Color.LightSeaGreen : Color.Gold);

            var bd = buffer.GetDebugFlagAndEntIdxs();
            var keysps = RenderKeys(sp + Vector2.UnitY * 16, bd.endIdxs, "enti", buffer.ComponentCount, hspc, cols[c], keysPos);

            if (CurrentDbgLoopEntry != null)
                if (CurrentDbgLoopEntry.Value.buffersIndices.TryGetValue(buffer, out int indexInThisBuffer))
                    sb.Draw(px, new Rectangle((int) sp.X + 28 + 28*indexInThisBuffer, (int) sp.Y+28, 20, 8), new Color(Color.Yellow, 1f));

            //buffer.GetDebugIdxFromKey()

            if (!buffer.Sparse)
            {
                GetType().GetMethod("RenderCompBufDense").MakeGenericMethod(comptype)
                    .Invoke(this, new object[] { sp + Vector2.UnitY * 48, buffer, keysps});
            }
            else
            {
                GetType().GetMethod("RenderCompBufSparse").MakeGenericMethod(comptype)
                    .Invoke(this, new object[] { sp + Vector2.UnitY * 48, buffer, keysps});
            }

            sp += Vector2.UnitY*96;
            c++;
        }

        if (debugLoopQueue_.Count > 0)
        {
            ctr++;
            if (ctr > 3)
            {
                debugLoopQueue_.Dequeue();
                ctr = 0;
            }
        }
    }

    private int ctr = 0;

    public void RenderCompBufDense<T>(Vector2 pos, ComponentBufferBase buf, Dictionary<int, Vector2> keysRenderPos) where T : struct
    {
        var b = (ComponentBufferDense<T>) buf;
        var buffers = b.__GetBuffers();
        RenderDenseMap(pos, buffers.entIdx2i, 28, keysRenderPos);

        for (var i = 0; i < b.ComponentCount; i++)
        {
            T comp = buffers.data[i];
            Vector2 dpos = ind2pos(pos + Vector2.UnitY * 8 + Vector2.UnitX * 28, i);
            DrawString(dpos, comp.ToString(), Color.DarkSlateGray);
        }
    }

    public void RenderCompBufSparse<T>(Vector2 pos, ComponentBufferBase buf, Dictionary<int, Vector2> keysRenderPos) where T : struct
    {
        var b = (ComponentBufferSparse<T>) buf;
        var buffers = b.__GetBuffers();

        DrawString(pos, "k2iS:", Color.Magenta);

        for (var i = 0; i < buffers.entIdx2i.Length; i++)
        {
            int compIdx = buffers.entIdx2i[i];
            Vector2 posw = ind2pos(pos + Vector2.UnitX * 28, i);
            DrawString(posw, i + ":" + compIdx, compIdx >= 0 ? Color.Lime : Color.IndianRed);

            try
            {
                if(keysRenderPos.ContainsKey(i))
                DrawLine(keysRenderPos[i] + Vector2.One * 10, posw + Vector2.One * 10,
                    new Color(Color.Cyan, 0.5f));
            }
            catch
            {
            }
        }

        for (var i = 0; i < buffers.data.Length; i++)
        {
            T comp = buffers.data[i];
            Vector2 dpos = ind2pos(pos + Vector2.UnitY * 8 + Vector2.UnitX * 28, i);
            DrawString(dpos, comp.ToString(), Color.DarkSlateGray);
        }
    }

    private void RenderDenseMap<T>(Vector2 k2iPos, Dictionary<T, int> map, int hspc, Dictionary<T, Vector2> keysRenderPos)
    {
        DrawString(k2iPos, "k2iD:", Color.Cyan);
        int c = 0;
        foreach (KeyValuePair<T, int> pair in map)
        {
            Vector2 pos = ind2pos(k2iPos + Vector2.UnitX * hspc, c++);
            DrawString(pos, pair.Key + ":" + pair.Value, Color.Lime);

            try
            {
                DrawLine(keysRenderPos[pair.Key] + Vector2.One * 10, pos + Vector2.One * 10, new Color(Color.Cyan, 0.5f));
            }
            catch {}
        }
    }

    private Dictionary<T, Vector2> RenderKeys<T>(Vector2 keysPos, T[] keys, string keysname, int usedCount, int hspc, Color linecol, Vector2? entKeysPos = null)
    {
        Dictionary<T, Vector2> keysRenderPos = new Dictionary<T, Vector2>();

        DrawString(keysPos, $"{keysname}:", Color.White);
        for (var i = 0; i < usedCount; i++)
        {
            T val = keys[i];
            Vector2 pos = ind2pos(keysPos + Vector2.UnitX * hspc, i);
            keysRenderPos[val] = pos; //todo fixme (dense only) use add here and pass dict from dense to check if key is valid before adding
            DrawString(pos, val.ToString(), Color.Magenta);

            if (entKeysPos != null)
            {
                DrawLine(pos, ind2pos(entKeysPos.Value + Vector2.UnitX * hspc, Convert.ToInt32(val)), new Color(linecol,0.6f));
            }
        }

        DrawString(keysPos + Vector2.UnitY * 8, "raw:", Color.DimGray);
        for (var i = 0; i < keys.Length; i++)
        {
            T val = keys[i];
            Vector2 pos = ind2pos(keysPos + Vector2.UnitY * 8 + Vector2.UnitX * hspc, i);
            DrawString(pos, val.ToString(), new Color(Color.DarkMagenta, 0.5f));
        }

        DrawString(keysPos + Vector2.UnitY * 16, "idx:", Color.DarkGray);
        for (var i = 0; i < keys.Length; i++)
        {
            Vector2 pos = ind2pos(keysPos + Vector2.UnitY * 16 + Vector2.UnitX * hspc, i);
            DrawString(pos, i.ToString(), Color.DodgerBlue);
        }

        return keysRenderPos;
    }

    static Stopwatch sw = new Stopwatch();

    public static void Time(string name, Action action)
    {
        sw.Restart();
        action.Invoke();
        timings.Add(name + ": " + sw.ElapsedMilliseconds + " ms");
        //timings.Add(name + ": " + sw.ElapsedTicks / (Stopwatch.Frequency / 1000000f) + "us");
    }

    protected override void Draw(GameTime gameTime)
    {
        // registry.Loop((int entIdx, ref Position pos, ref Velocity vel) =>
        // {
        //     pos.pos += vel.vel;
        // });

        gfx.Clear(Color.Black);

        curState = Mouse.GetState();

        sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

        if (imbutton(new Vector2(10, 10), "Add Entity"))
        {
            registry.CreateEntity();
        }

        if (imbutton(new Vector2(110, 10), "Sort Entities"))
        {
            Time("Sort Entities",() =>
            {
                registry.SortEntities();
            });
        }

        Type firstCompType = null;
        var c = 0;
        foreach (ComponentBufferBase buffer in registry.GetDebugComponentBufferBases())
        {
            string compNameStr = buffer.GetType().GenericTypeArguments[0].Name;
            var comptype = Type.GetType(compNameStr, false, true);

            if (imbutton(new Vector2(250 + c++ * 100, 10), $"Sort {compNameStr}"))
            {
                Time($"Sort {compNameStr}",() =>
                {
                registry.GetType().GetMethod("SortComponents").MakeGenericMethod(comptype).Invoke(registry, new object[0]);
                });
            }

            // if (firstCompType != null && buffer.Sparse && imbutton(new Vector2(150 + c * 100, 20), $"Streamline {compNameStr}"))
            // {
            //     Time( $"Streamline {compNameStr}",() =>
            //     {
            //     registry.GetType().GetMethod("StreamlineComponents").MakeGenericMethod(firstCompType, comptype).Invoke(registry, new object[0]);
            //     });
            // }

            if(buffer.Sparse)
                firstCompType = comptype;
        }

        if (imbutton(new Vector2(250 + c * 100, 10), "Debug Loop"))
        {
            List<string> compNames = new List<string>();
            foreach (ComponentBufferBase buffer in registry.GetDebugComponentBufferBases())
            {
                var typeStr = buffer.GetType().GenericTypeArguments[0].AssemblyQualifiedName;
                compNames.Add(typeStr);
            }
            registry.DebugLoop((entIdx, compIdxsDict) =>
            {
                debugLoopQueue_.Enqueue((entIdx, compIdxsDict));
            }, compNames.ToArray());
        }

        DrawBuffer(new Vector2(10,100));
        
        sb.End();

        lastState = curState;

        base.Draw(gameTime);
    }
}

class Program
{
    static void Main(string[] args)
    {
        new MinEcsTest().Run();
    }
}

