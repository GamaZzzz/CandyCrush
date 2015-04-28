using UnityEngine;
using System.Collections;

public class ActionControoler : MonoBehaviour {

	public float xOff = 0f;//x轴偏移量
	public float yOff = 0f;//y轴偏移量

	private bool isReadyToExchange =true;
	private ArrayList exchangeList;
	private CandyAction exchangeItem;
	void Start () {
		exchangeItem = null;
		exchangeList = new ArrayList ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void reset_pos(){
		if (exchangeList.Count == 2) {
			CandyAction item0 = exchangeList [0] as CandyAction;
			CandyAction item1 = exchangeList[1] as CandyAction;
			//
			exchange_pos(item0,item1);
			//
			SendMessage("apply_adjust_postion_0",exchangeList);//请求更新在数组中的位置
			//
			isReadyToExchange = true;
		}
	}
	void apply_exchange_pos(CandyAction item){
		//
		if (null != exchangeItem && item != exchangeItem && isReadyToExchange) {
			//列判断是否可以交换位置
			if (Mathf.Approximately (item.mPos.y, exchangeItem.mPos.y)) {
				if (Mathf.Approximately (item.mPos.x + xOff, exchangeItem.mPos.x) || Mathf.Approximately (item.mPos.x - xOff, exchangeItem.mPos.x)) {
					exchangeList.Clear();//清空交换列表
					//
					exchange_pos(exchangeItem,item);//交换位置
					//
					exchangeList.Add (item);
					exchangeList.Add (exchangeItem);
					//
					isReadyToExchange = false;
					SendMessage("apply_adjust_postion",exchangeList);//请求更新在数组中的位置
					
				} else {
					exchangeItem = item;
				}
				
			} else if (Mathf.Approximately (item.mPos.x, exchangeItem.mPos.x)) {//行判断是否可以交换位置
				if (Mathf.Approximately (item.mPos.y + yOff, exchangeItem.mPos.y) || Mathf.Approximately (item.mPos.y - yOff, exchangeItem.mPos.y)) {
					exchangeList.Clear();//清空交换列表
					//
					exchange_pos(exchangeItem,item);//交换位置
					//
					exchangeList.Add (item);
					exchangeList.Add (exchangeItem);
					//
					isReadyToExchange = false;
					SendMessage("apply_adjust_postion",exchangeList);//请求更新在数组中的位置
				} else {
					exchangeItem = item;
				}
			} else {
				exchangeItem = item;
			}
		} else {
			exchangeItem = item;
		}
	}
	//
	void exchange_pos(CandyAction item0,CandyAction item1){
		//item0.mPos = item1.transform.position;
		//item1.mPos = item0.transform.position;

		Vector3 temp_pos = item0.mPos;
		item0.mPos = item1.mPos;
		item1.mPos = temp_pos;
		//
		int temp_row = item0.mRow;
		int temp_col = item0.mCol;
		//
		item0.mCol = item1.mCol;
		item0.mRow = item1.mRow;
		//
		item1.mRow = temp_row;
		item1.mCol = temp_col;
	}
}
