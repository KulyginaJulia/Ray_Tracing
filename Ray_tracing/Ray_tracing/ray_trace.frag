#version 330
#define EPSILON  0.001
#define BIG  1000000.0
#define STACK_MAX_SIZE 20

const int DIFFUSE = 1;
const int REFLECTION = 2;
const int REFRACTION = 3;


struct SSphere
{
	 vec3 Center;
	 float Radius;
	 int MaterialIdx;
};
struct STriangle
{
	vec3 v1;
	vec3 v2;
	vec3 v3;
	int MaterialIdx;
};
struct SLight
{
	vec3 Position;
};
out vec4 FragColor;
in vec3 glPosition;

/*** DATA STRUCTURES ***/
struct SCamera
{
	vec3 Position;
	vec3 View;
	vec3 Up;
	vec3 Side;
	vec2 Scale;
};
struct SRay
{
	vec3 Origin;
	vec3 Direction;
};
struct SIntersection
{
	float Time;
	vec3 Point;
	vec3 Normal;
	vec3 Color;

	vec4 LightCoeffs;

	float ReflectionCoef;
	float RefractionCoef;
	int MaterialType;
};
struct SMaterial
{
 //diffuse color
	vec3 Color;
 // ambient, diffuse and specular coeffs
	vec4 LightCoeffs;
 // 0 - non-reflection, 1 - mirror
	float ReflectionCoef;
	float RefractionCoef;
	int MaterialType;
};
const int DIFFUSE_REFLECTION = 1;
const int MIRROR_REFLECTION = 2;

struct STracingRay
{
	SRay ray;
	float contribution;
	int depth;
};
//**Stack of SRay **//
struct Stack_t {
    STracingRay data[STACK_MAX_SIZE];
    int size;
};

void pushRay(Stack_t stack, const STracingRay value) 
{
	if (stack.size >= STACK_MAX_SIZE - 1)
		discard;
    stack.data[stack.size] = value;
    stack.size++;
}
STracingRay popRay(Stack_t stack)
{
	if (stack.size < 0)
		discard;
	stack.size--;
	STracingRay temp = stack.data[stack.size];
    return temp;
}
bool isEmpty(Stack_t stack)
{
	if (stack.size == 0)
		return true;
	else 
		return false;
}
void initializeDefaultScene( out STriangle triangles[10],  out SSphere spheres[2])
{
	/** TRIANGLES **/
	/* left wall */
	triangles[0].v1 = vec3(-5.0,-5.0,-5.0);
	triangles[0].v2 = vec3(-5.0, 5.0, 5.0);
	triangles[0].v3 = vec3(-5.0, 5.0,-5.0);
	triangles[0].MaterialIdx = 0;
	triangles[1].v1 = vec3(-5.0,-5.0,-5.0);
	triangles[1].v2 = vec3(-5.0,-5.0, 5.0);
	triangles[1].v3 = vec3(-5.0, 5.0, 5.0);
	triangles[1].MaterialIdx = 0;
	/* back wall */
	triangles[2].v1 = vec3(-5.0,-5.0, 5.0);
	triangles[2].v2 = vec3( 5.0,-5.0, 5.0);
	triangles[2].v3 = vec3(-5.0, 5.0, 5.0);
	triangles[2].MaterialIdx = 0;
	triangles[3].v1 = vec3( 5.0, 5.0, 5.0);
	triangles[3].v2 = vec3(-5.0, 5.0, 5.0);
	triangles[3].v3 = vec3( 5.0,-5.0, 5.0);
	triangles[3].MaterialIdx = 0;
	/*right wall */
	triangles[4].v1 = vec3(5.0, 5.0, 5.0);
	triangles[4].v2 = vec3(5.0, -5.0, 5.0);
	triangles[4].v3 = vec3(5.0, 5.0, -5.0);
	triangles[4].MaterialIdx = 0;
	triangles[5].v1 = vec3(5.0, -5.0, 5.0);
	triangles[5].v2 = vec3(5.0, 5.0, -5.0);
	triangles[5].v3 = vec3(5.0, -5.0, -5.0);
	triangles[5].MaterialIdx = 0;
	/*down wall */
	triangles[6].v1 = vec3(-5.0,-5.0,-5.0);
	triangles[6].v2 = vec3(-5.0,-5.0, 5.0);
	triangles[6].v3 = vec3( 5.0,-5.0, 5.0);
	triangles[6].MaterialIdx = 0;
	triangles[7].v1 = vec3( 5.0,-5.0, 5.0);
	triangles[7].v2 = vec3(5.0, -5.0, -5.0);
	triangles[7].v3 = vec3(-5.0,-5.0,-5.0);
	triangles[7].MaterialIdx = 0;
	/*up wall */
	triangles[8].v1 = vec3(-5.0, 5.0, 5.0);
	triangles[8].v2 = vec3(-5.0, 5.0,-5.0);
	triangles[8].v3 = vec3( 5.0, 5.0, 5.0);
	triangles[8].MaterialIdx = 0;
	triangles[9].v1 = vec3(-5.0, 5.0, -5.0);
	triangles[9].v2 = vec3( 5.0, 5.0, 5.0);
	triangles[9].v3 = vec3(5.0, 5.0, -5.0);
	triangles[9].MaterialIdx = 0;
	
	/** SPHERES **/
	spheres[0].Center = vec3(-1.0,-1.0,-2.0);
	spheres[0].Radius = 2.0;
	spheres[0].MaterialIdx = 1;
	spheres[1].Center = vec3(2.0,1.0,2.0);
	spheres[1].Radius = 1.0;
	spheres[1].MaterialIdx = 1;
}

