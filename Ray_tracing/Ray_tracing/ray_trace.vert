﻿in vec3 vPosition;
varying out vec3 glPosition;

void main (void)
{
	gl_Position = vec4(vPosition, 1.0);
	glPosition = vPosition;
}