using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

//Represents a building.
public struct Building
{
	public List<Vector3> shape;
	public float height;
	public Vector3 position;
}

//Represents an 2 dimensional integer vector.
public struct IntVector2
{
	public int x;
	public int y;
	
	public IntVector2(int x, int y){
		this.x = x;
		this.y = y;
	}
	
	public static IntVector2 operator +(IntVector2 vec1, IntVector2 vec2){
		return (new IntVector2(vec1.x + vec2.x, vec1.y + vec2.y));
	}
	
	public static IntVector2 operator -(IntVector2 vec1, IntVector2 vec2){
		return (new IntVector2(vec1.x - vec2.x, vec1.y - vec2.y));
	}
	
	override public string ToString(){
		return "(" + x.ToString () + ", " + y + ")";
	}
}