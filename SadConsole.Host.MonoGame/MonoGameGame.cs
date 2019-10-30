﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SadConsole.MonoGame
{
    /// <summary>
    /// A MonoGame <see cref="Microsoft.Xna.Framework.Game"/> instance that runs SadConsole.
    /// </summary>
    public sealed partial class Game : Microsoft.Xna.Framework.Game
    {
        internal bool _resizeBusy = false;
        internal Action<Game> _initCallback;

        /// <summary>
        /// Global instance of the MonoGame Game.
        /// </summary>
        public static Game Instance { get; private set; }

        /// <summary>
        /// Graphics device manager used by MonoGame.
        /// </summary>
        public GraphicsDeviceManager GraphicsDeviceManager;

        /// <summary>
        /// Sprite batch used for rendering.
        /// </summary>
        public SpriteBatch SpriteBatch;

        /// <summary>
        /// The render target of SadConsole. This is generally rendered to the screen as the final step of drawing.
        /// </summary>
        public RenderTarget2D RenderOutput;

        /// <summary>
        /// The current game window width.
        /// </summary>
        public int WindowWidth => GraphicsDevice.PresentationParameters.BackBufferWidth;

        /// <summary>
        /// The current game window height.
        /// </summary>
        public int WindowHeight => GraphicsDevice.PresentationParameters.BackBufferHeight;

        /// <summary>
        /// Raised when the window is resized and the render area has been calculated.
        /// </summary>
        public event EventHandler WindowResized;

        internal Game(Action<Game> ctorCallback, Action<Game> initCallback)
        {
            Instance = this;

            _initCallback = initCallback;

            GraphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = Microsoft.Xna.Framework.Graphics.GraphicsProfile.Reach
            };

            Content.RootDirectory = "Content";

            GraphicsDeviceManager.HardwareModeSwitch = Settings.UseHardwareFullScreen;
            ctorCallback?.Invoke(this);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (!_resizeBusy)
            {
                if (!GraphicsDeviceManager.IsFullScreen && SadConsole.Settings.WindowMinimumSize != Point.Zero.ToPoint())
                {
                    if (GraphicsDevice.PresentationParameters.BackBufferWidth < SadConsole.Settings.WindowMinimumSize.X
                        || GraphicsDevice.PresentationParameters.BackBufferHeight < SadConsole.Settings.WindowMinimumSize.Y)
                    {
                        _resizeBusy = true;
                        GraphicsDeviceManager.PreferredBackBufferWidth = SadConsole.Settings.WindowMinimumSize.X;
                        GraphicsDeviceManager.PreferredBackBufferHeight = SadConsole.Settings.WindowMinimumSize.Y;
                        GraphicsDeviceManager.ApplyChanges();
                    }
                }
            }
            else
            {
                _resizeBusy = false;
            }

            //if (!resizeBusy && Settings.IsExitingFullscreen)
            //{
            //    GraphicsDeviceManager.PreferredBackBufferWidth = Global.WindowWidth;
            //    GraphicsDeviceManager.PreferredBackBufferHeight = Global.WindowHeight;

            //    resizeBusy = true;
            //    GraphicsDeviceManager.ApplyChanges();
            //    resizeBusy = false;
            //    Settings.IsExitingFullscreen = false;
            //}

            //Global.WindowWidth = GraphicsDeviceManager.PreferredBackBufferWidth;
            //Global.WindowHeight = GraphicsDeviceManager.PreferredBackBufferHeight;
            //Global.WindowWidth = Global.RenderWidth = GraphicsDeviceManager.PreferredBackBufferWidth;
            //Global.WindowHeight = Global.RenderHeight = GraphicsDeviceManager.PreferredBackBufferHeight;
            ResetRendering();

            if (!_resizeBusy)
            {
                WindowResized?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void Initialize()
        {
            if (SadConsole.Settings.UnlimitedFPS)
            {
                GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                IsFixedTimeStep = false;
            }

            // Let the XNA framework show the mouse.
            IsMouseVisible = true;

            // Initialize the SadConsole engine with a font, and a screen size that mirrors MS-DOS.
            Components.Add(new ClearScreenGameComponent(this));
            Components.Add(new SadConsoleGameComponent(this));

            // Call the default initialize of the base class.
            base.Initialize();

            // Hook window change for resolution fixes
            //Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowUserResizing = SadConsole.Settings.AllowWindowResize;

            Instance.GraphicsDeviceManager = GraphicsDeviceManager;
            Instance.SpriteBatch = new SpriteBatch(GraphicsDevice);

            _initCallback?.Invoke(this);

            ResetRendering();

            // After we've init, clear the graphics device so everything is ready to start
            GraphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Resizes the graphics device manager based on this font's glyph size.
        /// </summary>
        /// <param name="manager">Graphics device manager to resize.</param>
        /// <param name="width">The width glyphs.</param>
        /// <param name="height">The height glyphs.</param>
        /// <param name="additionalWidth">Additional pixel width to add to the resize.</param>
        /// <param name="additionalHeight">Additional pixel height to add to the resize.</param>
        public void ResizeGraphicsDeviceManager(Font font, int width, int height, int additionalWidth, int additionalHeight)
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = (font.Size.X * width) + additionalWidth;
            GraphicsDeviceManager.PreferredBackBufferHeight = (font.Size.Y * height) + additionalHeight;

            SadConsole.Settings.Rendering.RenderWidth = GraphicsDeviceManager.PreferredBackBufferWidth;
            SadConsole.Settings.Rendering.RenderHeight = GraphicsDeviceManager.PreferredBackBufferHeight;

            GraphicsDeviceManager.ApplyChanges();
        }

        /// <summary>
        /// Resets the <see cref="RenderOutput"/> target and determines the appropriate <see cref="RenderRect"/> and <see cref="RenderScale"/> based on the window or fullscreen state.
        /// </summary>
        public void ResetRendering()
        {
            if (SadConsole.Settings.ResizeMode == SadConsole.Settings.WindowResizeOptions.Center)
            {
                RenderOutput = new RenderTarget2D(GraphicsDevice, SadConsole.Settings.Rendering.RenderWidth, SadConsole.Settings.Rendering.RenderHeight);
                SadConsole.Settings.Rendering.RenderRect = new Rectangle(
                                                            (GraphicsDevice.PresentationParameters.BackBufferWidth - SadConsole.Settings.Rendering.RenderWidth) / 2,
                                                            (GraphicsDevice.PresentationParameters.BackBufferHeight - SadConsole.Settings.Rendering.RenderHeight) / 2,
                                                            SadConsole.Settings.Rendering.RenderWidth,
                                                            SadConsole.Settings.Rendering.RenderHeight).ToRectangle();

                SadConsole.Settings.Rendering.RenderScale = new System.Numerics.Vector2(1);
            }
            else if (SadConsole.Settings.ResizeMode == SadConsole.Settings.WindowResizeOptions.Scale)
            {
                RenderOutput = new RenderTarget2D(GraphicsDevice, SadConsole.Settings.Rendering.RenderWidth, SadConsole.Settings.Rendering.RenderHeight);
                int multiple = 2;

                // Find the bounds
                while (true)
                {
                    if (SadConsole.Settings.Rendering.RenderWidth * multiple > GraphicsDevice.PresentationParameters.BackBufferWidth || SadConsole.Settings.Rendering.RenderHeight * multiple > GraphicsDevice.PresentationParameters.BackBufferHeight)
                    {
                        multiple--;
                        break;
                    }

                    multiple++;
                }

                SadConsole.Settings.Rendering.RenderRect = new Rectangle((GraphicsDevice.PresentationParameters.BackBufferWidth - (SadConsole.Settings.Rendering.RenderWidth * multiple)) / 2,
                                                                         (GraphicsDevice.PresentationParameters.BackBufferHeight - (SadConsole.Settings.Rendering.RenderHeight * multiple)) / 2,
                                                                         SadConsole.Settings.Rendering.RenderWidth * multiple,
                                                                         SadConsole.Settings.Rendering.RenderHeight * multiple).ToRectangle();
                SadConsole.Settings.Rendering.RenderScale = new System.Numerics.Vector2(SadConsole.Settings.Rendering.RenderWidth / ((float)SadConsole.Settings.Rendering.RenderWidth * multiple), SadConsole.Settings.Rendering.RenderHeight / (float)(SadConsole.Settings.Rendering.RenderHeight * multiple));
            }
            else if (SadConsole.Settings.ResizeMode == SadConsole.Settings.WindowResizeOptions.Fit)
            {
                RenderOutput = new RenderTarget2D(GraphicsDevice, SadConsole.Settings.Rendering.RenderWidth, SadConsole.Settings.Rendering.RenderHeight);
                float heightRatio = GraphicsDevice.PresentationParameters.BackBufferHeight / (float)SadConsole.Settings.Rendering.RenderHeight;
                float widthRatio = GraphicsDevice.PresentationParameters.BackBufferWidth / (float)SadConsole.Settings.Rendering.RenderWidth;

                float fitHeight = SadConsole.Settings.Rendering.RenderHeight * widthRatio;
                float fitWidth = SadConsole.Settings.Rendering.RenderWidth * heightRatio;

                if (fitHeight <= GraphicsDevice.PresentationParameters.BackBufferHeight)
                {
                    // Render width = window width, pad top and bottom

                    SadConsole.Settings.Rendering.RenderRect = new Rectangle(0,
                                                                            (int)((GraphicsDevice.PresentationParameters.BackBufferHeight - fitHeight) / 2),
                                                                            GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                                            (int)fitHeight).ToRectangle();

                    SadConsole.Settings.Rendering.RenderScale = new System.Numerics.Vector2(SadConsole.Settings.Rendering.RenderWidth / (float)GraphicsDevice.PresentationParameters.BackBufferWidth, SadConsole.Settings.Rendering.RenderHeight / fitHeight);
                }
                else
                {
                    // Render height = window height, pad left and right

                    SadConsole.Settings.Rendering.RenderRect = new Rectangle((int)((GraphicsDevice.PresentationParameters.BackBufferWidth - fitWidth) / 2),
                                                                             0,
                                                                             (int)fitWidth,
                                                                             GraphicsDevice.PresentationParameters.BackBufferHeight).ToRectangle();

                    SadConsole.Settings.Rendering.RenderScale = new System.Numerics.Vector2(SadConsole.Settings.Rendering.RenderWidth / fitWidth, SadConsole.Settings.Rendering.RenderHeight / (float)GraphicsDevice.PresentationParameters.BackBufferHeight);
                }
            }
            else if (SadConsole.Settings.ResizeMode == SadConsole.Settings.WindowResizeOptions.None)
            {
                SadConsole.Settings.Rendering.RenderWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
                SadConsole.Settings.Rendering.RenderHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
                RenderOutput = new RenderTarget2D(GraphicsDevice, SadConsole.Settings.Rendering.RenderWidth, SadConsole.Settings.Rendering.RenderHeight);
                SadConsole.Settings.Rendering.RenderRect = GraphicsDevice.Viewport.Bounds.ToRectangle();
                SadConsole.Settings.Rendering.RenderScale = new System.Numerics.Vector2(1);
            }
            else
            {
                RenderOutput = new RenderTarget2D(GraphicsDevice, SadConsole.Settings.Rendering.RenderWidth, SadConsole.Settings.Rendering.RenderHeight);
                SadConsole.Settings.Rendering.RenderRect = GraphicsDevice.Viewport.Bounds.ToRectangle();
                SadConsole.Settings.Rendering.RenderScale = new System.Numerics.Vector2(SadConsole.Settings.Rendering.RenderWidth / (float)GraphicsDevice.PresentationParameters.BackBufferWidth, SadConsole.Settings.Rendering.RenderHeight / (float)GraphicsDevice.PresentationParameters.BackBufferHeight);
            }
        }
    }
}