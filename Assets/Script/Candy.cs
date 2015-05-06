using UnityEngine;
using System.Collections;
using System.Linq;

public class Candy : MonoBehaviour {

	// Use this for initialization
	//public
	public Vector3 mScale = new Vector3(0.5f,0.5f,0.5f);

	public int mCol = 0;//纵序号
	public int mRow = 0;//横序号

	public Vector3 mPos;//物体目标地址

	public float mSpeed = 30.5f;

	public Material mMaterial;

	public GameController._TYPE mType = GameController._TYPE.NORMAL;
	//Fields
	private bool mSpecial = false;//是否是特殊糖果
	public bool isSpecial{
		get{return this.mSpecial;}
		set{this.mSpecial = value;}
	}
	// 索引，用于判断是哪种类型
	public int mIndex {
		get{return this.Index;}
		set{this.Index = value;}
	}
	private int Index;//索引
	//是否到达指定位置
	private bool mStatic = false;
	public bool isStatic{
		get{return mStatic;}
	}
	
	void Start () {
		transform.localScale = this.mScale;
	}
	// Update is called once per frame
	void Update () {
		float step = mSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, mPos, step);
		if (transform.position == mPos) {
			mStatic = true;		
		} else {
			mStatic = false;
		}
	}
	void OnMouseUpAsButton () {
		print (this.mCol.ToString() + "," + this.mRow.ToString());
		SendMessageUpwards ("apply_exchange_pos", this);
	}
	public void setDark(){
		SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer> ();
		sr.material = mMaterial;
	}
	public void setChosen(bool ischosen){
		if (ischosen) {
			this.transform.localScale = this.mScale * 1.2f;
		} else {
			this.transform.localScale = this.mScale * 1f;
		}
	}
}
