using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
///
/// FootprintHandler generates extracts building footprints from a map and manuplates them.
/// 
public class FootprintHandler
{
	float threshold;

	int max_height = 30;
	int min_height = 10;

	public List<Building> buildings;
	private int[,] map;

	private int map_height;
	private int map_width;

	public FootprintHandler (float threshold)
	{
		this.threshold = threshold;
		buildings = new List<Building> ();
	}

	public FootprintHandler ():this(15.0f){}

	//generates buildings from a 2 dimensional map.
	//every non-zero element represents a non-building element on the map.
	public void createFromMap(int[,] map, int width, int height){

		this.map = map;
		map_width = width;
		map_height = height;

		//extract building footprints
		findContours ();
		approximateShapes ();
		clean ();


		//do modifications
		splitRandomBuildings ();
		createHeights ();
		calculatePositions ();
	}

	//selects buildings in buildings list and applys splitBuilding() to them.
	private void splitRandomBuildings(){
		System.Random rand = new System.Random ();
		int ammount = rand.Next(0, buildings.Count);
		int current_count = buildings.Count;

		while (ammount != 0) {
			int index = rand.Next(0, current_count);

			splitBuilding(rand.Next(0, index));
			ammount--;
		}
	}

	//Splits the building at position i in buildings list in half.
	private void splitBuilding(int i){
		System.Random rand = new System.Random ();
		Building b1 = new Building(); b1.shape = new List<Vector3>();
		Building b2 = new Building(); b2.shape = new List<Vector3>();
		
		Building b = buildings[i];

		if (b.shape.Count <= 3) {
			return;
		}

		int split_index_1 = rand.Next(0, b.shape.Count-2); // b.shape.Count / 2 - 1;
		int split_index_2 = rand.Next(split_index_1+2, b.shape.Count); // b.shape.Count - 1;
		
		
		for (int j = 0; j < b.shape.Count; j++){
			if (j < split_index_1 || j > split_index_2){
				b1.shape.Add(b.shape[j]);
			}else if (j == split_index_1 || j == split_index_2){
				b1.shape.Add(b.shape[j]);
				b2.shape.Add(b.shape[j]);
			}else{
				b2.shape.Add(b.shape[j]);
			}
		}
		
		buildings[i] = b1;
		buildings.Add(b2);
	}

	//calculates the mid point of every building and sets it to the buildings position.
	private void calculatePositions(){
		for (int b_i = 0; b_i < buildings.Count; b_i++) {
			Building b = buildings[b_i];

			Vector3 mid = Vector3.zero;
			int count = 0;
			foreach(Vector3 v in b.shape){
				mid.x += v.x;
				mid.z += v.z;
				count += 1;
			}

			mid = mid/count;

			for (int i = 0; i < b.shape.Count; i++){
				b.shape[i]-=mid;
			}

			b.position = mid;
			buildings[b_i] = b;
		}
	}

	//Removes every building with less than 3 vertexes in its shape.
	private void clean(){
		for (int i = 0; i < buildings.Count; i++){
			if (buildings[i].shape.Count < 3){
				buildings.RemoveAt(i);
				i--;
			}
		}
		buildings.TrimExcess ();
	}

	//Sets a height between min_height and max_height to each building in the buildings list.
	private void createHeights(){
		System.Random rand = new System.Random ();

		for(int i = 0; i < buildings.Count; i++){
			Building b = buildings[i];
			b.height = rand.Next(min_height,max_height);
			buildings[i] = b;
		}
	}

	//Picks out the most relevant points in the building's
	//contours in order to approximate the buildings shape.
	private void approximateShapes(){

		for (int b_i = 0; b_i < buildings.Count; b_i++) {
			Building b = buildings[b_i];
			
			List<Vector3> new_shape = new List<Vector3>();
			
			//loop through all vectors in b.shape
			for (int i = 0; i < b.shape.Count; i++){
				Vector3 v1 = b.shape[i];
				new_shape.Add(v1);

				//The first vector from the end that has error_sum < threshold is the next point
				for (int j = b.shape.Count-1; j > i; j--){
					Vector3 v2 = b.shape[j];
					
					float error_sum = 0;
					//loop through all vectors between v1 and v2
					for (int l = i+1; l < j; l++){
						error_sum+=getError(v1, v2, b.shape[l]);
					}
					
					if(error_sum < threshold){
						//this is the best point. Continue from here
						i = j;
					}
				}
			}
			b.shape.Clear();
			b.shape.AddRange(new_shape);
			b.shape.TrimExcess();
		}
	}

