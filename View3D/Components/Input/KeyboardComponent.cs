﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Text;
using View3D.Components;

namespace View3D.Components.Input
{
    public delegate void KeybordButtonReleasedDelegate(Keys key);

    public class KeyboardComponent : BaseComponent
    {
        public event KeybordButtonReleasedDelegate KeybordButtonReleased;

        KeyboardState _currentKeyboardState;
        KeyboardState _lastKeyboardState;

        WpfKeyboard _wpfKeyboard;

        public KeyboardComponent(WpfGame game) : base(game)
        {
            _wpfKeyboard = new WpfKeyboard(Game);
            UpdateOrder = (int)ComponentUpdateOrderEnum.Input;
        }

        public void Reset()
        {
            _currentKeyboardState = _wpfKeyboard.GetState();
            _lastKeyboardState = _wpfKeyboard.GetState();
        }

        public override void Update(GameTime t)
        {
            

            var keyboardState = _wpfKeyboard.GetState();

            _lastKeyboardState = _currentKeyboardState;
            _currentKeyboardState = keyboardState;

            if (_lastKeyboardState == null)
                _lastKeyboardState = keyboardState;

            foreach (var key in _lastKeyboardState.GetPressedKeys())
            {
                if (IsKeyUp(key))
                    KeybordButtonReleased?.Invoke(key);
            }
        }

        public bool IsKeyReleased(Keys key)
        {
            var currentUp = _currentKeyboardState.IsKeyUp(key);
            var lastDown = _lastKeyboardState.IsKeyDown(key);
            return currentUp && lastDown;
        }

        public bool IsKeyComboReleased(Keys key, Keys modificationKey, bool consumeIfTrue = true)
        {
            var value = (IsKeyReleased(key) && IsKeyDownOrReleased(modificationKey));
            return value;
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyDownOrReleased(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) || IsKeyReleased(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return _currentKeyboardState.IsKeyUp(key);
        }
    }
}