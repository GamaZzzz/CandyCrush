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
	public Vector3 BVector = Vector3.zero;//矩阵原点位置
	public Vector3 SVector = Vector3.zero;//物体生成初始位置
	public float xOff = 0f;//x轴偏移量
	public float yOff = 0f;//y轴偏移量
	public int mCol = 0;//列数
	public int mRow = 0;//行数
	//private
	private bool isReadyToCheck = false;//检测标识
	private ArrayList OPool;//对象池
	private ArrayList MatchList;//匹配列表
	private int frames = 0;
	private bool isExchange = false;
	void Start () {
		//
		InitPool (this.mCol, this.mRow, ref this.OPool);
		InitCandys (SVector);
		//
		MatchList = new ArrayList ();
		//
		while (0<CheckMatch()) {
			removeMatchCandy();
			DestroyCandy();
			InitCandys (SVector);
		}
	}
	void InitPool(int pcol,int prow,ref ArrayList pool){
		pool = new ArrayList (pcol);
		for (int col=0; col<pcol; col++) {
			ArrayList temp = new ArrayList(prow);
			pool.Add(temp);
		}
	}
	void InitCandys(Vector3 initPos){
		for (int col = 0; col < this.mCol; col++) {
			ArrayList temp = OPool[col] as ArrayList;
			int row = 0 ;
			for(;row < temp.Count;row++){		
				Candy temp_item = temp[row] as Candy;
				temp_item.mRow = row;
				temp_item.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0f);
			}
			for(;row < this.mRow;row++){
				Candy item = getRandomGameObject(col,row,initPos);
				temp.Add(item);
			}
		}
	}
	// Update is called once per frame
	void Update () {
		if (MatchList.Count == 0 && isAllReady () && isReadyToCheck) {
			isReadyToCheck = false;
			StartCoroutine (waitAndCheck ());	
			SendMessage ("notice_isReadyToExchange", true);//通知可以交换位置
		} 

	}
	int CheckMatch(){

		for (int col = 0; col<mCol; col++) {
			for(int row = 0; row < mRow;row ++){
				Candy item0 = getCandy(col,row);
				Candy item1 = getCandy(col,row+1);
				Candy item2 = getCandy(col,row+2);
				if(null!= item0 && null != item1 && null != item2){
					if(item0.mIndex == item1.mIndex && item0.mIndex == item2.mIndex){
						if(!MatchList.Contains(item0))MatchList.Add(item0);
						if(!MatchList.Contains(item1))MatchList.Add(item1);
						if(!MatchList.Contains(item2))MatchList.Add(item2);
					}
				}
			}
		}
		for(int row = 0; row < mRow;row ++){
			for (int col = 0; col<mCol; col++) {
				Candy item0 = getCandy(col,row);
				Candy item1 = getCandy(col+1,row);
				Candy item2 = getCandy(col+2,row);
				if(null!= item0 && null != item1 && null != item2){
					if(item0.mIndex == item1.mIndex && item0.mIndex == item2.mIndex){
						if(!MatchList.Contains(item0))MatchList.Add(item0);
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
			Candy temp = MatchList[index] as Candy;
			removeCandy(temp);	
		}
	}
	void DestroyCandy(){

		ArrayList temp_list = new ArrayList ();
		while (0< MatchList.Count) {
			ArrayList temp_group = new ArrayList();
			Candy temp_item = MatchList[0] as Candy;
			for(int row = 0;row < MatchList.Count;row++){
				Candy item = MatchList[row] as Candy;
				if(item.mIndex == temp_item.mIndex){
					temp_group.Add(item);
				}
			}
			for(int index = 0;index < temp_group.Count;index++){
				Candy item = temp_group[index] as Candy;
				MatchList.Remove(item);
			}
			temp_list.Add(temp_group);
		}
		for (int index=0; index<temp_list.Count; index ++) {

			ArrayList templist = temp_list[index] as ArrayList;
			if(templist.Count>0 && templist.Count<3){
				print("Match Error!!!");
			}
			else{
				for(int row = 0;row<templist.Count;row++){
					Candy item = templist[row] as Candy;
					Destroy(item.gameObject);
				}
			}
		}
		print("Match Line:"+temp_list.Count.ToString());
		MatchList.Clear ();
	}
	void addCandys(){
		for (int col = 0; col < this.mCol; col++) {
			ArrayList temp = OPool[col] as ArrayList;
			int topOff = 0;
			for(int row = temp.Count;row < this.mRow;row++){
				Candy item = getRandomGameObject(col,row,new Vector3(BVector.x + col*xOff,BVector.y+(this.mRow+topOff)*yOff,0f));
				temp.Add(item);
				topOff++;
			}
		}
	}
	void resetPositon(){
		for (int col = 0; col < this.mCol; col++) {
			ArrayList temp = OPool[col] as ArrayList;
			int row = 0 ;
			for(;row < temp.Count;row++){		
				Candy temp_item = temp[row] as Candy;
				temp_item.mRow = row;
				temp_item.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0f);
			}
		}
	}
	void apply_adjust_postion(ArrayList items){
		if (2 == items.Count) {
			Candy item0 = items[0] as Candy;
			Candy item1 = items[1] as Candy;
			resetCandy(item0);
			resetCandy(item1);
			isExchange = true;
			isReadyToCheck = true;
			SendMessage ("notice_isReadyToExchange", false);//通知可以交换位置
		}
	}
	void apply_adjust_postion_0(ArrayList items){
		if (2 == items.Count) {
			Candy item0 = items[0] as Candy;
			Candy item1 = items[1] as Candy;
			resetCandy(item0);
			resetCandy(item1);

		}
	}
	IEnumerator Checking(){
		yield return new WaitForSeconds (0.5f);
		CheckMatch ();
	}
	IEnumerator waitAndCheck(){
		yield return new WaitForSeconds (0.5f);
		if (2 < CheckMatch ()) {
			removeMatchCandy ();
			yield return new WaitForSeconds (1f);
			DestroyCandy ();
			resetPositon();
			yield return new WaitForSeconds (1.5f);
			addCandys ();
			isReadyToCheck = true;
			isExchange = false;

		} else if (isExchange) { 
			SendMessage ("reset_pos");	
			isExchange = false;
		}
	}
	Candy getCandy(int col,int row){
		if (col < 0 || row < 0) {
			return null;		
		}
		if (col < this.mCol && row < this.mRow) {
			try{
				ArrayList temp = OPool[col] as ArrayList;
				return temp[row] as Candy;
			}
			catch{
				return null;
			}
		} else {
			return null;
		}
	}
	Candy resetCandy(Candy item){
		ArrayList temp = OPool[item.mCol] as ArrayList;
		Candy temp_object = temp[item.mRow] as Candy;
		temp[item.mRow] = item;
		return temp_object;
	}
	void removeCandy(Candy item){
		ArrayList temp = OPool[item.mCol] as ArrayList;
		temp.Remove (item);
	}
	bool isAllReady(){
		int total = 0;
		for (int col=0; col<this.mCol; col++) {
			for(int row = 0;row<this.mRow;row++){
				Candy item = getCandy(col,row);
				if(null != item && item.isStatic){
					total ++;
				}
			}
		}
		return (total == this.mCol * this.mRow);
	}
	/// <summary>
	/// 获取随机糖果
	/// </summary>
	/// <returns>The random game object.</returns>
	private Candy getRandomGameObject(int col,int row,Vector3 sPos){
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
		Candy ca = null;
		if (null != result) {
			result.transform.parent = this.transform;
			result.transform.position = sPos;
			ca = result.GetComponent (typeof(Candy)) as Candy;
			ca.mIndex = index;
			ca.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0);
			ca.mRow = row;
			ca.mCol = col;
		}
		return ca;
	}
}
