﻿using Microsoft.Xna.Framework;
using Point = SadRogue.Primitives.Point;

namespace SadConsole.MonoGame
{
    class Mouse : SadConsole.Input.IMouseState
    {
        Microsoft.Xna.Framework.Input.MouseState _mouse;

        public Mouse()
        {
            _mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
        }

        public bool IsLeftButtonDown => _mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;

        public bool IsRightButtonDown => _mouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;

        public bool IsMiddleButtonDown => _mouse.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;

        public Point ScreenPosition => _mouse.Position.ToPoint();

        public int MouseWheel => _mouse.ScrollWheelValue;
    }
}
