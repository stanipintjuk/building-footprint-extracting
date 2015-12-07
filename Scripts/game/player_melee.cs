using UnityEngine;
using System.Collections;

public class player_melee : MonoBehaviour {

	private RaycastHit hit;
	public GameObject explosion;
	public GameObject weapon;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray = camera.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0));
		if (Input.GetMouseButton (0)) {
			weapon.animation.CrossFade("hit");
		}
		if (Physics.Raycast (ray, out hit)) {
			
			if(Input.GetMouseButtonDown(0)){
				if (hit.collider.gameObject.name == "building"){
					Vector3 pos = hit.collider.gameObject.transform.position-(new Vector3(12.5f,0f,12.5f));
					Instantiate(explosion,pos,Quaternion.identity);
					Destroy(hit.collider.gameObject);
				}	
			}
		}

		if (weapon.animation.isPlaying == false && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
		{
			weapon.animation.CrossFade("walk");
		}
	}
}
