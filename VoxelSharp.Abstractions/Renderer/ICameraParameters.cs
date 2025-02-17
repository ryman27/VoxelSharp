﻿using System.Numerics;

namespace VoxelSharp.Abstractions.Renderer;

public interface ICameraParameters
{
    public enum CameraType
    {
        Perspective,
        Orthographic
    }

    Vector3 Position { get; }

    Vector3 Rotation { get; }

    float FieldOfView { get; }
    float NearClip { get; }
    float FarClip { get; }
    float AspectRatio { get; }

    CameraType Camera { get; }
}