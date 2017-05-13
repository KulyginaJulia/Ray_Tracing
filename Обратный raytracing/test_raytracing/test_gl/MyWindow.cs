using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using System.Drawing;

namespace test_gl {
  sealed class MyWindow : OpenTK.GameWindow {

    State myState;
    float global_time = 0;

    DateTime next_fps = DateTime.Now.AddSeconds(1);
    float fps = 0;

    Vector3 camera_pos = new Vector3(0, 0, -8);
    float cur_x, cur_y;

    float a1 = 0f, a2 = 0f;

    static int w = 1280;
    static int h = 720;

    int p_cur_x = w / 2, p_cur_y = h / 2;
    float distance = 1;

    Vector3 camera_view = new Vector3(0, 0, 1);
    Vector3 camera_up = new Vector3(0, 1, 0);
    Vector3 camera_side = new Vector3(1, 0, 0);

    public MyWindow()
      : base(w, h, GraphicsMode.Default, "OpenTK Intro",
             GameWindowFlags.Default, DisplayDevice.Default,
             3, 0, GraphicsContextFlags.ForwardCompatible) {
      Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
      Console.WriteLine("glsl version: " + GL.GetString(StringName.ShadingLanguageVersion));
      myState = new State("raytracing");
      myState.initData();
            p_cur_x = Bounds.Width / 2 - 8;
            p_cur_y = Bounds.Height / 2 - 31;

            CursorVisible = false;
    }

    protected override void OnResize(EventArgs e) {
      w = this.Width;
      h = this.Height;
      GL.Viewport(0, 0, this.Width, this.Height);
    }


    void updateMouse() {
      cur_x = (float)(Mouse.X - p_cur_x) / (w / 2);
      cur_y = (float)(Mouse.Y - p_cur_y) / (h / 2);

      a2 -= (float)Math.Atan2(cur_y, distance);

      a1 += (float)Math.Atan2(cur_x, distance);

      OpenTK.Input.Mouse.SetPosition(Bounds.X + Bounds.Width/2, Bounds.Y + Bounds.Height/2);
            ///Console.WriteLine("{0:F3},{1:F3}", Bounds.Width / 2, Bounds.Height / 2);
            ///Console.WriteLine("{0:F3},{1:F3}", Mouse.X, Mouse.Y);

            Vector3 lookat = new Vector3();

      lookat.X = (float)(Math.Sin(a1) * Math.Cos(a2));
      lookat.Y = (float)(Math.Sin(a2));
      lookat.Z = (float)(Math.Cos(a1) * Math.Cos(a2));

      Matrix4 view = Matrix4.LookAt(camera_pos, camera_pos + lookat, Vector3.UnitY);

      camera_side = -view.Column0.Xyz;
      camera_up   = view.Column1.Xyz;
      camera_view = -view.Column2.Xyz;
    }



    protected override void OnUpdateFrame(FrameEventArgs e) {
      updateMouse();

      //GL.Uniform1(myState.uniform_aspect, 1.0f);
      //GL.Uniform1(myState.uniform_time, global_time);
      GL.Uniform3(myState.uniform_pos, camera_pos);
      GL.Uniform3(myState.uniform_view, camera_view);
      GL.Uniform3(myState.uniform_up, camera_up);
      GL.Uniform3(myState.uniform_side, camera_side);
      //global_time += 0.001f;

      if (DateTime.Now >= next_fps) {
        this.Title = String.Format("FPS = {0:F3}", fps);
        next_fps = DateTime.Now.AddSeconds(1);
        fps = 0;
      }

      fps++;
    }

    protected override void OnKeyPress(OpenTK.KeyPressEventArgs e) {
      switch (e.KeyChar) {
        case 'a':
          camera_pos -= Vector3.Multiply(camera_side, 0.1f);
          break;
        case 'd':
          camera_pos += Vector3.Multiply(camera_side, 0.1f);
          break;
        case 'w':
          camera_pos += Vector3.Multiply(camera_view, 0.1f);
          break;
        case 's':
          camera_pos -= Vector3.Multiply(camera_view, 0.1f);
          break;
        case 'r':
          camera_pos += Vector3.Multiply(camera_up, 0.1f);
          break;
        case 'f':
          camera_pos -= Vector3.Multiply(camera_up, 0.1f);
          break;
        case 'u':
          System.Environment.Exit(-1);
          break;
      }
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
      GL.ClearColor(Color4.Purple);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      GL.DrawArrays(PrimitiveType.Quads, 0, 4);

      this.SwapBuffers();
    }

    public static void Main() {
      MyWindow mw = new MyWindow();
      mw.Run();
    }
  }
}

public class State {
  int vbo_position;
  int attribute_vpos = 0;
  public int uniform_pos;
  public int uniform_view;
  public int uniform_up;
  public int uniform_side;
  public int uniform_aspect;
  public int uniform_time;
  double aspect = 1;


  int VERTEX_SHADER_ID;
  int FRAGMENT_SHDER_ID;
  int PROGRAM_ID;

  public State(String folder) {
    PROGRAM_ID = GL.CreateProgram(); // создание объекта программы
    folder = "../../../shaders/" + folder + "/";
    loadShader(folder + "vert.cpp", ShaderType.VertexShader,
      PROGRAM_ID,
      out VERTEX_SHADER_ID);
    loadShader(folder + "frag.cpp", ShaderType.FragmentShader, PROGRAM_ID,
      out FRAGMENT_SHDER_ID);
    GL.LinkProgram(PROGRAM_ID);

    int status = 0;
    GL.GetProgram(PROGRAM_ID, GetProgramParameterName.LinkStatus, out status);
    Console.WriteLine(GL.GetProgramInfoLog(PROGRAM_ID));
  }

  public void initData() {
    Vector3[] vertdata = new Vector3[] {
			new Vector3(-1f, -1f, 0f),
			new Vector3( 1f, -1f, 0f),
			new Vector3( 1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f) };
    GL.GenBuffers(1, out vbo_position);
    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
    GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                            (IntPtr)(vertdata.Length * Vector3.SizeInBytes),
                            vertdata,
                            BufferUsageHint.StaticDraw);

    GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);
    GL.EnableVertexAttribArray(attribute_vpos);

   // uniform_aspect = GL.GetUniformLocation(PROGRAM_ID, "aspect");
   // uniform_time = GL.GetUniformLocation(PROGRAM_ID, "time");
    uniform_pos = GL.GetUniformLocation(PROGRAM_ID, "campos");
    uniform_view = GL.GetUniformLocation(PROGRAM_ID, "view");
    uniform_up = GL.GetUniformLocation(PROGRAM_ID, "up");
    uniform_side = GL.GetUniformLocation(PROGRAM_ID, "side");

    GL.UseProgram(PROGRAM_ID);
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
  }

  public static void loadShader(String filename, ShaderType type, int program, out int address) {
    address = GL.CreateShader(type);
    using (System.IO.StreamReader sr = new System.IO.StreamReader(filename)) {
      GL.ShaderSource(address, sr.ReadToEnd());
    }
    GL.CompileShader(address);
    GL.AttachShader(program, address);
    Console.WriteLine(GL.GetShaderInfoLog(address));
  }
}
