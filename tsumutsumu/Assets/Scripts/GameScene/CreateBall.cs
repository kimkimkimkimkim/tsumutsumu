using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateBall : MonoBehaviour {

	//オブジェクト参照
	public GameObject ballPrefab;
	public Sprite[] ballSprite = new Sprite[5];

	// Use this for initialization
	void Start () {
		StartCoroutine ("DropBall", 55);  
	}

	//count個ボールを降らせる
	private IEnumerator DropBall(int count){
		for (int i = 0; i < count; i++) {
			GameObject ball = (GameObject)Instantiate (ballPrefab);
			ball.transform.position = new Vector3 (
				Random.Range (-2.0f, 2.0f), 7, 0);
			ball.transform.eulerAngles = new Vector3 (
				0, 0, Random.Range (-40, 40));
			int spriteId = Random.Range (0, 5);
			ball.name = "Ball" + spriteId;
			ball.GetComponent<SpriteRenderer> ().sprite = ballSprite [spriteId];

			yield return new WaitForSeconds (0.05f);  
		}
	}
}
