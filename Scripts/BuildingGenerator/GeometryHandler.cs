using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Geometry handler has all the necessary methods for creating a 3D model from the Building struct.
public class GeometryHandler
{
	public GeometryHandler ()
	{
	}	

	//Takes a plane mesh facing the negative Y axis and returns the dragged out block version of it.
	public Mesh dragOut(Mesh ground_plane, float height){

		Mesh building = new Mesh();

		Vector3[] bottom_v = ground_plane.vertices;
		int[] bottom_t = ground_plane.triangles;

		Vector3[] top_v = new Vector3[ground_plane.vertices.Length];
		int[] top_t = new int[ground_plane.triangles.Length];

		//1. bottom vertices -> top vertices -> side vertices
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();

		//create top vertices
		//add all bottom_v to vertices list and to side_v.
		Vector3 trans = new Vector3 (0.0f,height,0.0f);
		for (int i = 0; i < bottom_v.Length; i++) {
			top_v[i] = bottom_v[i]+trans;
			vertices.Add (bottom_v[i]);
		}
		//add all top_v to vertices list and to side_v
		for (int i = 0; i < top_v.Length; i++){
			vertices.Add(top_v[i]);
		}

		//invert rotate bottom triangles to create top triangles
		for (int i = 0; i < bottom_t.Length; i++) {
			//add the length of bottom_v to every vertex index in top_v
			top_t[i] = bottom_v.Length+bottom_t[i];
			if ((i+1)%3 == 0){
				int tmp = top_t[i-1];
				top_t[i-1] = top_t[i];
				top_t[i] = tmp;
			}
			triangles.Add(bottom_t[i]);
		}
		//add all top_t to triangles
		for (int i = 0; i < top_t.Length; i++) {
			triangles.Add(top_t[i]);
		}


		//create side faces
		List<Vector3> side_v = new List<Vector3>();
		List<int> side_t = new List<int>();
		int len = top_v.Length;
		int indent = top_v.Length * 2;
		for (int i = 0; i < len; i++) {
			side_v.Add(bottom_v[i]);
			side_v.Add(bottom_v[(i+1)%len]);
			side_v.Add(top_v[i]);
			side_v.Add(top_v[(i+1)%len]);			

			side_t.Add (indent+i*4);
			side_t.Add (indent+i*4+2);
			side_t.Add (indent+i*4+1);

			side_t.Add (indent+i*4+1);
			side_t.Add (indent+i*4+2);
			side_t.Add (indent+i*4+3);
		}

		//add side vertices and triangles to vertices and triangles lists
		vertices.AddRange (side_v);
		triangles.AddRange (side_t);


		vertices.TrimExcess ();
		triangles.TrimExcess ();
		//wrap up with uv and recalulating normals
		Vector2[] uvs = new Vector2[vertices.Count];
		for (int i = 0; i < uvs.Length; i ++) {
			uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
		}
		
		building.vertices = vertices.ToArray();
		building.uv = uvs;
		building.triangles = triangles.ToArray();
		
		building.RecalculateNormals ();
		
		return building;
	}

	//Creates a plane of the input verices. This plane will be facing negative Y axis.
	public Mesh createPlane(Vector3[] verticesIn){

		Mesh mesh = new Mesh ();
		mesh.Clear ();

		int len = verticesIn.Length;
		//Calculate triangles
		//2. cases: even/odd number of verteces

		bool even = (len % 2 == 0);
		int[] triangles;

		if (even) {
			triangles = new int[((len / 2) - 1) * 6];
		} else {
			triangles = new int[((len-1)/2)*6-3];
		}

		if (even) {
			for (int i = 0; i < len/2-1; i++){
				triangles[i*6] = i;
				triangles[i*6+1] = i+1;
				triangles[i*6+2] = (len-1)-i;

				triangles[i*6+3] = i+1;
				triangles[i*6+4] = (len-1)-1-i;
				triangles[i*6+5] = (len-1)-i;
			}
		} else {
			for (int i = 0; i < (len-1)/2; i++){
				triangles[i*6] = i;
				triangles[i*6+1] = i+1;
				triangles[i*6+2] = (len-1)-i;

				if (i < (len-1)/2-1){
					triangles[i*6+3] = i+1;
					triangles[i*6+4] = (len-1)-1-i;
					triangles[i*6+5] = (len-1)-i;
				}
			}
		}

		//end
		Vector2[] uvs=new Vector2[verticesIn.Length];
		for (int i = 0; i < uvs.Length; i++) {
			uvs[i] = new Vector2(verticesIn[i].x, verticesIn[i].z);
		}
		
		mesh.vertices = verticesIn;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		
		mesh.RecalculateNormals ();
		return mesh;
		
	}

	//Creates a 1x1 square plane using createPlane()
	public Mesh squarePlane(){
		Vector3[] square = new Vector3[4];
		square[0]=new Vector3(0.5f, -0.5f, 0.5f);
		square[1]=new Vector3(-0.5f, -0.5f, 0.5f);
		square[2]=new Vector3(-0.5f, -0.5f, -0.5f);
		square[3]=new Vector3(0.5f, -0.5f, -0.5f);

		Mesh sqr = createPlane (square);
		return sqr;
	}

	//Creates a plane based on Building b.shape,
	//drags it up according to b.height and creates a GameObject from the mesh.
	public GameObject materializeBuilding(Building b, Vector3 position){
		Mesh mesh = dragOut(createPlane(b.shape.ToArray()), b.height);
		GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
		building.name = ("building");
		building.transform.GetComponent<MeshFilter>().mesh = mesh;
		
		MeshCollider meshc = building.AddComponent(typeof(MeshCollider)) as MeshCollider;
		meshc.sharedMesh = mesh;
		building.transform.position = b.position+position;
		return building;
	}
}