SRay GenerateRay ( SCamera uCamera )
{
	vec2 coords = glPosition.xy * uCamera.Scale;
	vec3 direction = uCamera.View + uCamera.Side * coords.x + uCamera.Up * coords.y;
	return SRay ( uCamera.Position, normalize(direction) );
}
SCamera initializeDefaultCamera()
{
 //** CAMERA **//
	SCamera camera;
	camera.Position = vec3(0.0, 0.0, -8.0);
	camera.View = vec3(0.0, 0.0, 1.0);
	camera.Up = vec3(0.0, 1.0, 0.0);
	camera.Side = vec3(1.0, 0.0, 0.0);
	camera.Scale = vec2(1.5, 2.0);
	return camera;
}

/*Intersection */
bool IntersectSphere ( SSphere sphere, SRay ray, float start, float final, out float time )
{
	ray.Origin -= sphere.Center;
	float A = dot ( ray.Direction, ray.Direction );
	float B = dot ( ray.Direction, ray.Origin );
	float C = dot ( ray.Origin, ray.Origin ) - sphere.Radius * sphere.Radius;
	float D = B * B - A * C;
	if ( D > 0.0 )
	{
		D = sqrt ( D );
		//time = min ( max ( 0.0, ( -B - D ) / A ), ( -B + D ) / A );
		float t1 = ( -B - D ) / A;
		float t2 = ( -B + D ) / A;
		if((t1 < 0) && (t2 < 0))
			return false;

		if(min(t1, t2) < 0)
		{
			time = max(t1,t2);
			return true;
		}
		time = min(t1, t2);
		return true;
	}
	return false;
}
bool IntersectTriangle (SRay ray, vec3 v1, vec3 v2, vec3 v3, out float time )
{
	time = -1;
	vec3 A = v2 - v1;
	vec3 B = v3 - v1;
	vec3 N = cross(A, B);
	float NdotRayDirection = dot(N, ray.Direction);
	if (abs(NdotRayDirection) < 0.001)
		return false;
	float d = dot(N, v1);

	float t = -(dot(N, ray.Origin) - d) / NdotRayDirection;

	if (t < 0)
		return false;

	vec3 P = ray.Origin + t * ray.Direction;

	vec3 C;

	vec3 edge1 = v2 - v1;
	vec3 VP1 = P - v1;
	C = cross(edge1, VP1);
	if (dot(N, C) < 0)
		return false;

	vec3 edge2 = v3 - v2;
	vec3 VP2 = P - v2;
	C = cross(edge2, VP2);
	if (dot(N, C) < 0)
		return false;

	vec3 edge3 = v1 - v3;
	vec3 VP3 = P - v3;
	C = cross(edge3, VP3);
	if (dot(N, C) < 0)
		return false;

	time = t;
	return true;

}

