using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class Candy : MonoBehaviour {

	// Use this for initialization
	//public
	public Vector3 mScale = new Vector3(0.5f,0.5f,0.5f);
	public Vector3 mPos;//物体目标地址
	//
	public int mCol = 0;//纵序号
	public int mRow = 0;//横序号
	//
	public float mSpeed = 30.5f;
	//
	public Material mDarkMaterial;
	public Material mNormalMaterial;
	//
	public GameController._TYPE mType = GameController._TYPE.NORMAL;
	//
	public delegate void ExchangeEventHandler(object sender,ExchangeEventArgs e);
	private event ExchangeEventHandler ExchangeEvent;
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

	public class ExchangeEventArgs:EventArgs
	{
		public ExchangeEventArgs (){
			this.mRow = 0;
			this.mCol = 0;
		}
		public ExchangeEventArgs(int col,int row){
			this.mRow = row;
			this.mCol = col;
		}
		public int mRow;
		public int mCol;
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
		this.onSelected (new ExchangeEventArgs ());
	}
	public void setDark(bool flag){
		SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer> ();
		if (flag) {
			sr.material = mDarkMaterial;
		} else {
			sr.material = mNormalMaterial;
		}
	}
	public void setChosen(bool ischosen){
		if (ischosen) {
			this.transform.localScale = this.mScale * 1.2f;
		} else {
			this.transform.localScale = this.mScale * 1f;
		}
	}
	protected virtual void onSelected(ExchangeEventArgs e){
		if (null != this.ExchangeEvent) {
			ExchangeEvent(this,e);		
		}
	}
	public void AttachEventCallback(ExchangeEventHandler eeh){
		this.ExchangeEvent += eeh;
	}
	public void DetachEventCallback(ExchangeEventHandler eeh){
		this.ExchangeEvent -= eeh;	
	}
}
