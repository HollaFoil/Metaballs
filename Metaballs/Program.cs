using System;
using System.Diagnostics;
using GLFW;
using Metaballs;
using static Metaballs.GL;

class Program
{
    static Stopwatch sw = new Stopwatch();
    static int sizeX = 950;
    static int sizeY = 600;
    static uint bufferSize = 0;
    /// <summary>
    /// Obligatory name for your first OpenGL example program.
    /// </summary>
    private const string TITLE = "Metaballs";

    
    static void Main(string[] args)
    {
        // Set context creation hints
        PrepareContext();
        // Create a window and shader program
        var window = CreateWindow(sizeX, sizeY);
        var program = CreateProgram();

        rand = new Random();
        CreateGrid(6, out Grid grid);

        

        var location = glGetUniformLocation(program, "color");
        SetRandomColor(location);
        long n = 0;
        glLineWidth(3.5f);
        glEnable(GL_LINE_SMOOTH);
        glEnable(GL_BLEND);
        glHint(GL_LINE_SMOOTH_HINT, GL_NICEST);

        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        sw.Start();
        grid.CalculateThreshold(sizeX, sizeY);
        CreateVertices(grid.GetVerticesMarchingSquares(sizeX, sizeY), out var vao, out var vbo, out bufferSize);
        while (!Glfw.WindowShouldClose(window))
        {
            
            Glfw.PollEvents();
            if (sw.ElapsedMilliseconds < 30) continue;
            Glfw.GetWindowSize(window, out int width, out int height);
            if (width == 0 || height == 0) continue;
            if (width != sizeX || height != sizeY)
            {
                var screen = Glfw.PrimaryMonitor.WorkArea;
                var x = (screen.Width - width) / 2;
                var y = (screen.Height - height) / 2;
                glViewport(0, 0, width, height);
                grid.BringBackSources(width, height, sizeX, sizeY);
                grid.CalculateThreshold(width, height);
                sizeY = height;
                sizeX = width;
            }
            
            grid.UpdateSources(sw.ElapsedMilliseconds / 40.0f, sizeX, sizeY);
            sw.Restart();
            // Swap fore/back framebuffers, and poll for operating system events.
            Glfw.SwapBuffers(window);
            // Clear the framebuffer to defined background color
            glClear(GL_COLOR_BUFFER_BIT);

            //SetRandomColor(location);

            UpdateBuffer(grid.GetVerticesMarchingSquares(sizeX,sizeY), ref vao, ref vbo);

            glDrawArrays(GL_LINES, 0, grid.count);
        }

        Glfw.Terminate();
    }
    
    private static void CreateGrid(int sourceCount, out Grid grid)
    {
        Source[] sources = new Source[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            float scale = (float)(rand.NextDouble() + 0.5) / 2;
            float[] loc = { (float)(rand.NextDouble() * 2 - 1)*sizeX, (float)(rand.NextDouble() * 2 - 1)* sizeY };
            float[] dir = { (float)(rand.NextDouble() * 2 - 1) / 150, (float)(rand.NextDouble() * 2 - 1) / 150 };
            sources[i] = new Source(loc, dir, scale);
        }
        grid = new Grid(sources, 50f, 0.025f);
    }
    private static void SetRandomColor(int location)
    {
        var r = (float)rand.NextDouble();
        var g = (float)rand.NextDouble();
        var b = (float)rand.NextDouble();
        glUniform3f(location, r, g, b);
    }

