using System;
using System.Collections;
using System.Collections.Generic;

namespace AillieoUtils
{
    public struct Vector2 : IEquatable<Vector2>
    {
        public enum Axis
        {
            X = 0,
            Y = 1,
        }
        
        private static readonly Vector2 zeroVector = new Vector2(0.0f, 0.0f);
        private static readonly Vector2 oneVector = new Vector2(1f, 1f);
        private static readonly Vector2 upVector = new Vector2(0.0f, 1f);
        private static readonly Vector2 downVector = new Vector2(0.0f, -1f);
        private static readonly Vector2 leftVector = new Vector2(-1f, 0.0f);
        private static readonly Vector2 rightVector = new Vector2(1f, 0.0f);

        private static readonly Vector2 positiveInfinityVector =
            new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        private static readonly Vector2 negativeInfinityVector =
            new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        public float x;
        public float y;

        public const float kEpsilon = 1E-05f;
        public const float kEpsilonNormalSqrt = 1E-15f;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float this[Axis axis]
        {
            get
            {
                switch(axis)
                {
                    case Axis.X:
                        return x;
                    case Axis.Y:
                        return y;
                }
                throw new IndexOutOfRangeException("Invalid axis!");
            }
            set
            {
                switch(axis)
                {
                    case Axis.X:
                        x = value;
                        return;
                    case Axis.Y:
                        y = value;
                        return;
                }
                throw new IndexOutOfRangeException("Invalid axis!");
            }
        }
        
        public void Normalize()
        {
            float mag = magnitude;
            if (mag > kEpsilon)
                this = this / mag;
            else
                this = zero;
        }

        public Vector2 normalized
        {
            get
            {
                Vector2 v = new Vector2(x, y);
                v.Normalize();
                return v;
            }
        }

        public override string ToString()
        {
            return string.Format("({0:F2}, {1:F2})", (object)this.x, (object)this.y);
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
        }

        public override bool Equals(object other)
        {
            return other is Vector2 other1 && this.Equals(other1);
        }

        public bool Equals(Vector2 other)
        {
            return this.x.Equals(other.x) && this.y.Equals(other.y);
        }

        public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal)
        {
            return -2f * Vector2.Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static Vector2 Perpendicular(Vector2 inDirection)
        {
            return new Vector2(-inDirection.y, inDirection.x);
        }

        public static float Dot(Vector2 lhs, Vector2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y); } }

        public float sqrMagnitude { get { return x * x + y * y; } }


        public static float Distance(Vector2 a, Vector2 b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;
            return (float)Math.Sqrt(diff_x * diff_x + diff_y * diff_y);
        }

        public static float SqrMagnitude(Vector2 a)
        {
            return a.x * a.x + a.y * a.y;
        }

        public float SqrMagnitude()
        {
            return this.x * this.x + this.y * this.y;
        }

        public static Vector2 Min(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y));
        }

        public static Vector2 Max(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y));
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }

        public static Vector2 operator *(Vector2 a, float d)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator *(float d, Vector2 a)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator /(Vector2 a, float d)
        {
            return new Vector2(a.x / d, a.y / d);
        }

        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            // Returns false in the presence of NaN values.
            float diff_x = lhs.x - rhs.x;
            float diff_y = lhs.y - rhs.y;
            return (diff_x * diff_x + diff_y * diff_y) < kEpsilon * kEpsilon;
        }

        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return !(lhs == rhs);
        }

        public static Vector2 zero
        {
            get
            {
                return Vector2.zeroVector;
            }
        }

        public static Vector2 one
        {
            get
            {
                return Vector2.oneVector;
            }
        }

        public static Vector2 up
        {
            get
            {
                return Vector2.upVector;
            }
        }

        public static Vector2 down
        {
            get
            {
                return Vector2.downVector;
            }
        }

        public static Vector2 left
        {
            get
            {
                return Vector2.leftVector;
            }
        }

        public static Vector2 right
        {
            get
            {
                return Vector2.rightVector;
            }
        }

        public static Vector2 positiveInfinity
        {
            get
            {
                return Vector2.positiveInfinityVector;
            }
        }

        public static Vector2 negativeInfinity
        {
            get
            {
                return Vector2.negativeInfinityVector;
            }
        }
    }
}
