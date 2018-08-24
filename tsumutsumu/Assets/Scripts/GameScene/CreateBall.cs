﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateBall : MonoBehaviour {

	//オブジェクト参照
	public GameObject ballPrefab; //ボールプレハブ
	public GameObject countDown; //カウントダウンタイマーとなるオブジェクト
	public GameObject timer; //制限時間タイマーとなるオブジェクト
	public GameObject score; //スコア表示
	public Sprite[] ballSprite = new Sprite[5]; //ボールの画像

	//メンバ変数
	private GameObject firstBall; //最初にタッチしたボール
	private GameObject lastBall; //直前にドラッグしたボール
	private List<GameObject> removableBallList; //消去するボールのリスト
	private string currentName; //現在リストにあるボールの名前
	private bool isPlaying = false; //プレイ中かどうか
	private Text timerText; //タイマーのテキスト
	private Text countDownText; //カウントダウンのテキスト
	private Text scoreText; //スコアテキスト
	private float timeLimit = 60; //制限時間
	private int countTime = 5; //カウントダウンの秒数
	private int currentScore = 0; //現在のスコア

	// Use this for initialization
	void Start () {
		countDownText = countDown.GetComponent<Text>(); //タイマーを取得
		timerText = timer.GetComponent<Text>(); //制限時間タイマー
		scoreText = score.GetComponent<Text>(); //scoreTextを設定
		StartCoroutine ("CountDown"); //カウントダウン開始
	}

	private IEnumerator CountDown(){
		int count = countTime;
		while(count > 0){
			countDownText.text = count.ToString (); //カウントダウンのテキスト
			yield return new WaitForSeconds(1); //1秒待つ
			count -= 1; //カウントを１つ減らす
		}
		countDownText.text = "Start!";
		isPlaying = true;
		yield return new WaitForSeconds (1);
		countDown.SetActive (false);
		StartCoroutine ("DropBall", 55);  //ボールを降らす
		StartCoroutine ("StartTimer"); //制限時間のカウントを開始
	}

	private IEnumerator StartTimer(){
		float count = timeLimit;
		while (count > 0) {
			timerText.text = count.ToString ();
			yield return new WaitForSeconds (1);
			count -= 1;
		}
		countDown.SetActive (true);
		countDownText.text = "Finish";
		OnDragEnd ();
		isPlaying = false;
	}

	//count個ボールを降らせる
	private IEnumerator DropBall(int count){
		for (int i = 0; i < count; i++) {
			
			//ボールのプレハブを読み込み
			GameObject ball = (GameObject)Instantiate (ballPrefab); 

			//ボールの座標と角度をランダムに設定
			ball.transform.position = new Vector3 (
				Random.Range (-2.0f, 2.0f), 7, 0);
			ball.transform.eulerAngles = new Vector3 (
				0, 0, Random.Range (-40, 40));

			//ボールの画像のidをランダムに設定し名前と画像をidに合わせて変更
			int spriteId = Random.Range (0, 5);
			ball.name = "Ball" + spriteId;
			ball.GetComponent<SpriteRenderer> ().sprite = ballSprite [spriteId];

			//次のボールを生成するまで一定時間待つ
			yield return new WaitForSeconds (0.05f);  
		}
	}

	private void Update(){
		if (isPlaying) {
			if (Input.GetMouseButtonDown (0) && firstBall == null) {
				//ボールをドラッグし始めた時
				OnDragStart ();
			} else if (Input.GetMouseButtonUp (0)) {
				//ボールをドラッグし終わった時
				OnDragEnd ();
			} else if (firstBall != null) {
				//ボールをドラッグしている途中
				OnDragging ();
			}
		}

		scoreText.text = currentScore.ToString(); //現在のスコアを表示
	}

	private void OnDragStart(){
		Collider2D col = GetCurrentHitCollider ();//現在マウスカーソルの位置
		if (col != null) {
			//何かをドラッグしている時
			GameObject colObj = col.gameObject;
			if(colObj.name.IndexOf("Ball") != -1){
				//名前に"Ball"を含むオブジェクトをドラッグした時
				removableBallList = new List<GameObject>(); 
				firstBall = colObj; //はじめにドラッグしたボールを現在のボールに設定
				currentName = colObj.name; //現在のリストのボールの名前を設定
				PushToList(colObj);
			}
		}
	}

	private void OnDragEnd(){
		if(firstBall != null){
			//1つ以上のボールをなぞっている時
			int length = removableBallList.Count;
			if (length >= 3) {
				//消去するリストに３個以上のボールがあれば
				for (int i = 0; i < length; i++) {
					Destroy (removableBallList [i]); //リストにあるボールを消去
				}

				currentScore += (CalculateBaseScore(length) + 50 * length);

				StartCoroutine ("DropBall", length); //消した分だけボールを生成
			} else {
				//消去するリストに3個以上ボールがない時
				for (int j = 0; j < length; j++) {
					GameObject listedBall = removableBallList [j];
					ChangeColor (listedBall, 1.0f); //アルファ値を戻す
					listedBall.name = listedBall.name.Substring (1, 5);
				}
			}
			firstBall = null; //変数の初期化
		}
	}

	private void OnDragging(){
		Collider2D col = GetCurrentHitCollider ();
		if (col != null) {
			//何かをドラッグしている時
			GameObject colObj = col.gameObject;
			if (colObj.name == currentName) {
				//現在リストに追加している色と同じ色のボールの時
				if (lastBall != colObj) {
					//直前にリストに入れたのと異なるボールの時
					float dist = Vector2.Distance (lastBall.transform.position, colObj.transform.position);
					if (dist <= 1.5) {
						//ボール間の距離が一定値以下の時
						PushToList (colObj); //消去するリストにボールを追加
					}
				}
			}
		}
	}

	void PushToList(GameObject obj){
		lastBall = obj; //直前にドラッグしたボールに現在のボールを追加
		ChangeColor(obj, 0.5f); //現在のボールを半透明にする
		removableBallList.Add(obj); //消去するリストに現在のボールを追加
		obj.name = "_" + obj.name; //区別するため、消去するボールのリストに加えたボールの名前を変更
	}

	Collider2D GetCurrentHitCollider(){
		RaycastHit2D hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero);
		return hit.collider;
	}

	private void ChangeColor(GameObject obj, float transparency){

		Color originalColor = obj.GetComponent<SpriteRenderer> ().color;
		obj.GetComponent<SpriteRenderer> ().color = new Color (originalColor.r, originalColor.g, originalColor.b, transparency);
	
	}

	private int CalculateBaseScore(int num){
		int tempScore = 50 * (num) * (num + 1) - 300;
		return tempScore;
	}

	public void Reset(){
		SceneManager.LoadScene ("GameScene");
	}
}