    private static void PrepareContext()
    {
        // Set some common hints for the OpenGL profile creation
        Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Doublebuffer, true);
        Glfw.WindowHint(Hint.Decorated, true);
    }

    /// <summary>
    /// Creates and returns a handle to a GLFW window with a current OpenGL context.
    /// </summary>
    /// <param name="width">The width of the client area, in pixels.</param>
    /// <param name="height">The height of the client area, in pixels.</param>
    /// <returns>A handle to the created window.</returns>
    private static Window CreateWindow(int width, int height)
    {
        // Create window, make the OpenGL context current on the thread, and import graphics functions
        var window = Glfw.CreateWindow(width, height, TITLE, GLFW.Monitor.None, Window.None);

        // Center window
        var screen = Glfw.PrimaryMonitor.WorkArea;
        var x = (screen.Width - width) / 2;
        var y = (screen.Height - height) / 2;
        Glfw.SetWindowPosition(window, x, y);

        Glfw.MakeContextCurrent(window);
        Import(Glfw.GetProcAddress);



        return window;
    }

    /// <summary>
    /// Creates an extremely basic shader program that is capable of displaying a triangle on screen.
    /// </summary>
    /// <returns>The created shader program. No error checking is performed for this basic example.</returns>
    private unsafe static uint CreateProgram()
    {
        var vertex = CreateShader(GL_VERTEX_SHADER, @"#version 330 core
                                                    layout (location = 0) in vec2 pos;
                                                    out vec2 pos2;
                                                    void main()
                                                    {
                                                        gl_Position = vec4(pos.x, pos.y, 0.0, 1.0);
                                                        pos2 = pos;
                                                    }");
        int success = 0;
        glGetShaderiv(vertex, GL_COMPILE_STATUS, &success);
        if (success == 0)
        {
            Console.Write("ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" + glGetShaderInfoLog(vertex));
        }
        var fragment = CreateShader(GL_FRAGMENT_SHADER, @"#version 330 core
                                                        out vec4 result;
                                                        in vec2 pos2;
                                                        uniform vec3 color;

                                                        void main()
                                                        {
                                                            result = vec4(pos2, 1.0, 1.0);
                                                        } ");
        success = 0;
        glGetShaderiv(fragment, GL_COMPILE_STATUS, &success);
        if (success == 0)
        {
            Console.Write("ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" + glGetShaderInfoLog(fragment));
        }

        var program = glCreateProgram();
        glAttachShader(program, vertex);
        glAttachShader(program, fragment);

        glLinkProgram(program);

        glDeleteShader(vertex);
        glDeleteShader(fragment);

        glUseProgram(program);
        return program;
    }

    /// <summary>
    /// Creates a shader of the specified type from the given source string.
    /// </summary>
    /// <param name="type">An OpenGL enum for the shader type.</param>
    /// <param name="source">The source code of the shader.</param>
    /// <returns>The created shader. No error checking is performed for this basic example.</returns>
    private static uint CreateShader(int type, string source)
    {
        var shader = glCreateShader(type);
        glShaderSource(shader, source);
        glCompileShader(shader);
        return shader;
    }

    /// <summary>
    /// Creates a VBO and VAO to store the vertices for a triangle.
    /// </summary>
    /// <param name="vao">The created vertex array object for the triangle.</param>
    /// <param name="vbo">The created vertex buffer object for the triangle.</param>
    private static unsafe void CreateVertices(List<float[]> vert, out uint vao, out uint vbo, out uint bufferSize)
    {

        ListToVertArray(vert, out float[] vertices);
        //Console.WriteLine("Vertices: " + vertices.Length.ToString());
        //foreach (var vertex in vertices) Console.WriteLine(vertex);
        vao = glGenVertexArray();
        vbo = glGenBuffer();

        glBindVertexArray(vao);

        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
        }

        glVertexAttribPointer(0, 2, GL_FLOAT, true, 2 * sizeof(float), NULL);
        glEnableVertexAttribArray(0);
        bufferSize = (uint)(sizeof(float) * vertices.Length);
    }
    private static unsafe void UpdateBuffer(List<float[]> vert, ref uint vao, ref uint vbo)
    {
        ListToVertArray(vert, out float[] vertices);
        if (vertices.Length*sizeof(float) > bufferSize)
        {
            CreateVertices(vert, out vao, out vbo, out bufferSize);
            return;
        }
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        glClearBufferfi(GL_ARRAY_BUFFER, 0, bufferSize, 0);
        fixed (float* v = &vertices[0])
        {
            glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(float) * vertices.Length, v);
        }
        glVertexAttribPointer(0, 2, GL_FLOAT, true, 2 * sizeof(float), NULL);
        glEnableVertexAttribArray(0);
    }
    private static void ListToVertArray(List<float[]> l, out float[] vertices)
    {
        vertices = new float[l.Count * 2];
        for (int i = 0; i < l.Count; i++)
        {
            vertices[i * 2] = l[i][0]/(float)sizeX;
            vertices[i * 2 + 1] = l[i][1]/(float)sizeY;
            //vertices[i * 3+2] = 0.0f;
        }
    }
    private static Random rand;
}
