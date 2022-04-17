﻿using System.Drawing;
using Mirage.Utils;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Key = Mirage.Input.Key;

namespace Mirage;

/// <summary>
/// A representation of a <see cref="Window"/>.
/// </summary>
/// <remarks>There should only be one <see cref="Window"/> created ever.</remarks>
public sealed class Window : IDisposable, Boundable
{
    /// <summary>
    /// The width in pixels of the <see cref="Window"/>.
    /// </summary>
    public readonly int Width;
    
    /// <summary>
    /// The height in pixels of the <see cref="Window"/>.
    /// </summary>
    public readonly int Height;
    
    readonly WindowOptions _options;

    IWindow _native;
    readonly Icon _icon;

    /// <summary>
    /// The background color of the <see cref="Window"/>.
    /// </summary>
    public Color Background { get; set; } = Color.Black;

    /// <summary>
    /// The <see cref="Bounds"/> of the <see cref="Window"/>.
    /// </summary>
    public Bounds Bounds => new(Vector2.Zero, new(Width, Height));

    /// <summary>
    /// Creates a <see cref="Window"/>.
    /// </summary>
    /// <param name="title">The title of the <see cref="Window"/>.</param>
    /// <param name="width">The width in pixels of the <see cref="Window"/>.</param>
    /// <param name="height">The height in pixels of the <see cref="Window"/>.</param>
    /// <param name="maximized">Optionally Maximizes the <see cref="Window"/> if true.</param>
    /// <param name="resizable">Optionally Allows the <see cref="Window"/> to be resizable if true.</param>
    /// <param name="background">Optionally provides a background <see cref="Color"/> for the <see cref="Window"/>.</param>
    /// <param name="icon">Optionally provides the path to a custom image file to use for the <see cref="Icon"/>. Defaults to the default <see cref="Icon"/> if null.</param>
    public Window(string title, uint width, uint height, bool maximized = false, bool resizable = true, Color background = default, string icon = null)
    {
        Width = (int) width;
        Height = (int) height;
        _options = WindowOptions.Default;
        _options.Title = title;
        _options.Size = new((int) width, (int) height);
        _options.WindowBorder = resizable ? WindowBorder.Resizable : WindowBorder.Fixed;
        _options.WindowState = maximized ? WindowState.Maximized : WindowState.Normal;
        Background = background;
        _icon = icon is null ? null : new(icon);
    }
    
    /// <summary>
    /// Closes the <see cref="Window"/>.
    /// </summary>
    public void Close() => _native?.Close();
    
    /// <summary>
    /// Creates an OpenGL instance from the <see cref="Window"/>.
    /// </summary>
    internal GL CreateGL() => _native?.CreateOpenGL();

    /// <summary>
    /// Loads the <see cref="Window"/>.
    /// </summary>
    /// <param name="onOpen">Called when the <see cref="Window"/> is first loaded.</param>
    /// <param name="onClose">Called when the <see cref="Window"/> is closed.</param>
    /// <param name="onUpdate">Called every <see cref="Window"/> update tick.</param>
    /// <param name="onRender">Called every <see cref="Window"/> render tick.</param>
    /// <param name="onKeyPress">Called when a <see cref="Key"/> is pressed.</param>
    /// <param name="onKeyRelease">Called when a <see cref="Key"/> is released.</param>
    internal void Load(Action onOpen, Action onClose, Action<float> onUpdate, Action onRender, Action<Key> onKeyPress, Action<Key> onKeyRelease)
    {
        _native = Silk.NET.Windowing.Window.Create(_options);
        _native.Load += () =>
        {
            var input = _native.CreateInput().Keyboards[0];
            input.KeyDown += (_, key, _) => onKeyPress((Key) (int) key);
            input.KeyUp += (_, key, _) => onKeyRelease((Key) (int) key);
            _native.Center();
            if (_icon is not null)
            {
                var raw = _icon.Raw;  
                _native.SetWindowIcon(ref raw);
            }
            onOpen();
        };
        _native.Closing += onClose;
        _native.Update += dt => onUpdate((float) dt);
        _native.Render += _ => onRender();
        _native.Run();
    }
    
    /// <inheritdoc />
    public void Dispose() => _native?.Dispose();
}