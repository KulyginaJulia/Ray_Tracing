#version 130
in vec3 vPosition;
out vec3 glPosition;

 //vec3 org, dir;
void main()
{
    gl_Position = vec4(vPosition, 1.0);
    glPosition = vPosition;
	

    //org = vec3(0, 1, 10); //позиция камеры

    //dir = normalize(vec3(gl_Vertex.x * 1.66667, gl_Vertex.y, -1.0)); //направление
}

