using UnityEngine;
using System;
using System.Collections;

public class ActionController : MonoBehaviour {

	public float xOff = 0f;//x轴偏移量
	public float yOff = 0f;//y轴偏移量
	//
	private bool isReadyToExchange =true;
	//
	private ArrayList exchangeList;
	//
	private Candy exchangeItem;
	//
	public delegate void ControllEventHandler(object sender,ControllEventArgs e);
	private event ControllEventHandler ControllEvent;
	//
	public class ControllEventArgs:EventArgs{
		public ControllEventArgs(){
			this.srcCandy = null;
			this.destCandy = null;
			this.mType = 0;
		}
		public ControllEventArgs(Candy src,Candy dest,int pType){
			this.srcCandy = src;
			this.destCandy = dest;
			this.mType = pType;
		}
		public Candy srcCandy;
		public Candy destCandy;
		public int mType;
	}
	//
	void Start () {
		exchangeItem = null;
		exchangeList = new ArrayList ();
	}
	//
	void Update () {
		
	}
//
	public void isReadyCallback(object sender,GameController.GameControllerEventArgs e){
		switch(e.mOper){
		case GameController._OPERATIONS.EXCHANGE:{
			isReadyToExchange = (e.mStatus == GameController._STATUS.READY);
			break;
		}
		case GameController._OPERATIONS.DESTROY:{
			break;
		}
		case GameController._OPERATIONS.RESET:{
			if (exchangeList.Count == 2) {
				Candy item0 = exchangeList [0] as Candy;
				Candy item1 = exchangeList[1] as Candy;
				//
				exchange_pos(item0,item1);
				//
				this.onExchange(new ControllEventArgs(item0,item1,3));
				//
				exchangeItem = null;
				//
				this.isReadyToExchange = true;
				//
				item0.setChosen(false);
				item1.setChosen(false);
			}
			break;
		}
		default:break;
		}
	}
	//
	public void ExchangeEventCallback(object sender,Candy.ExchangeEventArgs e){
		Candy item = sender as Candy;
		//交换位置判断
		if(isReadyToExchange){
			if (null != exchangeItem ){
				if(item != exchangeItem && item.mIndex!=exchangeItem.mIndex){
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
							
							if(item.isSpecial && exchangeItem.isSpecial){//两个特殊糖果交换
								this.onExchange(new ControllEventArgs(exchangeItem,item,2));
							}else if(item.isSpecial || exchangeItem.isSpecial){//其中一个是特殊糖果
								//任意一个是彩色糖果则触发彩色糖果特效
								
								if(item.mType == GameController._TYPE.COLORFUL || exchangeItem.mType == GameController._TYPE.COLORFUL){
									//SendMessage("apply_trigger_special_corlorful_candy",exchangeList);
									this.onExchange(new ControllEventArgs(exchangeItem,item,1));
								}
								else{
									//SendMessage("apply_adjust_postion",exchangeList);//请求更新在数组中的位置
									this.onExchange(new ControllEventArgs(exchangeItem,item,0));
								}
							}else{//普通糖果则请求更新在数组中的位置
								//SendMessage("apply_adjust_postion",exchangeList);//请求更新在数组中的位置
								this.onExchange(new ControllEventArgs(exchangeItem,item,0));
							}
							
							//
							exchangeItem = null;
						}
                        else
                        {
                            exchangeItem.setChosen(false);
                            exchangeItem = item;
                            exchangeItem.setChosen(true);
                        }

                    }
                    else if (Mathf.Approximately(item.mPos.x, exchangeItem.mPos.x))
                    {//行判断是否可以交换位置
                        if (Mathf.Approximately(item.mPos.y + yOff, exchangeItem.mPos.y) ||
                            Mathf.Approximately(item.mPos.y - yOff, exchangeItem.mPos.y))
                        {
                            exchangeList.Clear();//清空交换列表
                            //
                            exchange_pos(exchangeItem, item);//交换位置
                            //
                            exchangeList.Add(item);
                            exchangeList.Add(exchangeItem);
                            //
                            isReadyToExchange = false;
                            //
                            //如果是彩色糖果则请求触发彩色特殊糖果的效果
                            if (item.isSpecial && exchangeItem.isSpecial)
                            {//两个特殊糖果交换
                                this.onExchange(new ControllEventArgs(exchangeItem, item, 2));
                            }
                            else if (item.isSpecial || exchangeItem.isSpecial)
                            {
                                if (item.mType == GameController._TYPE.COLORFUL || exchangeItem.mType == GameController._TYPE.COLORFUL)
                                {
                                    this.onExchange(new ControllEventArgs(exchangeItem, item, 1));
                                }
                                else
                                {
                                    this.onExchange(new ControllEventArgs(exchangeItem, item, 0));
                                }
                            }
                            else
                            {//普通糖果则请求更新在数组中的位置
                                this.onExchange(new ControllEventArgs(exchangeItem, item, 0));//请求更新在数组中的位置
                            }
                            //
                            exchangeItem = null;
                        }
                        else
                        {
                            exchangeItem.setChosen(false);
                            exchangeItem = item;
                            exchangeItem.setChosen(true);
                        }
                    }
                    else
                    {
                        exchangeItem.setChosen(false);
                        exchangeItem = item;
                        exchangeItem.setChosen(true);
                    }
				}else{
					exchangeItem.setChosen(false);
                    exchangeItem = item;
					item.setChosen (true);
					
				}
			} else {
				item.setChosen (true);
				exchangeItem = item;
			}
		}
	}
	//交换位置及索引
	void exchange_pos(Candy item0,Candy item1){

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
	//
	public void AttachEventCallback(ControllEventHandler ceh){
		this.ControllEvent += ceh;
	}
	public void DetachEventCallback(ControllEventHandler ceh){
		this.ControllEvent -= ceh;
	}
	protected virtual void onExchange(ControllEventArgs e){
		if (null != ControllEvent) {
			ControllEvent(this,e);	
		}
	}
}
