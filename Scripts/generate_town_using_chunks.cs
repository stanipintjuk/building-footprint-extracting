using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class generate_town_using_chunks: MonoBehaviour {
	public Texture2D map_image;
	public float threshold = 15.0f;
	public int split_x = 10;
	public int split_y = 10;
	public Transform player;

	private RunThreadableJob run;
	private BuildingGenerator[,] cityChunks;
	private int chunk_width;
	private int chunk_height;

	// Use this for initialization
	void Awake () {
		if (split_x > 1 && split_y > 1) {
				// prepare chunks only if the user specified more than 1
				createChunks ();
				run = null;
		} else {
			// otherwise simply generate the chunk
			new BuildingGenerator(map_image,transform.position,threshold,transform).Create ();
		}
	}

	void Update()
	{
		// generate chunks only if the user spcified more than 1
		if (split_x > 1 && split_y > 1) {
				generateChunks ();
		}
	}

	//Generates chunks around the players position
	void generateChunks(){
		if (run != null) {
			if (run.Update ()) {
				// Alternative to the OnFinished callback
				run = null;
				print ("done");	
			}else if(run.time > 30.0f){
				run.Abort();
				run = null;
				print ("aborted");
			}
		} else {
			int p_x = ((int)player.transform.position.x)/chunk_width;
			int p_y = ((int)player.transform.position.z)/chunk_width;

			if (generatePart(p_x,p_y)){
				print ("generating");
			}else if (generatePart(p_x-1,p_y)){
				print ("generating");
			}else if (generatePart(p_x-1,p_y-1)){
				print ("generating");
			}else if (generatePart(p_x-1,p_y+1)){
				print ("generating");
			}else if (generatePart(p_x+1,p_y-1)){
				print ("generating");
			}else if (generatePart(p_x+1,p_y+1)){
				print ("generating");
			}else if (generatePart(p_x,p_y-1)){
				print ("generating");
			}else if (generatePart(p_x,p_y+1)){
				print ("generating");
			}else if (generatePart(p_x+1,p_y)){
				print ("generating");
			}
	
		}
	}

	bool generatePart(int x, int y){
		if (!isGenerated (x, y)) {
			run = new RunThreadableJob (cityChunks [x, y]);
			run.Start ();

			return true;
		} else {
			return false;
		}
	}

	bool isGenerated(int x, int y){
		if (x > -1 && y > -1 && x < cityChunks.GetLength (0) && y < cityChunks.GetLength (1)) {
				return cityChunks[x,y].generated;
		} else {
			return true;
		}
	}
	//Breaks down the map into 10x10 chunks.
	void createChunks(){
		chunk_width = map_image.width / split_x;
		chunk_height = map_image.height / split_y;
		cityChunks = new BuildingGenerator[split_x, split_y];
		
		for (int i = 0; i < cityChunks.GetLength(0); i++){
			for (int j = 0; j < cityChunks.GetLength(1); j++){
				
				Texture2D map = new Texture2D(chunk_width, chunk_height);
				Vector3 position = new Vector3(i*chunk_width, 0f, j*chunk_height);
				
				for (int x = i*chunk_width; x < (i+1)*chunk_width; x++){
					for (int y = j*chunk_height; y < (j+1)*chunk_height; y++){
						map.SetPixel(x,y,map_image.GetPixel(x,y));
					}
				}
				BuildingGenerator b = new BuildingGenerator(map,position,threshold,transform);
				cityChunks[i,j] = b;
			}
		}
	}
}