bool Raytrace ( SRay ray, SSphere spheres[2], STriangle triangles[10], SMaterial materials[6], float start, float final, inout SIntersection intersect )
{
	bool result = false;
	float test = start;
	intersect.Time = final;
	for(int i = 0; i < 2; i++)
	{
		SSphere sphere = spheres[i];
		if( (IntersectSphere (sphere, ray, start, final, test)) && (test < intersect.Time) )
		{
			intersect.Time = test;
			intersect.Point = ray.Origin + ray.Direction * test;
			intersect.Normal = normalize ( intersect.Point - spheres[i].Center );
			intersect.Color = vec3(0.0, 1.0, 0.0);
			intersect.LightCoeffs = vec4(0.4,0.9,0.0,512.0);
			intersect.ReflectionCoef = 0;
			intersect.RefractionCoef = 0;
			intersect.MaterialType = MIRROR_REFLECTION;
			result = true;
		}
	}
	for(int i = 0; i < 10; i++)
	{
		STriangle triangle = triangles[i];
		if((IntersectTriangle(ray, triangle.v1, triangle.v2, triangle.v3, test)) && (test < intersect.Time))
		{
			intersect.Time = test;
			intersect.Point = ray.Origin + ray.Direction * test;
			intersect.Normal = normalize(cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2));
			intersect.Color = vec3(0.0, 0.0, 1.0);
			intersect.LightCoeffs = vec4(0.4,0.9,0.0,512.0);
			intersect.ReflectionCoef = 0;
			intersect.RefractionCoef = 0;
			intersect.MaterialType = DIFFUSE_REFLECTION;
			result = true;
		}
	}
	return result;
}
/*Light */
void initializeDefaultLightMaterials(out SLight light, out SMaterial materials[6])
{
	//** LIGHT **//
	light.Position = vec3(0.0, 2.0, -4.0f);
	/** MATERIALS **/
	vec4 lightCoefs = vec4(0.4,0.9,0.0,512.0);
	materials[0].Color = vec3(0.0, 1.0, 0.0);
	materials[0].LightCoeffs = vec4(lightCoefs);
	materials[0].ReflectionCoef = 0.5;
	materials[0].RefractionCoef = 1.0;
	materials[0].MaterialType = DIFFUSE;
	materials[1].Color = vec3(0.0, 0.0, 1.0);
	materials[1].LightCoeffs = vec4(lightCoefs);
	materials[1].ReflectionCoef = 0.5;
	materials[1].RefractionCoef = 1.0;
	materials[1].MaterialType = DIFFUSE;
}
vec3 Phong ( SCamera uCamera, SIntersection intersect, SLight currLight, float shadowing)
{
	vec3 light = normalize ( currLight.Position - intersect.Point );
	float diffuse = max(dot(light, intersect.Normal), 0.0);
	vec3 view = normalize(uCamera.Position - intersect.Point);
	vec3 reflected = reflect( -view, intersect.Normal );
	float specular = pow(max(dot(reflected, light), 0.0), intersect.LightCoeffs.w);
	int Unit = 1;
	return intersect.LightCoeffs.x * intersect.Color + intersect.LightCoeffs.y * diffuse * intersect.Color * shadowing  + intersect.LightCoeffs.z * specular * Unit;
}
float Shadow(SLight currLight, SIntersection intersect, SSphere spheres[2], STriangle triangles[10], SMaterial materials[6])
{
	float shadowing = 1.0;
	vec3 direction = normalize(currLight.Position - intersect.Point);
	float distanceLight = distance(currLight.Position, intersect.Point);
	SRay shadowRay = SRay(intersect.Point + direction * EPSILON, direction);
	SIntersection shadowIntersect;
	shadowIntersect.Time = BIG;
	if(Raytrace(shadowRay, spheres, triangles, materials, 0, distanceLight, shadowIntersect))
	{
		shadowing = 0.0;
	}
	return shadowing;
}

void main ( void )
{
	float start = 0;
	float final = BIG;
	SSphere sf;
	SSphere spheres[2];
	STriangle triangles[10];
	SLight uLight;
	SMaterial materials[6];
	Stack_t StackOfRay;
/*	for (int i = 0; i < 512; i++)
	{
		StackOfRay.data[i];
	} */
//	StackOfRay.data;
	StackOfRay.size = 0;
	
	SCamera uCamera = initializeDefaultCamera();
	SRay ray = GenerateRay( uCamera);
	
	vec3 resultColor = vec3(0,0,0);
	initializeDefaultScene ( triangles, spheres );
	initializeDefaultLightMaterials(uLight, materials);
	
	STracingRay trRay1 = STracingRay(ray, 1, 0);
	pushRay(StackOfRay, trRay1);
	while((isEmpty(StackOfRay)) == false)
	{
		STracingRay trRay;
		trRay = popRay(StackOfRay);
		ray = trRay.ray;
		trRay.contribution = 1; //Undefined without
		SIntersection intersect;
		intersect.Time = BIG;
		start = 0;
		final = BIG;
		intersect.Point = vec3(0,0,0); intersect.Normal = vec3(0, 0, 0); intersect.Color = vec3(0, 0, 0); intersect.LightCoeffs = vec4(0, 0, 0, 0); intersect.ReflectionCoef = 0.0; intersect.RefractionCoef = 0.0; intersect.MaterialType = 0;
		bool flag = Raytrace(ray, spheres, triangles, materials, start, final, intersect);
		if (flag)
		{
			if(intersect.MaterialType == DIFFUSE_REFLECTION)
			{
				float shadowing = Shadow(uLight, intersect, spheres, triangles, materials);
				resultColor += trRay.contribution * Phong (uCamera, intersect, uLight, shadowing );
				
			}
			else 
			{	
				resultColor += (0, 1, 0);
				if(intersect.ReflectionCoef < 1)
				{
					float contribution = trRay.contribution * (1 - intersect.ReflectionCoef);
					float shadowing = Shadow(uLight, intersect, spheres, triangles, materials);
					resultColor += contribution * Phong(uCamera, intersect, uLight, shadowing);
				}
				vec3 reflectDirection = reflect(ray.Direction, intersect.Normal);
				float contribution = trRay.contribution * intersect.ReflectionCoef;
				STracingRay reflectRay = STracingRay( SRay(intersect.Point + reflectDirection * EPSILON, reflectDirection), contribution, trRay.depth + 1);
				pushRay(StackOfRay, reflectRay);
			}
			
		}
	}

/*	float contribution = 1.0;
	if (Raytrace(ray, spheres, triangles, materials, start, final, intersect)) // Tracing primary ray
	{
		float shadowing = Shadow(uLight, intersect, spheres, triangles, materials);
		resultColor += contribution * Phong (uCamera,  intersect, uLight, shadowing );
	}
*/	
	FragColor = vec4 (resultColor, 1.0);
}