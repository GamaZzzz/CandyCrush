using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    //public
    public GameObject T;
    //
    public List<Sprite> normalCandys;//普通糖果
    public List<Sprite> streakHCandys;//斑纹横纹糖果
    public List<Sprite> streakVCandys;//斑纹纵纹糖果
    public List<Sprite> packageCandys;//包装糖果
    public List<Sprite> colorfulCandys;//彩色糖果
    //
    public Vector3 BVector = Vector3.zero;//矩阵原点位置
    public Vector3 SVector = Vector3.zero;//物体生成初始位置
    //
    public float xOff = 0f;//x轴偏移量
    public float yOff = 0f;//y轴偏移量
    public float mWait = 0.3f;//动作时间间隔
    //
    public int mCol = 0;//列数
    public int mRow = 0;//行数
    //private
    private bool isReadyToCheck = false;//检测开关
    private bool isExchange = false;//交换标识
    //
    private ArrayList ShowList;//物体列表
    private ArrayList MatchList;//匹配列表
    //
    private List<Candy> DestroyList;//消除列表
    private List<Candy> RecycleList;//回收列表 
    //
    private ActionController ac;//
    //
    public enum _OPERATIONS
    {
        NONE = 0,//无动作
        RESET = 1,//重置位置
        NEW = 2,//新生成糖果
        DESTROY = 3,//销毁糖果
        EXCHANGE = 4,//交换糖果
        MATCH = 5,//匹配糖果
        GROUP = 6,//分组
        DESPLAY = 7//显示
    }
    //
    public enum _STATUS
    {
        READY = 0,//准备
        CHECKING = 1,//正在检测
        DESTROYING = 2,//正在销毁
        BUSY = 3//未知繁忙
    }
    //糖果类型
    public enum _TYPE
    {
        NORMAL = 0,//普通糖果
        STREAKH = 1,//斑纹横纹糖果
        STREAKV = 2,//斑纹纵纹糖果
        PACKAGE = 3,//包装糖果
        COLORFUL = 4//彩色糖果
    }
    //
    public struct SCandy
    {
        public int mCol;//列序号
        public int mRow;//行序号
        public _TYPE mType;//糖果类型
        public int mIndex;//糖果索引
        public SCandy(int pCol, int pRow, _TYPE pType, int pIndex)
        {
            this.mCol = pCol;
            this.mRow = pRow;
            this.mType = pType;
            this.mIndex = pIndex;
        }
    }
    //
    public delegate void GameControllEnventHandler(object sender, GameControllerEventArgs e);
    private event GameControllEnventHandler GameControllerEvents;
    //
    public class GameControllerEventArgs : EventArgs
    {
        public GameControllerEventArgs()
        {
            mStatus = _STATUS.READY;
        }
        public GameControllerEventArgs(_STATUS pStatus, _OPERATIONS pOper)
        {
            this.mStatus = pStatus;
            this.mOper = pOper;
        }
        public _STATUS mStatus;
        public _OPERATIONS mOper;
    }
    //
    void Start()
    {
        //
        RecycleList = new List<Candy>();
        //
        ac = this.gameObject.GetComponent<ActionController>();
        ac.AttachEventCallback(this.ControllEventCallback);
        //
        this.AttachEventHandler(ac.isReadyCallback);
        //
        InitPool(this.mCol, this.mRow, ref this.ShowList);
        InitCandys(SVector);
        //
        MatchList = new ArrayList();
        //
        DestroyList = new List<Candy>();
        //
        while (0 < CheckMatch())
        {
            removeMatchCandy();

            DestroyCandy();

            InitCandys(SVector);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isReadyToCheck && MatchList.Count == 0 && isAllReady())
        {
            //
            isReadyToCheck = false;//进入匹配步骤，关闭检测开关
            //
            StartCoroutine(waitAndCheck());//进行匹配和消除相关操作
        }
    }
    //模式2
    IEnumerator waitAndCheck()
    {
        yield return new WaitForSeconds(mWait * 2f);
        //进行检测
        if (2 < CheckMatch())
        {

            yield return new WaitForSeconds(mWait);
            //对匹配列表进行分组
            ArrayList temp_list = group_match_candys(ref this.MatchList);
            //判断是否会产生特殊糖果，并返回特殊糖果标识
            List<SCandy> speciallist = hasSpecialCandy(ref temp_list);
            //生成特殊糖果
            if (speciallist.Count > 0)
            {
                addSpecial(speciallist);
            }
            //分组删除糖果
            for (int group = 0; group < temp_list.Count; group++)
            {
                ArrayList temp_items = temp_list[group] as ArrayList;
                //检测匹配组里的特殊糖果
                List<Candy> temp_special_list = matchSpecialCandy(ref temp_items);
                //如果存在特殊糖果，则表示需要触发特殊效果
                specialRemove(ref temp_special_list, ref temp_items);
                //从数组中移除匹配糖果
                removeMatchCandy(ref temp_items);
                DestroyCandy(ref temp_items);
                yield return new WaitForSeconds(mWait);
            }
            //
            resetPositon();
            //
            addCandys();
            isReadyToCheck = true;//打开检测开关
            isExchange = false;//重置交换状态
            yield return new WaitForSeconds(mWait);
            //
        }
        else if (isExchange)
        { 	//如果上次检测配对情况是由于交换位置产生的并且没有可以消除的，则通知重置已交换的两个物体的位置
            //	
            this.onReadyCallback(new GameControllerEventArgs(_STATUS.READY, _OPERATIONS.RESET));
            isExchange = false;

        }
        else
        {//直到没有可以消除的才允许移动位置
            this.onReadyCallback(new GameControllerEventArgs(_STATUS.READY, _OPERATIONS.EXCHANGE));
        }
    }
    //消除由彩色糖果产生的消除列表
    IEnumerator waitAndRemoveColorful()
    {
        removeCandy(DestroyList);
        //
        yield return new WaitForSeconds(mWait);
        //
        DestroyCandy(DestroyList);
        //
        yield return new WaitForSeconds(mWait);
        //
        resetPositon();
        //
        addCandys();
        isReadyToCheck = true;//打开检测开关
        isExchange = false;//重置交换状态
        yield return new WaitForSeconds(mWait);
    }
    //
    void InitPool(int pcol, int prow, ref ArrayList pool)
    {
        pool = new ArrayList(pcol);
        //
        for (int col = 0; col < pcol; col++)
        {
            ArrayList temp = new ArrayList(prow);
            pool.Add(temp);
        }
    }
    void InitCandys(Vector3 initPos)
    {
        for (int col = 0; col < this.mCol; col++)
        {
            ArrayList temp = ShowList[col] as ArrayList;
            int row = 0;
            //
            for (; row < temp.Count; row++)
            {
                Candy temp_item = temp[row] as Candy;
                temp_item.mRow = row;
                temp_item.mPos = new Vector3(BVector.x + col * xOff, BVector.y + row * yOff, 0f);
            }
            //
            for (; row < this.mRow; row++)
            {
                int index = UnityEngine.Random.Range(0, normalCandys.Count);
                Candy item = NewCandy(col, row, SVector, _TYPE.NORMAL, index);
                temp.Add(item);
            }
        }
    }
    //检测匹配
    private int CheckMatch()
    {

        for (int col = 0; col < mCol; col++)
        {
            for (int row = 0; row < mRow; row++)
            {
                //
                Candy item0 = getCandy(col, row);
                //
                if (null != item0 && _TYPE.COLORFUL != item0.mType)
                {
                    //列检测
                    Candy item1 = getCandy(col, row + 1);
                    Candy item2 = getCandy(col, row + 2);
                    //
                    if (null != item1 && _TYPE.COLORFUL != item1.mType && null != item2 && _TYPE.COLORFUL != item2.mType)
                    {
                        if (item0.mIndex == item1.mIndex && item0.mIndex == item2.mIndex)
                        {

                            AddToMatchList(item0);

                            AddToMatchList(item1);

                            AddToMatchList(item2);
                        }
                    }
                    //行检测
                    Candy item3 = getCandy(col + 1, row);
                    Candy item4 = getCandy(col + 2, row);
                    //
                    if (null != item3 && _TYPE.COLORFUL != item3.mType && null != item4 && _TYPE.COLORFUL != item4.mType)
                    {
                        if (item0.mIndex == item3.mIndex && item0.mIndex == item4.mIndex)
                        {

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
    private int AddToMatchList(Candy item)
    {
        item.setDark(true);
        if (!MatchList.Contains(item))
        {
            MatchList.Add(item);
        }
        return MatchList.Count;
    }
    private int AddToMatchList(List<Candy> item_list)
    {
        for (int index = 0; index < item_list.Count; index++)
        {
            AddToMatchList(item_list[index]);
        }
        return MatchList.Count;
    }
    //添加到消除列表
    private int addToDestroyList(Candy item)
    {
        item.setDark(true);
        if (!DestroyList.Contains(item))
        {
            DestroyList.Add(item);
        }
        return DestroyList.Count;
    }
    private int addToDestroyList(List<Candy> item_list)
    {
        for (int index = 0; index < item_list.Count; index++)
        {
            addToDestroyList(item_list[index]);
        }
        return DestroyList.Count;
    }
    //
    private bool needToDestroy()
    {
        return (this.DestroyList.Count > 0);
    }
    //
    void specialRemove(ref List<Candy> specialcandys, ref ArrayList temp_group_list)
    {

        for (int index = 0; index < specialcandys.Count; index++)
        {

            Candy temp_item = specialcandys[index];

            switch (temp_item.mType)
            {

                case _TYPE.STREAKH:
                    {

                        List<Candy> same_row = getSameRow(temp_item);

                        for (int col = 0; col < same_row.Count; col++)
                        {
                            if (!temp_group_list.Contains(same_row[col]))
                            {
                                same_row[col].setDark(true);
                                temp_group_list.Add(same_row[col]);
                            }
                        }
                        break;
                    }
                case _TYPE.STREAKV:
                    {

                        List<Candy> same_col = getSameCol(temp_item);

                        for (int row = 0; row < same_col.Count; row++)
                        {
                            if (!temp_group_list.Contains(same_col[row]))
                            {
                                same_col[row].setDark(true);
                                temp_group_list.Add(same_col[row]);
                            }
                        }
                        break;
                    }
                case _TYPE.PACKAGE:
                    {

                        List<Candy> same_round = getRound8(temp_item);

                        for (int col = 0; col < same_round.Count; col++)
                        {
                            if (!temp_group_list.Contains(same_round[col]))
                            {
                                same_round[col].setDark(true);
                                temp_group_list.Add(same_round[col]);
                            }
                        }
                        break;
                    }
                default: break;
            }
        }
    }
    //在对象池中移除匹配物体
    private void removeMatchCandy()
    {
        for (int index = 0; index < MatchList.Count; index++)
        {
            Candy temp = MatchList[index] as Candy;
            removeCandy(temp);
        }
    }
    private void removeMatchCandy(ref ArrayList match_grouped_list)
    {
        //
        for (int index = 0; index < match_grouped_list.Count; index++)
        {
            Candy temp = match_grouped_list[index] as Candy;
            removeCandy(temp);
        }
    }
    //销毁匹配物体
    private void DestroyCandy()
    {
        //
        for (int index = 0; index < this.MatchList.Count; index++)
        {
            Candy temp = MatchList[index] as Candy;
            DestroyCandy(temp);
        }
        MatchList.Clear();
    }
    private void DestroyCandy(Candy item)
    {
        if (null != item)
        {
            item.gameObject.SetActive(false);
            AddToRecycleList(item);
        }
    }
    //消除糖果
    private void DestroyCandy(ref ArrayList templist)
    {
        //
        if (templist.Count > 0 && templist.Count < 3)
        {
            print("Match Error!!!");
        }
        else
        {
            //
            for (int row = 0; row < templist.Count; row++)
            {
                Candy item = templist[row] as Candy;
                //
                DestroyCandy(item);
            }
            //
        }
        templist.Clear();
    }
    //消除糖果
    private void DestroyCandy(List<Candy> templist)
    {
        //
        if (templist.Count > 0 && templist.Count < 3)
        {
            print("Match Error!!!");
        }
        else
        {
            //
            for (int row = 0; row < templist.Count; row++)
            {

                Candy item = templist[row];
                DestroyCandy(item);
            }
            //
        }
        templist.Clear();
    }
    //
    private int AddToRecycleList(Candy item)
    {
        if (!RecycleList.Contains(item))
        {
            RecycleList.Add(item);
        }
        return RecycleList.Count;
    }
    //重复使用已有的游戏对象
    private Candy reuseCandy(_TYPE pType, int pIndex)
    {
        Candy result = null;
        result = RecycleList.Find((x) => (x.mType == pType && x.mIndex == pIndex));
        if (null != result)
        {
            RecycleList.Remove(result);
        }
        return result;
    }
    //获取匹配组里的特殊糖果
    List<Candy> matchSpecialCandy(ref ArrayList inputlist)
    {
        List<Candy> result = new List<Candy>();
        for (int index = 0; index < inputlist.Count; index++)
        {
            Candy item = inputlist[index] as Candy;
            if (item.isSpecial)
            {
                result.Add(item);
            }
        }
        return result;
    }
    //检测是否会产生特殊糖果(inputlist 分组后的待消除糖果)
    List<SCandy> hasSpecialCandy(ref ArrayList inputlist)
    {
        List<SCandy> result = new List<SCandy>();
        for (int index = 0; index < inputlist.Count; index++)
        {
            //
            ArrayList temp_list = inputlist[index] as ArrayList;
            int random_index = UnityEngine.Random.Range(0, temp_list.Count);
            Candy item = temp_list[random_index] as Candy;
            //
            if (3 < temp_list.Count)
            {
                int sum_row = 0;
                int sum_col = 0;
                //
                for (int i = 0; i < temp_list.Count; i++)
                {
                    Candy temp_item = temp_list[i] as Candy;

                    if (item.mRow == temp_item.mRow)
                    {
                        sum_row++;
                    }
                    if (item.mCol == temp_item.mCol)
                    {
                        sum_col++;
                    }
                }
                //匹配组等于4
                if (4 == temp_list.Count)
                {
                    if (4 == sum_col)
                    {

                        SCandy temp_item = new SCandy(item.mCol, item.mRow, _TYPE.STREAKV, item.mIndex);
                        result.Add(temp_item);

                    }
                    else if (4 == sum_row)
                    {

                        SCandy temp_item = new SCandy(item.mCol, item.mRow, _TYPE.STREAKH, item.mIndex);
                        result.Add(temp_item);

                    }
                }
                else if (5 == temp_list.Count)
                {//匹配组等于5
                    if (5 == sum_col || 5 == sum_row)
                    {

                        SCandy temp_item = new SCandy(item.mCol, item.mRow, _TYPE.COLORFUL, item.mIndex);
                        result.Add(temp_item);

                    }
                    else
                    {

                        SCandy temp_item = new SCandy(item.mCol, item.mRow, _TYPE.PACKAGE, item.mIndex);
                        result.Add(temp_item);

                    }
                }
                else if (5 < temp_list.Count && sum_col == 4)
                {//匹配组大于5

                    SCandy temp_item = new SCandy(item.mCol, item.mRow, _TYPE.STREAKV, item.mIndex);
                    result.Add(temp_item);

                }
                else if (5 < temp_list.Count && sum_row == 4)
                {

                    SCandy temp_item = new SCandy(item.mCol, item.mRow, _TYPE.STREAKH, item.mIndex);
                    result.Add(temp_item);

                }
                else if (5 < temp_list.Count && (5 == sum_col || 5 == sum_row))
                {

                    SCandy temp_item = new SCandy(item.mCol, item.mRow, _TYPE.COLORFUL, item.mIndex);
                    result.Add(temp_item);

                }
            }
        }
        return result;
    }
    //对匹配列表进行分组
    private ArrayList group_match_candys(ref ArrayList pList)
    {

        ArrayList temp_list = new ArrayList();
        //fen zu
        while (0 < pList.Count)
        {
            ArrayList temp_group = new ArrayList();
            Candy temp_item = pList[0] as Candy;
            //
            temp_group.Add(temp_item);
            pList.Remove(temp_item);
            //
            for (int row = 0; row < pList.Count; row++)
            {
                Candy item = pList[row] as Candy;
                //
                if (item.mIndex == temp_item.mIndex)
                {
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
    void addCandys()
    {
        //
        for (int col = 0; col < this.mCol; col++)
        {

            ArrayList temp = ShowList[col] as ArrayList;
            //
            int topOff = 0;
            //
            for (int row = temp.Count; row < this.mRow; row++)
            {

                Vector3 temp_vector3 = new Vector3(BVector.x + col * xOff, BVector.y + (this.mRow + topOff) * yOff, 0f);
                int index = UnityEngine.Random.Range(0, normalCandys.Count);
                Candy item = NewCandy(col, row, temp_vector3, _TYPE.NORMAL, index);

                temp.Add(item);
                topOff++;
            }//
        }
    }
    //根据特殊糖果记录列表添加特殊糖果
    void addSpecial(List<SCandy> speciallist)
    {
        for (int index = 0; index < speciallist.Count; index++)
        {

            SCandy temp_struct = speciallist[index];
            Vector3 vpos = new Vector3(BVector.x + temp_struct.mCol * xOff, BVector.y + temp_struct.mRow * yOff, 0f);
            Candy item = NewCandy(temp_struct.mCol, temp_struct.mRow, vpos, temp_struct.mType, temp_struct.mIndex);

            setCandy(item);
        }
    }
    //获取所有特殊糖果
    private List<Candy> findSpecialCandys()
    {
        List<Candy> result = new List<Candy>();
        for (int col = 0; col < this.mCol; col++)
        {
            for (int row = 0; row < this.mRow; row++)
            {

                Candy item = getCandy(col, row);

                if (null != item && item.isSpecial)
                {
                    result.Add(item);
                }
            }
        }
        return result;
    }
    //获取某一类型的所有糖果
    private List<Candy> findCandysOfType(Candy pitem)
    {

        List<Candy> result = new List<Candy>();

        for (int col = 0; col < this.mCol; col++)
        {
            for (int row = 0; row < this.mRow; row++)
            {

                Candy item = getCandy(col, row);

                if (null != item && !item.isSpecial && item.mIndex == pitem.mIndex)
                {
                    result.Add(item);
                }
            }
        }
        return result;
    }
    //获取某一糖果周围8个糖果
    private List<Candy> getRound8(Candy item)
    {

        List<Candy> result = new List<Candy>();
        //
        Candy top_item = getCandy(item.mCol, item.mRow + 1);
        if (null != top_item) result.Add(top_item);
        //
        Candy bottom_item = getCandy(item.mCol, item.mRow - 1);
        if (null != bottom_item)
            result.Add(bottom_item);
        //
        Candy left_item = getCandy(item.mCol - 1, item.mRow);
        if (null != left_item)
            result.Add(left_item);
        //
        Candy right_item = getCandy(item.mCol + 1, item.mRow);
        if (null != right_item)
            result.Add(right_item);
        //
        Candy left_top_item = getCandy(item.mCol - 1, item.mRow + 1);
        if (null != left_top_item)
            result.Add(left_top_item);
        //
        Candy left_bottom_item = getCandy(item.mCol - 1, item.mRow - 1);
        if (null != left_bottom_item)
            result.Add(left_bottom_item);
        //
        Candy right_top_item = getCandy(item.mCol + 1, item.mRow + 1);
        if (null != right_top_item)
            result.Add(right_top_item);
        //
        Candy right_bottom_item = getCandy(item.mCol + 1, item.mRow - 1);
        if (null != right_bottom_item)
            result.Add(right_bottom_item);

        return result;
    }
    //获取同一列的糖果
    List<Candy> getSameCol(Candy item)
    {

        ArrayList temp_arraylist = ShowList[item.mCol] as ArrayList;
        List<Candy> temp_list = new List<Candy>();

        for (int row = 0; row < temp_arraylist.Count; row++)
        {

            Candy temp_item = temp_arraylist[row] as Candy;
            temp_list.Add(temp_item);

        }

        return temp_list;
    }
    //获取同一行的糖果
    List<Candy> getSameRow(Candy item)
    {

        List<Candy> temp_list = new List<Candy>();

        for (int col = 0; col < this.ShowList.Count; col++)
        {

            Candy temp_item = getCandy(col, item.mRow);
            if (null != temp_item) temp_list.Add(temp_item);

        }
        return temp_list;
    }
    //重新排列糖果
    private void resetPositon()
    {
        for (int col = 0; col < this.mCol; col++)
        {

            ArrayList temp = ShowList[col] as ArrayList;
            int row = 0;
            //
            for (; row < temp.Count; row++)
            {

                Candy temp_item = temp[row] as Candy;

                temp_item.mRow = row;
                temp_item.mPos = new Vector3(BVector.x + col * xOff, BVector.y + row * yOff, 0f);
                temp_item.setChosen(false);
            }
        }
    }
    //
    void ControllEventCallback(object sender, ActionController.ControllEventArgs e)
    {

        Candy item0 = e.srcCandy;
        Candy item1 = e.destCandy;
        //
        setCandy(item0);
        setCandy(item1);
        //
        switch (e.mType)
        {
            case 0:
                {
                    isExchange = true;//设置交换状态为真
                    isReadyToCheck = true;//打开检测开关
                    //
                    this.onReadyCallback(new GameControllerEventArgs(_STATUS.BUSY, _OPERATIONS.EXCHANGE));//通知不可以交换位置
                    //
                    break;
                }
            case 1:
                {
                    //
                    addToDestroyList(item0);
                    //
                    addToDestroyList(item1);
                    //
                    Candy temp_item = (!item0.isSpecial) ? item0 : item1;
                    //
                    List<Candy> item_list = findCandysOfType(temp_item);
                    //
                    addToDestroyList(item_list);
                    //
                    StartCoroutine(this.waitAndRemoveColorful());
                    break;
                }
            case 2:
                {
                    break;
                }
            case 3:
                {
                    setCandy(item0);
                    setCandy(item1);
                    break;
                }
            default: break;
        }
    }
    //获取某一序列对的糖果
    Candy getCandy(int col, int row)
    {
        if (col < 0 || row < 0)
        {
            return null;
        }
        //
        if (col < this.mCol && row < this.mRow)
        {
            ArrayList temp = ShowList[col] as ArrayList;
            //
            if (row < temp.Count)
            {
                return temp[row] as Candy;
            }
            else
            {
                return null;
            }
            //
        }
        else
        {
            return null;
        }
    }
    //设置糖果在数组中的位置
    Candy setCandy(Candy item)
    {
        ArrayList temp = ShowList[item.mCol] as ArrayList;
        Candy temp_object = temp[item.mRow] as Candy;
        //
        temp[item.mRow] = item;
        //
        return temp_object;
    }
    //从当前游戏对象列表中移除游戏物体
    private void removeCandy(Candy item)
    {
        ArrayList temp = ShowList[item.mCol] as ArrayList;
        temp.Remove(item);
    }
    //
    private void removeCandy(List<Candy> plist)
    {
        for (int index = 0; index < plist.Count; index++)
        {
            removeCandy(plist[index]);
        }
    }
    //
    bool isAllReady()
    {
        int total = 0;
        //
        for (int col = 0; col < this.mCol; col++)
        {
            for (int row = 0; row < this.mRow; row++)
            {
                Candy item = getCandy(col, row);
                if (null != item && item.isStatic)
                {
                    total++;
                }
            }
        }
        //
        return (total == this.mCol * this.mRow);
    }
    //新建糖果
    private Candy NewCandy(int col, int row, Vector3 sPos, _TYPE ptype, int type_Index)
    {

        Candy ca = this.reuseCandy(ptype, type_Index);
        if (null == ca)
        {
            //
            GameObject go = Instantiate(T) as GameObject;
            //
            Sprite sprite = this.getSpecialSprite(ptype, type_Index);
            //
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            //
            if (null != go)
            {
                go.transform.parent = this.transform;
                go.transform.position = sPos;
                ca = go.GetComponent(typeof(Candy)) as Candy;
                ca.mIndex = type_Index;
                ca.mType = ptype;
                ca.isSpecial = (!(ptype == _TYPE.NORMAL));
                ca.mPos = new Vector3(BVector.x + col * xOff, BVector.y + row * yOff, 0);
                ca.mRow = row;
                ca.mCol = col;
                ca.AttachEventCallback(ac.ExchangeEventCallback);
            }
        }
        else
        {
            ca.gameObject.SetActive(true);
            ca.gameObject.transform.position = sPos;
            ca.isSpecial = (!(ptype == _TYPE.NORMAL));
            ca.mPos = new Vector3(BVector.x + col * xOff, BVector.y + row * yOff, 0);
            ca.mRow = row;
            ca.mCol = col;
            ca.setDark(false);
            ca.setChosen(false);
        }
        //
        return ca;
    }
    //
    private Sprite getSpecialSprite(_TYPE ptype, int index)
    {
        Sprite result = null;
        switch (ptype)
        {
            case _TYPE.NORMAL:
                {
                    result = this.normalCandys[index];
                    break;
                }
            case _TYPE.STREAKH:
                {
                    result = streakHCandys[index];
                    break;
                }
            case _TYPE.STREAKV:
                {
                    result = streakVCandys[index];
                    break;
                }
            case _TYPE.PACKAGE:
                {
                    result = packageCandys[index];
                    break;
                }
            case _TYPE.COLORFUL:
                {
                    result = colorfulCandys[index];
                    break;
                }
            default: break;
        }
        return result;
    }
    //
    protected void onReadyCallback(GameControllerEventArgs e)
    {
        if (null != this.GameControllerEvents)
        {
            this.GameControllerEvents(this, e);
        }
    }
    //
    public void AttachEventHandler(GameControllEnventHandler geh)
    {
        this.GameControllerEvents += geh;
    }
    public void DetachEventHandler(GameControllEnventHandler geh)
    {
        this.GameControllerEvents -= geh;
    }
    void OnDestroy()
    {

        if (null != this.GameControllerEvents)
        {
            this.DetachEventHandler(ac.isReadyCallback);
        }

        if (ac != null)
        {
            ac.DetachEventCallback(this.ControllEventCallback);

            for (int col = 0; col < this.mCol; col++)
            {
                for (int row = 0; row < this.mRow; row++)
                {

                    Candy item = getCandy(col, row);

                    if (null != item)
                    {
                        item.DetachEventCallback(ac.ExchangeEventCallback);
                        DestroyCandy(item);
                        item = null;
                    }

                }
            }
            for (int index = 0; index < this.RecycleList.Count; index++)
            {
                Candy temp = RecycleList[index];
                temp.DetachEventCallback(ac.ExchangeEventCallback);
                DestroyCandy(temp);
                temp = null;
            }
        }
    }
}