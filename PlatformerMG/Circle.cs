#region File Description
//-----------------------------------------------------------------------------
// Circle.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;

namespace TexasJames
{
    /// <summary>
    /// Represents a 2D circle.
    /// </summary>
    public class Circle
    {
        /// <summary>
        /// Center position of the circle.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Radius of the circle.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Constructs a new circle.
        /// </summary>
        public Circle(Vector2 position, float radius)
        {
            Center = position;
            Radius = radius;
        }

        /// <summary>
        /// Determines if a circle intersects a rectangle.
        /// </summary>
        /// <returns>True if the circle and rectangle overlap. False otherwise.</returns>
        public bool Intersects(Rectangle rectangle)
        {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                                    MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            Vector2 direction = Center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }

        public bool Intersects(Circle circle)
        {
            float distance = (float) Math.Sqrt((circle.Center.X - this.Center.X)* (circle.Center.X - this.Center.X)+
                (circle.Center.Y - this.Center.Y)* (circle.Center.Y - this.Center.Y));

            if(distance < circle.Radius+ this.Radius)
                return true;

            return false;
        }
    }
}
