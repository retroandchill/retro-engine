// // @file SimpleFlipbook.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Core.Math;
using RetroEngine.Tickables;
using RetroEngine.World;

namespace RetroEngine.Game.Sample;

public sealed class SimpleFlipbook : ITickable, IDisposable
{
    public bool TickEnabled => !_disposed;

    private bool _disposed;

    private readonly TickHandle _tickHandle;
    private readonly Sprite _sprite;
    private readonly Texture _texture;
    private readonly float _frameRate;
    private readonly float _frameTime;
    private float _timeSinceLastFrame;
    private int _currentFrame;
    private readonly int _frameCount;

    public Vector2F Scale
    {
        get => _sprite.Scale;
        set => _sprite.Scale = value;
    }

    public SimpleFlipbook(Scene scene, Texture texture, float frameRate)
    {
        _tickHandle = new TickHandle(this);
        _frameRate = frameRate;
        _frameTime = 1f / frameRate;
        _frameCount = texture.Width / texture.Height;
        _sprite = new Sprite(scene)
        {
            UVs = new UVs(Vector2F.Zero, new Vector2F((float)(texture.Height - 1) / texture.Width, 1)),
            Pivot = new Vector2F(0.5f, 0.5f),
            Texture = texture,
            Size = new Vector2F(texture.Height, texture.Height),
        };
        _texture = texture;
    }

    public void Tick(float deltaTime)
    {
        _timeSinceLastFrame += deltaTime;
        if (_timeSinceLastFrame < _frameTime)
            return;

        var framesPassed = (int)(_timeSinceLastFrame / _frameTime);
        _currentFrame = (_currentFrame + framesPassed) % _frameCount;

        var insetPx = 0.5f / _texture.Width;
        var startPixel = _currentFrame * _texture.Height;
        var endPixel = startPixel + _texture.Height - 1;

        _sprite.UVs = new UVs(
            new Vector2F((float)startPixel / _texture.Width + insetPx, 0),
            new Vector2F((float)endPixel / _texture.Width - insetPx, 1)
        );
        _timeSinceLastFrame -= framesPassed * _frameTime;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _tickHandle.Dispose();
        _sprite.Dispose();
        _disposed = true;
    }
}
