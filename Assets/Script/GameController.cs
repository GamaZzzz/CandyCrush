using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	//public
	public GameObject T1;//预设1
	public GameObject T2;//预设2
	public GameObject T3;//预设3
	public GameObject T4;//预设4
	public GameObject T5;//预设5
	public Vector3 BVector = Vector3.zero;//初始位置
	public float xOff = 0f;//x轴偏移量
	public float yOff = 0f;//y轴偏移量
	public int mCol = 0;//列数
	public int mRow = 0;//行数
	//private
	private bool isReadyToCheck = true;//检测标识
	private ArrayList OPool;//对象池
	private ArrayList MatchList;//匹配列表

	void Start () {
		//
		OPool = new ArrayList (mCol);
		//
		MatchList = new ArrayList ();
		//
		for (int col = 0; col<this.mCol; col++) {
			ArrayList temp = new ArrayList(mRow);
			for(int row = 0;row<this.mRow;row++){
				CandyAction item = getRandomGameObject(col,row);
				if(null!=item){
					temp.Add(item);
				}
				else{
					Debug.Log("getRandomGameObject(col,row) Faild!");
				}
			}
			OPool.Add(temp);
		}
		while (0<CheckMatch()) {
			removeMatchCandy();
			DestroyCandy();
			addCandys0();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isReadyToCheck) {
			isReadyToCheck = false;
			StartCoroutine(waitAndCheck());		
		}
	}
	int CheckMatch(){
		for (int col = 0; col<mCol; col++) {
			for(int row = 0; row < mRow;row ++){
				CandyAction item0 = getCandy(col,row);
				if(MatchList.Contains(item0)) item0 = null;
				CandyAction item1 = getCandy(col,row+1);
				CandyAction item2 = getCandy(col,row+2);
				if(null!= item0 && null != item1 && null != item2){
					if(item0.mIndex == item1.mIndex && item0.mIndex == item2.mIndex){
						MatchList.Add(item0);
						if(!MatchList.Contains(item1))MatchList.Add(item1);
						if(!MatchList.Contains(item2))MatchList.Add(item2);
					}
				}
			}
		}
		for(int row = 0; row < mRow;row ++){
			for (int col = 0; col<mCol; col++) {
				CandyAction item0 = getCandy(col,row);
				if(MatchList.Contains(item0)) item0 = null;
				CandyAction item1 = getCandy(col+1,row);
				CandyAction item2 = getCandy(col+2,row);
				if(null!= item0 && null != item1 && null != item2){
					if(item0.mIndex == item1.mIndex && item0.mIndex == item2.mIndex){
						MatchList.Add(item0);
						if(!MatchList.Contains(item1))MatchList.Add(item1);
						if(!MatchList.Contains(item2))MatchList.Add(item2);
					}
				}
			}
		}
		return MatchList.Count;
	}
	void removeMatchCandy(){
		for (int index = 0; index<MatchList.Count; index++) {
			CandyAction temp = MatchList[index] as CandyAction;
			removeCandy(temp);	
		}
	}
	void DestroyCandy(){
		for (int index = 0; index<MatchList.Count; index++) {
			CandyAction temp = MatchList[index] as CandyAction;
			Destroy(temp.gameObject);
		}
		MatchList.Clear ();
	}
	void addCandys(){
		for (int col = 0; col < this.mCol; col++) {
			ArrayList temp = OPool[col] as ArrayList;
			int row = 0 ;
			for(;row < temp.Count;row++){		
				CandyAction temp_item = temp[row] as CandyAction;
				temp_item.mRow = row;
				temp_item.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0f);
			}
			for(;row < this.mRow;row++){
				CandyAction item = getRandomGameObject(col,row);
				item.transform.position = new Vector3(BVector.x + col*xOff,BVector.y+(this.mRow+row)*yOff,0f);
				temp.Add(item);
			}
		}
	}
	void addCandys0(){
		for (int col = 0; col < this.mCol; col++) {
			ArrayList temp = OPool[col] as ArrayList;
			int row = 0 ;
			for(;row < temp.Count;row++){		
				CandyAction temp_item = temp[row] as CandyAction;
				temp_item.mRow = row;
				temp_item.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0f);
			}
			for(;row < this.mRow;row++){
				CandyAction item = getRandomGameObject(col,row);
				item.transform.position = Vector3.zero;
				temp.Add(item);
			}
		}
	}
	void apply_adjust_postion(ArrayList items){
		if (2 == items.Count) {
			CandyAction item0 = items[0] as CandyAction;
			CandyAction item1 = items[1] as CandyAction;
			resetCandy(item0);
			resetCandy(item1);
			isReadyToCheck = true;
		}
	}
	void apply_adjust_postion_0(ArrayList items){
		if (2 == items.Count) {
			CandyAction item0 = items[0] as CandyAction;
			CandyAction item1 = items[1] as CandyAction;
			resetCandy(item0);
			resetCandy(item1);
		}
	}
	IEnumerator waitAndCheck(){
		yield return new WaitForSeconds (0.5f);
		if (2 < CheckMatch ()) {
			;	
		} else {
			SendMessage("reset_pos");//恢复位置
		}
	}
	CandyAction getCandy(int col,int row){
		if (col < 0 || row < 0) {
			return null;		
		}
		if (col < this.mCol && row < this.mRow) {
			ArrayList temp = OPool[col] as ArrayList;
			return temp[row] as CandyAction;
		} else {
			return null;
		}
	}
	CandyAction resetCandy(CandyAction item){
		ArrayList temp = OPool[item.mCol] as ArrayList;
		CandyAction temp_object = temp[item.mRow] as CandyAction;
		temp[item.mRow] = item;
		return temp_object;
	}
	void removeCandy(CandyAction item){
		ArrayList temp = OPool[item.mCol] as ArrayList;
		temp.Remove (item);
	}
	/// <summary>
	/// 获取随机糖果
	/// </summary>
	/// <returns>The random game object.</returns>
	private CandyAction getRandomGameObject(int col,int row){
		GameObject result = null;
		//Random.seed = Mathf.FloorToInt (Time.time)+Random.Range(0,1000);
		int index = Random.Range (0, 5);
		switch (index) {
		case 0:{	
			result = Instantiate(T1) as GameObject;
			break;
		}
		case 1:{	
			result = Instantiate(T2) as GameObject;
			break;
		}
		case 2:{	
			result = Instantiate(T3) as GameObject;
			break;
		}
		case 3:{	
			result = Instantiate(T4) as GameObject;
			break;
		}
		case 4:{	
			result = Instantiate(T5) as GameObject;
			break;
		}
		default:break;
		}
		CandyAction ca = null;
		if (null != result) {
			result.transform.parent = this.transform;
			result.transform.position = Vector3.zero;
			ca = result.GetComponent (typeof(CandyAction)) as CandyAction;
			ca.mIndex = index;
			ca.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0);
			ca.mRow = row;
			ca.mCol = col;
			ca.isDestroy = false;
		}
		return ca;
	}
}
