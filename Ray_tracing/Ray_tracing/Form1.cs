using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Drawing;
using OpenTK.Graphics.OpenGL;


namespace Ray_tracing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            glControl1.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            Shaders m = new Shaders();
          
            Console.WriteLine(m.glslVersion);
            Console.WriteLine(m.glVersion);
            m.InitShaders();
            //GL.UseProgram(m.BasicProgramID);

            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.EnableVertexAttribArray(m.attribute_vpos);
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);


            GL.EnableVertexAttribArray(m.attribute_vpos);


            Console.WriteLine("OK");
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            Console.WriteLine("OK");

            GL.DisableVertexAttribArray(m.attribute_vpos);

            glControl1.SwapBuffers();
            Console.WriteLine("OK");

            GL.UseProgram(0);
        }
    }
}
