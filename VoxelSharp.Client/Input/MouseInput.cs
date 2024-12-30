﻿using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Client.Input;

using System;
using System.Runtime.InteropServices;

public partial class MouseInput : IUpdatable
{
    private Point _lastMousePosition;
    private bool _isTracking;
    private IntPtr _windowHandle;

    public double X { get; private set; }
    public double Y { get; private set; }

    public void StartTracking(IntPtr windowHandle)
    {
        if (_isTracking) return;

        _isTracking = true;
        _windowHandle = windowHandle;

        // Lock the cursor to the client area of the window
        ClipCursorToWindow(windowHandle);

        // Hide the cursor
        SetCursorVisibility(false);

        // Reset cursor position to the centre
        ResetCursorToCenter();
    }

    public void StopTracking()
    {
        _isTracking = false;

        // Unlock the cursor
        ClipCursor(IntPtr.Zero);

        // Show the cursor
        SetCursorVisibility(true);
    }

    public void Update(double deltaTime)
    {
        if (!_isTracking) return;

        if (!GetCursorPos(out var currentMousePosition)) return;

        // Calculate relative movement
        var deltaX = currentMousePosition.X - _lastMousePosition.X;
        var deltaY = currentMousePosition.Y - _lastMousePosition.Y;

        // Trigger relative movement updates
        (X, Y) = (deltaX, deltaY);

        // Reset the cursor position to the centre of the window
        ResetCursorToCenter();
    }

    private void ResetCursorToCenter()
    {
        if (_windowHandle == IntPtr.Zero) return;

        if (GetClientRect(_windowHandle, out var rect))
        {
            var screenPoint = new Point();
            ClientToScreen(_windowHandle, ref screenPoint);

            // Calculate the centre of the window
            int centreX = screenPoint.X + (rect.Right - rect.Left) / 2;
            int centreY = screenPoint.Y + (rect.Bottom - rect.Top) / 2;

            // Set the cursor to the centre
            SetCursorPos(centreX, centreY);

            // Update the last mouse position to the new centre
            _lastMousePosition = new Point { X = centreX, Y = centreY };
        }
    }

    public static void SetCursorVisibility(bool visible)
    {
        ShowCursor(visible);
    }

    private static void ClipCursorToWindow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero) return;

        // Get the client area of the window
        if (GetClientRect(windowHandle, out var rect))
        {
            var screenPoint = new Point();
            ClientToScreen(windowHandle, ref screenPoint);

            var clipRect = new Rect
            {
                Left = screenPoint.X + rect.Left,
                Top = screenPoint.Y + rect.Top,
                Right = screenPoint.X + rect.Right,
                Bottom = screenPoint.Y + rect.Bottom
            };

            ClipCursor(ref clipRect);
        }
    }

    // Import GetCursorPos from User32.dll
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out Point lpPoint);

    // Import SetCursorPos from User32.dll
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetCursorPos(int X, int Y);

    // Import ClipCursor from User32.dll
    [LibraryImport("user32.dll")]
    private static partial void ClipCursor(ref Rect lpRect);

    // Overload for unlocking cursor
    [LibraryImport("user32.dll")]
    private static partial void ClipCursor(IntPtr lpRect);

    // Import ShowCursor from User32.dll
    [LibraryImport("user32.dll")]
    private static partial int ShowCursor([MarshalAs(UnmanagedType.Bool)] bool bShow);

    // Import GetClientRect from User32.dll
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetClientRect(IntPtr hWnd, out Rect lpRect);

    // Import ClientToScreen from User32.dll
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    // Define a RECT structure for clipping region
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // Define a POINT structure for cursor position
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }
}