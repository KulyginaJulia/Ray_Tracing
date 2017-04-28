using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.IO;

namespace Ray_tracing
{
    public class Shaders
    {
        public string glVersion;
        public string glslVersion;
        public int BasicProgramID;
        public int BasicVertexShader;
        public int BasicFragmentShader;
        public int vbo_position;
        public Vector3 campos = new Vector3(1.0f,0, 0);
        public int attribute_vpos;
        public int uniform_pos;
        public int uniform_aspect;
        public double aspect;

        public Shaders()
        {
            glVersion = GL.GetString(StringName.Version);
            glslVersion = GL.GetString(StringName.ShadingLanguageVersion);

        }
        public void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        public void InitShaders()
        {
            Vector3[] vertdata = new Vector3[] {
                            new Vector3(-1f, -1f, 0f),
                            new Vector3( 1f, -1f, 0f),
                            new Vector3( 1f, 1f, 0f),
                            new Vector3(-1f, 1f, 0f)
           };
            BasicProgramID = GL.CreateProgram();
            loadShader("..\\..\\ray_trace.vert", ShaderType.VertexShader, BasicProgramID, out BasicVertexShader);
            loadShader("..\\..\\ray_trace.frag", ShaderType.FragmentShader, BasicProgramID, out BasicFragmentShader);
            //Компановка программы
            GL.LinkProgram(BasicProgramID);
            // Проверить успех компановки
            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine("InfoLog:");
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));

            //рисуем квад
            //создали один буфер, связали с атрибутом и заполнили данными
            GL.GenBuffers(1, out vbo_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);
            
            GL.Uniform3(uniform_pos, ref campos);
            GL.Uniform1(uniform_aspect, aspect);

            GL.UseProgram(BasicProgramID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }

}
