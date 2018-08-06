using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

struct Position
{
    public Vector2 pos;
}

struct Velocity
{
    public Vector2 vel;
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

    public MinEcsTest()
    {
        graphicsDeviceManager_ = new GraphicsDeviceManager(this);
        graphicsDeviceManager_.PreferredBackBufferWidth = 1000;
        graphicsDeviceManager_.PreferredBackBufferHeight = 600;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        registry.RegisterComponent<Position>(BufferType.Dense,1);
        registry.RegisterComponent<Velocity>(BufferType.Dense,1);

        var e = registry.CreateEntity();
        registry.AddComponent(e, new Position());
        registry.AddComponent(e, new Velocity { vel = new Vector2(1,1) });

        var e1 = registry.CreateEntity();
        registry.AddComponent(e1, new Position());
        //registry_.AddComponent(e1, new Velocity { x = 0, y = 1 });

        var e2 = registry.CreateEntity();
        registry.AddComponent(e2, new Position());
        registry.AddComponent(e2, new Velocity { vel = new Vector2(2,1)});

        var e3 = registry.CreateEntity();
        registry.AddComponent(e3, new Position());
        //registry_.AddComponent(e3, new Velocity { x = 0, y = 1 });
    }

    protected override void LoadContent()
    {
        sb = new SpriteBatch(gfx);
        sf = Content.Load<SpriteFont>("Font.xnb");
        px = new Texture2D(GraphicsDevice, 1, 1);
        px.SetData(new[] { Color.White });

        curState = Mouse.GetState();
        lastState = curState;
        
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        registry.Loop((int entIdx, ref Position pos, ref Velocity vel) =>
        {
            pos.pos += vel.vel;
        });

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
        sb.DrawString(sf, str, pos, col);
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
        DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), hover ? Color.White : Color.Gray);
        DrawString(new Vector2(rect.X, rect.Y-4), text, hover ? Color.White : Color.Gray);
        if (hover)
            if (curState.LeftButton == ButtonState.Pressed && lastState.LeftButton != ButtonState.Pressed)
                return true;
        return false;
    }

    void DrawBuffer(Vector2 position)
    {
        var buffers = registry.__GetBuffers();

        var entVals = position + Vector2.UnitY * 0;
        var keysPos = position + Vector2.UnitY * 28;
        var k2iPos = position + Vector2.UnitY * 64;

        int hspc = 28;

        DrawString(entVals, "data:", Color.Gray);
        DrawString(entVals+Vector2.UnitY*6 , "tags:", Color.Gray);
        DrawString(entVals+Vector2.UnitY*12 , "dens:", Color.Gray);
        DrawString(entVals+Vector2.UnitY*18, "spar:", Color.Gray);
        for (var i = 0; i < registry.Count; i++)
        {
            var val = buffers.data[i];

            var entuid = buffers.i2k[i];

            Vector2 pos = ind2pos(entVals + Vector2.UnitX * hspc, i);

            DrawString(pos+Vector2.UnitY*0 , mask2str(val.Tags), Color.RosyBrown);
            DrawString(pos+Vector2.UnitY*6 , mask2str(val.FlagsDense), Color.Khaki);
            DrawString(pos+Vector2.UnitY*12,mask2str(val.FlagsSparse), Color.Olive);

            if (imbutton(pos + Vector2.UnitY * -10, "rem"))
            {
                //registry.RemoveKey(buffers.i2k[i]);
            }

            if (imbutton(pos + Vector2.UnitY * -20, "+pos"))
                registry.AddComponent(entuid, new Position());
            if (imbutton(pos + Vector2.UnitY * -30, "+vel"))
                registry.AddComponent(entuid, new Velocity());

        }
        
        var keysRenderPos = RenderKeys(keysPos, buffers.i2k, registry.Count, hspc, Color.Red);
        RenderDenseMap(k2iPos, buffers.k2i, hspc, keysRenderPos);

        var sp = k2iPos + Vector2.UnitY * 96;

        int c = 0;
        var cols = new[]{Color.Red, Color.Yellow, Color.Lime, Color.Cyan, Color.CadetBlue, };

        foreach (ComponentBufferBase buffer in registry.GetDebugComponentBufferBases())
        {
            DrawString(sp, $"{buffer.GetType().GenericTypeArguments[0].Name} Buffer", Color.Orange);

            DrawString(sp + Vector2.UnitY * 6, $"flag: {mask2str(buffer.Matcher.Flag)}", Color.White);

            var bd = buffer.GetDebugFlagAndEntIdxs();
            RenderKeys(sp + Vector2.UnitY * 16, bd.endIdxs, buffer.ComponentCount, hspc, cols[c], keysPos);
            
            sp += Vector2.UnitY*64;
            c++;
        }
        

    }

    private void RenderDenseMap<T>(Vector2 k2iPos, Dictionary<T, int> map, int hspc, Dictionary<T, Vector2> keysRenderPos)
    {
        DrawString(k2iPos, "k2i:", Color.White);
        int c = 0;
        foreach (KeyValuePair<T, int> pair in map)
        {
            Vector2 pos = ind2pos(k2iPos + Vector2.UnitX * hspc, c++);
            DrawString(pos, pair.Key + ":" + pair.Value, Color.Lime);
            DrawLine(keysRenderPos[pair.Key] + Vector2.One * 10, pos + Vector2.One * 10, new Color(Color.Blue, 0.6f));
        }
    }

    private Dictionary<T, Vector2> RenderKeys<T>(Vector2 keysPos, T[] keys, int usedCount, int hspc, Color linecol, Vector2? entKeysPos = null)
    {
        Dictionary<T, Vector2> keysRenderPos = new Dictionary<T, Vector2>();

        DrawString(keysPos, "keys:", Color.White);
        for (var i = 0; i < usedCount; i++)
        {
            T val = keys[i];
            Vector2 pos = ind2pos(keysPos + Vector2.UnitX * hspc, i);
            keysRenderPos.Add(val, pos);
            DrawString(pos, val.ToString(), Color.Magenta);

            if (entKeysPos != null)
            {
                DrawLine(pos, ind2pos(entKeysPos.Value + Vector2.UnitX * hspc, Convert.ToInt32(val)), new Color(linecol,0.6f));
            }
        }

        DrawString(keysPos + Vector2.UnitY * 8, "raw:", Color.Gray);
        for (var i = 0; i < keys.Length; i++)
        {
            T val = keys[i];
            Vector2 pos = ind2pos(keysPos + Vector2.UnitY * 8 + Vector2.UnitX * hspc, i);
            DrawString(pos, val.ToString(), Color.DarkMagenta);
        }

        return keysRenderPos;
    }

    protected override void Draw(GameTime gameTime)
    {
        gfx.Clear(Color.Black);

        curState = Mouse.GetState();

        sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

        if (imbutton(new Vector2(10, 10), "Add Entity"))
        {
            registry.CreateEntity();
        }

        DrawBuffer(new Vector2(10,200));
        
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

