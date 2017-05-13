using System;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Ray_tracing
{
    public partial class Form1 : Form
    {
        //int FrameCount;
       // DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        
      

        //void displayFPS()
        //{
        //    if (DateTime.Now >= NextFPSUpdate)
        //    {
        //        this.Text = String.Format("Ray Tracing (fps = {0})", FrameCount);
        //        NextFPSUpdate = DateTime.Now.AddSeconds(1);
        //        FrameCount = 0;
        //    }
        //    FrameCount++;
        //}
        public Form1()
        {
            InitializeComponent();
            glControl1.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Application.Idle += Application_Idle;
        }
        //void Application_Idle(object sender, EventArgs e)
        //{
        //    while (glControl1.IsIdle)
        //    {
        //        displayFPS();
        //        glControl1.Invalidate();
        //    }
        //}
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            
            Shaders m = new Shaders();
          
            Console.WriteLine(m.glslVersion);
            Console.WriteLine(m.glVersion);

            m.InitShaders(glControl1.Width / (float)glControl1.Height);

            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);


            GL.EnableVertexAttribArray(m.attribute_vpos);
           

            Console.WriteLine("OK");
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);


            GL.DisableVertexAttribArray(m.attribute_vpos);

            glControl1.SwapBuffers();
            GL.UseProgram(0);
        }
    }
}
