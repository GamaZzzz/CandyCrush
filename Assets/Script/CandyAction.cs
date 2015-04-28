using UnityEngine;
using System.Collections;
using System.Linq;

public class CandyAction : MonoBehaviour {

	// Use this for initialization
	//public
	public int mCol = 0;//纵序号
	public int mRow = 0;//横序号
	public bool isReorder = true;
	public Vector3 mPos;
	public bool isDestroy = false;
	public float mSpeed = 30.5f;
	/// <summary>
	/// 索引，用于判断是哪种类型
	/// </summary>
	/// <value>The index of the m.</value>
	public int mIndex {
		get{return this.Index;}
		set{this.Index = value;}
	}
	private int Index;//序号
	//private
	private bool isChosen = false;//选中状态
	//
	public bool isStatic = false;
	
	void Start () {
	}
	// Update is called once per frame
	void Update () {
		print(transform.position);
		float step = mSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, mPos, step);
		if (transform.position == mPos) {
			isStatic = true;		
		} else {
			isStatic = false;
		}
	}
	void OnMouseUpAsButton () {
		this.isChosen = !this.isChosen;
		SendMessageUpwards ("apply_exchange_pos", this);
	}
}
