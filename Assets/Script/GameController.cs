using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	//public
	public GameObject T;
	//
	public List<Sprite> normalCandys;//普通糖果
	public List<Sprite> streakHCandys;//斑纹横纹糖果
	public List<Sprite> streakVCandys;//斑纹纵纹糖果
	public List<Sprite> packageCandys;//包装糖果
	public List<Sprite> colorfulCandys;//彩色糖果
	public List<Sprite> specialCandys;//t特殊糖果序列
	//
	public Vector3 BVector = Vector3.zero;//矩阵原点位置
	public Vector3 SVector = Vector3.zero;//物体生成初始位置
	//
	public float xOff = 0f;//x轴偏移量
	public float yOff = 0f;//y轴偏移量
	//
	public int mCol = 0;//列数
	public int mRow = 0;//行数
	//private
	private bool isReadyToCheck = false;//检测开关
	private ArrayList OPool;//物体列表
	private ArrayList MatchList;//匹配列表
	private bool isExchange = false;//交换标识

	//糖果类型
	public enum _TYPE{
		NORMAL = 0,
		STREAKH = 1,
		STREAKV = 2,
		PACKAGE = 3,
		COLORFUL = 4
	}
	public struct SCandy{
		public int mCol;
		public int mRow;
		public _TYPE mType;
		public int mIndex;
		public void setValue(int pCol,int pRow,_TYPE pType,int pIndex){
			this.mCol = pCol;
			this.mRow = pRow;
			this.mType = pType;
			this.mIndex = pIndex;		
		}
	}
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
	// Update is called once per frame
	void Update () {
		//所有物体的位置到达指定位置并且匹配列表没有可以删除的物体之后进行匹配检测
		if (isReadyToCheck  &&  MatchList.Count == 0 && isAllReady () ) {
			//
			isReadyToCheck = false;//进入匹配步骤，关闭检测开关
			//
			StartCoroutine (waitAndCheck ());//进行匹配和消除相关操作
		} 
	}
	//
	IEnumerator waitAndCheck(){
		yield return new WaitForSeconds (0.5f);
		//
		if (2 < CheckMatch ()) {

			yield return new WaitForSeconds (1f);
			//对匹配列表进行分组
			ArrayList temp_list = group_match_candys(ref this.MatchList);
			//判断是否会产生特殊糖果，并返回特殊糖果标识
			List<SCandy> speciallist = hasSpecialCandy(ref temp_list);
			//生成特殊糖果
			if(speciallist.Count >0){

			}
			//从数组中移除匹配糖果
			removeMatchCandy (ref temp_list);
			//分组删除糖果
			for(int group =0;group < temp_list.Count;group++){
				ArrayList temp_items = temp_list[group] as ArrayList;
				//检测匹配组里的特殊糖果
				List<Candy> temp_special_list = matchSpecialCandy(ref temp_items);
				//如果存在特殊糖果，则表示需要触发特殊效果
				if(0<temp_special_list.Count){
					
				}
				DestroyCandy(temp_items);
				yield return new WaitForSeconds (0.5f);
			}
			//
			resetPositon ();
			yield return new WaitForSeconds (1f);
			//
			addCandys ();
			isReadyToCheck = true;//打开检测开关
			isExchange = false;//重置交换状态
			yield return new WaitForSeconds (0.5f);
			//
		} else if (isExchange) { //如果上次检测配对情况是由于交换位置产生的并且没有可以消除的，则通知重置已交换的两个物体的位置
			//
			SendMessage ("reset_pos");	
			isExchange = false;
			
		} else {//直到没有可以消除的才允许移动位置
			SendMessage ("notice_isReadyToExchange", true);//通知可以交换位置
		}
	}
	//
	void InitPool(int pcol,int prow,ref ArrayList pool){
		pool = new ArrayList (pcol);
		//
		for (int col=0; col<pcol; col++) {
			ArrayList temp = new ArrayList(prow);
			pool.Add(temp);
		}
	}
	void InitCandys(Vector3 initPos){
		for (int col = 0; col < this.mCol; col++) {
			ArrayList temp = OPool[col] as ArrayList;
			int row = 0 ;
			//
			for(;row < temp.Count;row++){		
				Candy temp_item = temp[row] as Candy;
				temp_item.mRow = row;
				temp_item.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0f);
			}
			//
			for(;row < this.mRow;row++){
				int index = Random.Range(0,normalCandys.Count);
				Candy item = NewCandy(col,row,SVector,_TYPE.NORMAL,index);
				temp.Add(item);
			}
		}
	}
	private int CheckMatch(){

		for (int col = 0; col<mCol; col++) {
			for(int row = 0; row < mRow;row ++){
				//
				Candy item0 = getCandy(col,row);
				//
				if(null != item0){
					//列检测
					Candy item1 = getCandy(col,row+1);
					Candy item2 = getCandy(col,row+2);
					//
					if(null != item1 && null != item2){
						if(item0.mIndex == item1.mIndex && item0.mIndex == item2.mIndex){

							AddToMatchList(item0);

							AddToMatchList(item1);

							AddToMatchList(item2);

						}
					}
					//行检测
					Candy item3 = getCandy(col+1,row);
					Candy item4 = getCandy(col+2,row);
					//
					if(null!= item3 && null != item4){
						if(item0.mIndex == item3.mIndex && item0.mIndex == item4.mIndex){

							AddToMatchList(item0);

							AddToMatchList(item3);

							AddToMatchList(item4);

						}
					}
				}	
			}
		}
		return MatchList.Count;
	}
	//添加到匹配列表
	private int AddToMatchList(Candy item){
		item.setDark ();
		if (!MatchList.Contains (item)) {
			MatchList.Add(item);		
		}
		return MatchList.Count;
	}
	//在对象池中移除匹配物体
	private void removeMatchCandy(){
		for (int index = 0; index<MatchList.Count; index++) {
			Candy temp = MatchList [index] as Candy;
			removeCandy (temp);	
		}
	}
	private void removeMatchCandy(ref ArrayList match_grouped_list){
		//
		for (int col =0; col<match_grouped_list.Count; col++) {
			ArrayList temp_list = match_grouped_list[col] as ArrayList;
			for (int index = 0; index<temp_list.Count; index++) {
				Candy temp = temp_list [index] as Candy;
				removeCandy (temp);	
			}
		}
	}
	//销毁匹配物体
	private void DestroyCandy(){
		//
		for (int index = 0; index<this.MatchList.Count; index++) {
			Candy temp = MatchList[index] as Candy;
			Destroy(temp.gameObject);
		}
		MatchList.Clear ();
	}
	//消除糖果
	private void DestroyCandy(ArrayList templist){
		
		List<string> temp_debug_list = new List<string>();//Debug
		//
		if (templist.Count > 0 && templist.Count < 3) {
			print ("Match Error!!!");
		}
		else{
			//
			for(int row = 0;row<templist.Count;row++){
				Candy item = templist[row] as Candy;
				
				temp_debug_list.Add(item.mIndex.ToString());
				
				Destroy(item.gameObject);
			}
			//
		}
	}
	//获取匹配组里的特殊糖果
	List<Candy> matchSpecialCandy(ref ArrayList inputlist){
		List<Candy> result = new List<Candy> ();
		for (int index =0; index<inputlist.Count; index++) {
			Candy item = inputlist[index] as Candy;
			if(item.isSpecial){
				result.Add(item);
			}
		}
		return result;
	}
	//检测是否会产生特殊糖果(inputlist 分组后的待消除糖果)
	List<SCandy> hasSpecialCandy(ref ArrayList inputlist){
		List<SCandy> result = new List<SCandy> ();
		for (int index =0; index<inputlist.Count; index++) {
			//
			ArrayList temp_list = inputlist [index] as ArrayList;
			int random_index = Random.Range (0, temp_list.Count);
			Candy item = temp_list [random_index] as Candy;
			//
			if(3<temp_list.Count){
				int sum_row = 0;
				int sum_col = 0;
				//
				for(int i = 0;i<temp_list.Count;i++){
					Candy temp_item = temp_list[i] as Candy;
					if(item.mRow==temp_item.mRow){
						sum_row ++;
					}else if(item.mCol == temp_item.mCol){
						sum_col ++;
					}
				}
				switch(temp_list.Count){
				case 4:{
					if(4 == sum_col){
						SCandy temp_item = new SCandy();
						temp_item.setValue(item.mCol,item.mRow,_TYPE.STREAKV,item.mIndex);
						result.Add(temp_item);
					}
					else if(4 == sum_row){
						SCandy temp_item = new SCandy();
						temp_item.setValue(item.mCol,item.mRow,_TYPE.STREAKH,item.mIndex);
						result.Add(temp_item);
					}
					break;
				}
				case 5:{
					if(5 == sum_col || 5 == sum_row){
						SCandy temp_item = new SCandy();
						temp_item.setValue(item.mCol,item.mRow,_TYPE.COLORFUL,item.mIndex);
						result.Add(temp_item);
					}
					else{
						SCandy temp_item = new SCandy();
						temp_item.setValue(item.mCol,item.mRow,_TYPE.COLORFUL,item.mIndex);
						result.Add(temp_item);
					}
					break;
				}
				default:break;
				}
			}
		}
		return result;
	}
	//对匹配列表进行分组
	private ArrayList group_match_candys(ref ArrayList pList){

		ArrayList temp_list = new ArrayList ();
		//fen zu
		while (0< pList.Count) {
			ArrayList temp_group = new ArrayList();
			Candy temp_item = pList[0] as Candy;
			temp_group.Add(temp_item);
			pList.Remove(temp_item);
			//
			for(int row = 0;row < pList.Count;row++){
				Candy item = pList[row] as Candy;
				//
				if(item.mIndex == temp_item.mIndex){
					temp_group.Add(item);
					pList.Remove(item);
					row = -1;
				}
				//
			}
			temp_list.Add(temp_group);
		}
		//
		return temp_list;
	}
	//添加普通糖果
	void addCandys(){
		//
		for (int col = 0; col < this.mCol; col++) {

			ArrayList temp = OPool[col] as ArrayList;
			//
			int topOff = 0;
			//
			for(int row = temp.Count;row < this.mRow;row++){
				Vector3 temp_vector3 = new Vector3(BVector.x + col*xOff,BVector.y+(this.mRow+topOff)*yOff,0f);
				int index = Random.Range(0,normalCandys.Count);
				Candy item = NewCandy(col,row,temp_vector3,_TYPE.NORMAL,index);
				temp.Add(item);
				topOff++;
			}//
		}
	}
	//根据特殊糖果记录列表添加特殊糖果
	void addSpecial(List<SCandy> speciallist){
		for (int index=0; index<speciallist.Count; index++) {
			SCandy temp_struct = speciallist[index];
			Vector3 vpos = new Vector3(BVector.x+temp_struct.mCol*xOff,BVector.y+temp_struct.mRow*yOff,0f);
			Candy item = NewCandy(temp_struct.mCol,temp_struct.mRow,vpos,temp_struct.mType,temp_struct.mIndex);
			setCandy(item);
		}
	}
	//获取所有特殊糖果
	private List<Candy> findSpecialCandys(){
		List<Candy> result = new List<Candy> ();
		for (int col=0; col<this.mCol; col++) {
			for(int row = 0; row < this.mRow;row++){		
				Candy item =getCandy(col,row);
				if(null!=item && item.isSpecial){
					result.Add(item);
				}
			}
		}
		return result;
	}
	//获取某一类型的所有糖果
	private List<Candy> findCandysOfType(Candy pitem){
		List<Candy> result = new List<Candy> ();
		for (int col=0; col<this.mCol; col++) {
			for(int row = 0; row < this.mRow;row++){		
				Candy item =getCandy(col,row);
				if(null!=item && !item.isSpecial && item.mIndex==pitem.mIndex){
					result.Add(item);
				}
			}
		}
		return  result;
	}
	//获取某一糖果周围8个糖果
	private List<Candy> getRound8(Candy item){
		List<Candy> result = new List<Candy> ();
		//
		Candy top_item = getCandy (item.mCol, item.mRow + 1);
		if (null != top_item) result.Add (top_item);
		//
		Candy bottom_item = getCandy (item.mCol, item.mRow - 1);
		if (null != bottom_item)
						result.Add (bottom_item);
		//
		Candy left_item = getCandy (item.mCol - 1, item.mRow);
		if (null != left_item)
						result.Add (left_item);
		//
		Candy right_item = getCandy (item.mCol + 1, item.mRow);
		if (null != right_item)
						result.Add (right_item);
		//
		Candy left_top_item = getCandy (item.mCol - 1, item.mRow + 1);
		if (null != left_top_item)
						result.Add (left_top_item);
		//
		Candy left_bottom_item = getCandy (item.mCol - 1, item.mRow - 1);
		if (null != left_bottom_item)
						result.Add (left_bottom_item);
		//
		Candy right_top_item = getCandy (item.mCol + 1, item.mRow + 1);
		if (null != right_top_item)
						result.Add (right_top_item);
		//
		Candy right_bottom_item = getCandy (item.mCol + 1, item.mRow - 1);
		if (null != right_bottom_item)
						result.Add (right_bottom_item);
		
		return result;
	}
	//获取同一列的糖果
	List<Candy> getSameCol(Candy item){
		ArrayList temp_arraylist = OPool [item.mCol] as  ArrayList;
		List<Candy> temp_list = new List<Candy> ();
		for (int row=0; row <temp_arraylist.Count; row++) {
			Candy temp_item = temp_arraylist[row] as Candy;
			temp_list.Add(temp_item);
		}
		return temp_list;
	}
	//获取同一行的糖果
	List<Candy> getSameRow(Candy item){
		List<Candy> temp_list = new List<Candy> ();
		for (int col = 0; col<this.OPool.Count; col++) {
			Candy temp_item = getCandy(col,item.mRow);
			if(null!=temp_item) temp_list.Add(temp_item);
		}
		return temp_list;
	}
	//重新排列糖果
	private void resetPositon(){
		for (int col = 0; col < this.mCol; col++) {
			ArrayList temp = OPool[col] as ArrayList;
			int row = 0 ;
			//
			for(;row < temp.Count;row++){		
				Candy temp_item = temp[row] as Candy;
				temp_item.mRow = row;
				temp_item.mPos = new Vector3(BVector.x+col*xOff,BVector.y+row*yOff,0f);
				temp_item.setChosen(false);
			}

		}

	}
	//调整交换位置的糖果在数组中的位置
	void apply_adjust_postion(ArrayList items){
		if (2 == items.Count) {
			Candy item0 = items[0] as Candy;
			Candy item1 = items[1] as Candy;
			//
			setCandy(item0);
			setCandy(item1);
			//
			isExchange = true;//设置交换状态为真
			isReadyToCheck = true;//打开检测开关
			//
			SendMessage ("notice_isReadyToExchange", false);//通知不可以交换位置
		}
	}
	//
	void apply_adjust_postion_0(ArrayList items){
		if (2 == items.Count) {
			Candy item0 = items[0] as Candy;
			Candy item1 = items[1] as Candy;
			//
			setCandy(item0);
			setCandy(item1);
		}
	}
	//请求消除特殊糖果
	void apply_trigger_special_corlorful_candy(ArrayList items){
		apply_adjust_postion_0 (items);
		if (2 == items.Count) {
			Candy temp_item = items[0] as Candy;
			//
			temp_item = temp_item.isSpecial?temp_item:items[1] as Candy;
			//
			if(temp_item.isSpecial){
				if(0<CheckMatch()){
					
				}
			}
		}
	}
	//获取某一序列对的糖果
	Candy getCandy(int col,int row){
		if (col < 0 || row < 0) {
			return null;		
		}
		//
		if (col < this.mCol && row < this.mRow) {
			ArrayList temp = OPool[col] as ArrayList;
			//
			if(row<temp.Count){
				return temp[row] as Candy;
			}else{	
				return null;
			}
			//
		} else {
			return null;
		}
	}
	//设置糖果在数组中的位置
	Candy setCandy(Candy item){
		ArrayList temp = OPool[item.mCol] as ArrayList;
		Candy temp_object = temp[item.mRow] as Candy;
		//
		temp[item.mRow] = item;
		//
		return temp_object;
	}
	//
	void removeCandy(Candy item){
		ArrayList temp = OPool[item.mCol] as ArrayList;
		temp.Remove (item);
	}
	//
	bool isAllReady(){
		int total = 0;
		//
		for (int col=0; col<this.mCol; col++) {
			for(int row = 0;row<this.mRow;row++){
				Candy item = getCandy(col,row);
				if(null != item && item.isStatic){
					total ++;
				}
			}
		}
		//
		return (total == this.mCol * this.mRow);
	}
	//
	private Candy NewCandy(int col,int row ,Vector3 sPos,_TYPE ptype,int type_Index){
		//
		GameObject go = Instantiate (T) as GameObject;
		//
		Sprite sprite = this.getSpecialSprite (ptype, type_Index);
		//
		SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
		sr.sprite = sprite;
		//
		Candy ca = null;
		if (null != go) {
			go.transform.parent = this.transform;
			go.transform.position = sPos;
			ca = go.GetComponent (typeof(Candy)) as Candy;
			ca.mIndex = type_Index;
			ca.mType = ptype;
			ca.isSpecial = (!(ptype==_TYPE.NORMAL));
			ca.mPos = new Vector3 (BVector.x + col * xOff, BVector.y + row * yOff, 0);
			ca.mRow = row;
			ca.mCol = col;
		}
		//
		return ca;
	}
	//
	private Sprite getSpecialSprite(_TYPE ptype,int index){
		Sprite result = null;
		switch (ptype) {
		case _TYPE.NORMAL:{
			result = this.normalCandys[index];
			break;
		}
		case _TYPE.STREAKH:{
			result = streakHCandys[index];
			break;
		}
		case _TYPE.STREAKV:{
			result = streakVCandys[index];
			break;
		}
		case _TYPE.PACKAGE:{
			result = packageCandys[index];
			break;
		}
		case _TYPE.COLORFUL:{
			result = colorfulCandys[index];
			break;
		}
		default:break;
		}
		return result;
	}
}
