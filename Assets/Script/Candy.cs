using UnityEngine;
using System.Collections;
using System.Linq;

public class Candy : MonoBehaviour {

	// Use this for initialization
	//public
	public int mCol = 0;//纵序号
	public int mRow = 0;//横序号

	public Vector3 mPos;//物体目标地址

	public float mSpeed = 30.5f;
	/// <summary>
	/// 索引，用于判断是哪种类型
	/// </summary>
	/// <value>The index of the m.</value>
	public int mIndex {
		get{return this.Index;}
		set{this.Index = value;}
	}
	private int Index;//索引
	//private
	public bool isStatic = false;//是否到达目标位置
	
	void Start () {
	}
	// Update is called once per frame
	void Update () {
		float step = mSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, mPos, step);
		if (transform.position == mPos) {
			isStatic = true;		
		} else {
			isStatic = false;
		}
	}
	void OnMouseUpAsButton () {
		SendMessageUpwards ("apply_exchange_pos", this);
	}
}
