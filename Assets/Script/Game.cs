using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	//public
	public GameObject candy1;
	public GameObject candy2;
	public GameObject candy3;
	public GameObject candy4;
	public GameObject candy5;
	public float mSep = 0.318f;//物体间隔
	public Vector3 mBegin_Pos = new Vector3 (0f, 0f, 0f);//第一块所在位置
	public int Col_Count = 0;//列数量
	public int Row_Count = 0;//行数量

	//static
	public static int allUpdated = 0;//帧记录，方便一些有先后顺序的操作能按序进行
	//private
	private int grade = 0;//获得的分数
	private bool readyToDestroy = true;
	private bool readyToCheck = false;
	private bool readyToAdd = false;
	private bool isMoving = true;
	private List<CandyAction> Container;
	void Start () {
		float Re_X = mBegin_Pos.x;
		Container = new List<CandyAction> ();
		for (int row=0; row<this.Row_Count; ++row) {
			for(int col=0;col<this.Col_Count;++col){
				CandyAction instance = getRandomGameObject(col,row);
				if(null != instance){
					instance.gameObject.transform.position = new Vector3(mBegin_Pos.x+ col*mSep,mBegin_Pos.y+row*mSep,0f);
					Container.Add(instance);
				}
				else{
					Debug.Log("getRandomGameObject(col,row) Faild!");
				}
			}
		}

	}
	
	// Update is called once per frame
	void Update () {
		//执行销毁
		//每隔固定帧进行一次删除操作
		if (allUpdated > 4) {
			var query_moving_object = from CandyAction ca in Container where ca.isStatic == false select ca;
			print("Moving Object"+query_moving_object.Count ().ToString());
			//检查运动状态，所有物体静止的时候执行删除操作			
			if (query_moving_object.Count () == 0) {
				if(readyToCheck){
						readyToCheck = false;
						StartCoroutine(waitAndCheck());
				}
				if(readyToDestroy){
					readyToDestroy = false;
					StartCoroutine(waitAndDestroy());
				}
				if(readyToAdd){
					readyToAdd = false;
					StartCoroutine(waitAndReorder());
				}
			}
			allUpdated = 0;	
		}
		allUpdated++;
	}
	IEnumerator waitAndCheck(){
		yield return new WaitForSeconds (0.3f);
		checkMatch ();
		readyToCheck = false;
		readyToDestroy = true;
	}
	IEnumerator waitAndDestroy(){
		yield return new WaitForSeconds (0.2f);
		if (DestroyCandy ()) {
				StartCoroutine (waitOnly ());	
		} else {
				readyToAdd = true;
		}
		readyToDestroy = false;
	}
	IEnumerator waitAndReorder(){
		yield return new WaitForSeconds(0.1f);
		reOrder ();
		readyToAdd = false;
		readyToCheck = true;
	}
	IEnumerator waitOnly(){
		yield return new WaitForSeconds (0.3f);
		readyToAdd = true;
	}
	bool DestroyCandy(){
		int count = 0;
		List<CandyAction> temp = new List<CandyAction> ();
		for (int i = 0; i<Container.Count; i++) {
			Container[i].isReorder = false;
			if(Container[i].isDestroy){
				temp.Add(Container[i]);
				count++;
			}
		}
		for (int index = 0; index < temp.Count; index++) {
			Container.Remove(temp[index]);
			Destroy(temp[index].gameObject);
		}
		return count >0;
	}
	//重新编号
	void reOrder(){
		//重新编号
		float temp_x = mBegin_Pos.x;
		if (Container.Count <= this.Row_Count * this.Col_Count) {
			for (int col =0; col<Col_Count; col++) {
				//获取根据Y坐标排序后的每一列的游戏物体，以保证游戏物体是自下而上的编号是升序的
				var query_col = (from CandyAction ca in Container where ca.mCol == col && ca.isDestroy == false select ca).OrderBy ((CandyAction ca) => {
						return ca.gameObject.transform.position.y;});
				int row = 0;
				List<CandyAction> temp = query_col.ToList<CandyAction>();
				//重新编号
				for(int index = 0;index < temp.Count;index ++) {
					temp[index].mRow = row;
					temp[index].isDestroy = false;
					temp[index].isReorder = true;
					//temp[index].gameObject.transform.position = new Vector3(mBegin_Pos.x+col*mSep,mBegin_Pos.y+row*mSep,0f);
					temp[index].mPos = new Vector3(mBegin_Pos.x+col*mSep,mBegin_Pos.y+row*mSep,0f);
					row++;
				}
				//每列编号完成，根据行号决定每列应该生成新的游戏物体的数量并重新生成新的游戏物体
				for (; row<this.Row_Count; row++) {
					CandyAction new_object = getRandomGameObject (col, row);
					Container.Add(new_object);
					new_object.gameObject.transform.position = new Vector3(mBegin_Pos.x+col*mSep,mBegin_Pos.y+(this.Row_Count+row)*mSep,0f);
				}
			}
		}
	}
	void OnGUI(){
		GUILayout.BeginArea (new Rect (500, 100, 140, 140));
		GUILayout.Label ("分数：" + this.grade.ToString ());
		GUILayout.Label ("时间(秒)：" + Time.realtimeSinceStartup.ToString("f2").Replace('.','：'));
		GUILayout.EndArea ();
	}
	void checkMatch(){
		var query_destroyed = from CandyAction item in Container where item.isDestroy == true select item;
		var query_reorder = from CandyAction item in Container where item.isReorder == true select item;
		if (query_destroyed.Count() == 0 && Container.Count == this.Row_Count*this.Col_Count && query_reorder.Count() == this.Row_Count*this.Col_Count) {
			for (int index = 0;index<Container.Count;index++) {
				//获取左边邻居
				CandyAction leftObject = getCandyObject (Container[index].mCol - 1, Container[index].mRow);
				//获取右边邻居
				CandyAction rightObject = getCandyObject (Container[index].mCol + 1, Container[index].mRow);
				//获取上邻居
				CandyAction topObject = getCandyObject (Container[index].mCol, Container[index].mRow + 1);
				//获取下邻居
				CandyAction bottomObject = getCandyObject (Container[index].mCol, Container[index].mRow - 1);
				//判断是否应该被销毁
				//左右邻居
				if (null != leftObject && leftObject.mIndex == Container[index].mIndex) {
					if (null != rightObject && rightObject.mIndex == Container[index].mIndex) {
								Container[index].isDestroy = true;
								leftObject.isDestroy = true;
								rightObject.isDestroy = true;
						}
				} 
				//上下邻居
				if (null != topObject && topObject.mIndex == Container[index].mIndex) {
					if (null != bottomObject && bottomObject.mIndex == Container[index].mIndex) {
						Container[index].isDestroy = true;
								topObject.isDestroy = true;
								bottomObject.isDestroy = true;
						}
				}
			}
		}
		readyToDestroy = true;
		readyToAdd = false;
	}
	CandyAction getCandyObject(int col,int row){
		var query = from CandyAction ca in Container 
			where  ca.mCol == col && ca.mRow == row
				select ca;
		CandyAction temp_Object = query.Count () > 0 ? query.First () : null;
		return temp_Object;
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
			result = Instantiate(candy1) as GameObject;
			break;
		}
		case 1:{	
			result = Instantiate(candy2) as GameObject;
			break;
		}
		case 2:{	
			result = Instantiate(candy3) as GameObject;
			break;
		}
		case 3:{	
			result = Instantiate(candy4) as GameObject;
			break;
		}
		case 4:{	
			result = Instantiate(candy5) as GameObject;
			break;
		}
		default:break;
		}
		CandyAction ca = null;
		if (null != result) {
			ca = result.GetComponent (typeof(CandyAction)) as CandyAction;
			ca.mIndex = index;
			ca.mPos = new Vector3(mBegin_Pos.x+col*mSep,mBegin_Pos.y+row*mSep,0);
			ca.mRow = row;
			ca.mCol = col;
			ca.isDestroy = false;
		}
		return ca;
	}
}
