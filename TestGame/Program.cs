using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

struct Position
{
    public Point Point;
    public Vector2 AsVector2 => new Vector2(Point.X,Point.Y);
}

struct Velocity
{
    public Point Point;
}

struct Dimensions
{
    public int Width;
    public int Height;
}

struct Sprite
{
    public Rectangle SourceRectangle;
}

public class TestGame : Game
{
    GraphicsDeviceManager graphics_;
    Dictionary<string, Rectangle> sprites_ = new Dictionary<string, Rectangle>();
    private Texture2D atlas_;
    SpriteBatch spriteBatch_;
    EntityRegistry registry_ = new EntityRegistry();

    public TestGame()
    {
        registry_.RegisterComponent<Position>(BufferType.Sparse);
        registry_.RegisterComponent<Velocity>(BufferType.Sparse);
        registry_.RegisterComponent<Dimensions>(BufferType.Sparse);
        registry_.RegisterComponent<Sprite>(BufferType.Sparse);

        graphics_ = new GraphicsDeviceManager(this);
        spriteBatch_ = new SpriteBatch(graphics_.GraphicsDevice);

        Content.RootDirectory = "Assets";
        atlas_ = Content.Load<Texture2D>("sheet.png");

        var sheet = new XmlDocument();
        sheet.Load("Assets/sheet.xml");
        
        foreach (XmlNode xmlNode in sheet.FirstChild.ChildNodes)
        {
            var name = xmlNode.Attributes["name"].Value.Split('.')[0];
            var x = int.Parse(xmlNode.Attributes["x"].Value);
            var y = int.Parse(xmlNode.Attributes["y"].Value);
            var width = int.Parse(xmlNode.Attributes["width"].Value);
            var height = int.Parse(xmlNode.Attributes["height"].Value);
            sprites_.Add(name, new Rectangle(x, y, width, height));
        }

        graphics_.PreferredBackBufferWidth = 600;
        graphics_.PreferredBackBufferHeight = 720;
        graphics_.ApplyChanges();

        IntPtr surf = SDL2.SDL_image.IMG_Load("Assets/p1.png");
        IntPtr cur = SDL2.SDL.SDL_CreateColorCursor(surf, 50, 30);
        SDL2.SDL.SDL_SetCursor(cur);
        SDL2.SDL.SDL_ShowCursor(1);
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch_ = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

        var mouseState = Mouse.GetState();

        var playerPosition = new Point(mouseState.X, mouseState.Y);
        var playerRadius = 20;

        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            for (int i = 0; i < 32; i++)
            {
                var bullet = registry_.CreateEntity();
                registry_.AddComponent(bullet, new Position{Point = new Point(playerPosition.X+(i-16)*8, playerPosition.Y)});
                registry_.AddComponent(bullet, new Velocity{Point = new Point(0,-10)});
                registry_.AddComponent(bullet, new Sprite{SourceRectangle = sprites_["laserBlue01"]});
            }
        }

        registry_.Loop((int entIdx, ref Position position, ref Velocity velocity) =>
            {
                position.Point += velocity.Point;
                if (position.Point.Y < -100)
                {
                    var entUid = registry_.EntityUIDFromIdx(entIdx);
                    registry_.DeleteEntity(entUid);
                }
            });

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics_.GraphicsDevice.Clear(Color.DarkSlateBlue);

        spriteBatch_.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

        registry_.Loop((int entIdx, ref Sprite sprite, ref Position position) =>
        {
            spriteBatch_.Draw(atlas_,
                position.AsVector2 - new Vector2(sprite.SourceRectangle.Width/2f, sprite.SourceRectangle.Height/8f),
                sprite.SourceRectangle, Color.White);
        });

        spriteBatch_.End();

        base.Draw(gameTime);
    }
}


class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        new TestGame().Run();
    }
}

