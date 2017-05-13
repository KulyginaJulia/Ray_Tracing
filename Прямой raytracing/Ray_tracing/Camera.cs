using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Ray_tracing
{
    public class Camera
    {
        public Vector3 Position = new Vector3(0, 0, -4.9f);
        public Vector3 View = new Vector3(0, 0, 1.0f);
        public Vector3 Up = new Vector3(0, 1.0f, 0);
        public Vector3 Orientation = new Vector3(0f, 0f, 0f);
        public float MoveSpeed = 0.2f;
        public float MouseSensitivity = 0.1f;
        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Identity;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Camera() { }
        public Matrix4 GetViewMatrix()
        {
            /**This code uses some trigonometry to create a vector in the direction that the camera is looking,
             * and then uses the LookAt static function of the Matrix4 class to use that vector and
             * the position to create a view matrix we can use to change where our scene is viewed from. 
             * The Vector3.UnitY is being assigned to the "up" parameter,
             * which will keep our camera angle so that the right side is up.*/
            Vector3 lookat = new Vector3();

            lookat.X = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Y));
            lookat.Y = (float)Math.Sin((float)Orientation.Y);
            lookat.Z = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Y));

            Math.Round(lookat.X, 3);
            Math.Round(lookat.Y, 3);
            Math.Round(lookat.Z, 3);
            // Matrix4 V = Matrix4.LookAt(Vector3(0, 0, -4.9), vec3(0, 0, 0), vec3(0, 1, 0));
            return Matrix4.LookAt(Position, Position + lookat, Vector3.UnitY);
           // return Matrix4.LookAt(Position, View, Up);
        }

        public void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }

        public void CalculateModelMatrix()
        {
             ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
            //ModelMatrix = Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z);
        }


    }
}