	//Returns the orthogonal distance between vector vec and
	//the line drawn from vector 'from' to vector 'to'.
	private float getError(Vector3 from, Vector3 to, Vector3 vec){
		Vector3 l = to - from;
		Vector3 v3 = vec - to;
		Vector3 proj = Vector3.Project (v3, l.normalized);
		return (proj - v3).magnitude;
	}

	//Traces the contours of the buildings in the map using Moore's Neighborhood algorithm
	private void findContours(){

		for (int y = 0; y < map_height; y++) {
			for (int x = 0; x < map_width; x++){
				if (isNewBuilding(x,y)){
					List<IntVector2> bounds = new List<IntVector2> ();
					bounds.Add(new IntVector2(x,y));
					IntVector2 b_start = new IntVector2(x,y);

					IntVector2 dir = new IntVector2(-1,0);
					IntVector2 pos = new IntVector2(-1,-1);
					IntVector2 c = b_start+dir;
					IntVector2 prev_c = c;

					int rot_count = 0;

					while (pos.x != b_start.x || pos.y != b_start.y){
						if (getFromMap(c.x,c.y) != 0){
							bounds.Add(c);
							pos = c;
							dir = prev_c - c;
							c = prev_c;
							rot_count = 0;
						}else{
							rot_count++;
							prev_c = c;
							dir = rotateDir(dir, Mathf.PI/4.0f);

							if (pos.x > -1){
								c = pos+dir;
							}else{
								c = b_start+dir;
							}
						}

						if(rot_count > 18){
							MonoBehaviour.print("rotation limit exceded");
							return;
						}
					}

					bounds.TrimExcess();

					Building b = new Building();
					b.shape = new List<Vector3>();

					foreach(IntVector2 ivec2 in bounds){
						b.shape.Add(new Vector3(ivec2.x, 0.0f, ivec2.y));
					}

					b.shape.TrimExcess();
					buildings.Add(b);

					x=0; y=0;
				}
			}
		}

		buildings.TrimExcess ();

	}

	//Returns false if the (x,y) coordinates in the map array is a non-building element
	//or if the non-building element belongs to building that allready has been analyzed.
	//Returns true if (x,y) is a building element and belongs to a building preveously not analyzed.
	private bool isNewBuilding(int x, int y){
		if (getFromMap (x, y) == 0) {
				return false;
		} else {
			foreach(Building b in buildings){

				List<Vector3> sameRow = new List<Vector3>();
				//find all building elements on row y
				foreach(Vector3 vec3 in b.shape){
					if (y == (int)vec3.z){
						sameRow.Add(vec3);
					}
				}

				float min_x = map_width+1;
				float max_x = -1;
				//find an element with max x and an element with min x in the row
				foreach(Vector3 pos in sameRow){
					min_x = Mathf.Min(min_x, pos.x);
					max_x = Mathf.Max(max_x, pos.x);
				}

				if (x >= min_x && x <= max_x){
					return false;
				}				
			}

			return true;
		}
	}

	//Rotates dir theta degrees and returns the rounded result in an integer vector.
	private IntVector2 rotateDir(IntVector2 dir, float theta){
		float x = Mathf.Cos (theta)*dir.x - Mathf.Sin (theta)*dir.y;
		float y = Mathf.Sin (theta)*dir.x + Mathf.Cos (theta)*dir.y;
		Vector2 v = new Vector2 (x, y);
		v = v / v.magnitude;
		return new IntVector2 (pnCeil (v.x), pnCeil (v.y));
	}

	//positive-negative-ceil ceils both negative and positive numbers into their current direction.
	//eg: 0.5 -> 1, -0.62 -> -1
	private int pnCeil(float z){
		int dif = (int)(z / Mathf.Abs (z));
		z = dif * z;
		z = Mathf.Ceil (z);
		z = dif * z;
		return (int)z;
	}

	//Returns the element at position (x,y) in map.
	//Returns 0 if (x, y) is out of bound.
	private int getFromMap(int x, int y){
		if (x >= 0 && x < map_width && y >= 0 && y < map_height) {
			return map [x, y];
		} else {
			return 0;
		}
	}
}