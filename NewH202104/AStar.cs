using System;
using System.IO;
using System.Collections;
using Tanis.Collections;

namespace Huangbo.AStarPetri
{
    public class MyCost
    {
        #region Properties

        public decimal[] cost;

        #endregion

        #region Constructors

        public MyCost(int n)
        {
            cost = new decimal[n];
        }

        public MyCost(params decimal[] costs)
        {
            cost = costs;
            
        }

        public MyCost(MyCost temp)
        {
            cost = temp.cost;
        }

        #endregion

        #region Public Methods

        public void Equal(MyCost temp)
        {
            cost = temp.cost;
        }

        public void Equal(params decimal[] costs)
        {
            cost = costs;
        }

        public void AddCost(MyCost temp)
        {
            for(int i = 0; i < temp.cost.Length; i++)
            {
                cost[i] += temp.cost[i];
            }
        }

        public bool Equals(MyCost obj)
        {
            for(int i = 0; i < obj.cost.Length; i++)
            {
                if (cost[i] != obj.cost[i]) return false;
            }
            return true;
        }

        //1：a支配b
        //-1：a被b支配
        //0：不互相支配
        //2: a和b相同
        public static int Dominate(MyCost a, MyCost b)
        {
            int maybedominated = 0; //被支配可能标记
            int maydominate = 0; //支配可能标记
            for(int i = 0; i < a.cost.Length; i++)
            {
                if (a.cost[i] > b.cost[i]) maybedominated = 1;
                if (a.cost[i] < b.cost[i]) maydominate = 1;
            }
            if (maybedominated == 1)
            {
                if (maydominate == 0) return -1;
                else return 0;
            }
            else
            {
                if (maydominate == 1) return 1;
                else return 2;
            }
        }

        #endregion

        #region Overridden Methods

        public static MyCost operator +(MyCost a, MyCost b)
        {
            //安全校验：防止空指针
            if (a == null || b == null || a.cost == null || b.cost == null)
            {
                throw new ArgumentNullException("操作数或其内部的 cost 数组不能为空！");
            }

            //长度校验：如果两个数组长度不一致，直接相加会导致越界崩溃
            if (a.cost.Length != b.cost.Length)
            {
                throw new ArgumentException("两个 cost 数组的长度必须相同才能相加！");
            }

            //创建全新的对象和数组，绝不污染原始数据
            MyCost result = new MyCost(a.cost.Length);

            //执行相加逻辑
            for (int i = 0; i < a.cost.Length; i++)
            {
                result.cost[i] = a.cost[i] + b.cost[i];
            }

            //返回全新的对象
            return result;
        }

        public static MyCost operator -(MyCost a, MyCost b)
        {
            //安全校验：防止空指针
            if (a == null || b == null || a.cost == null || b.cost == null)
            {
                throw new ArgumentNullException("操作数或其内部的 cost 数组不能为空！");
            }

            //长度校验：如果两个数组长度不一致，直接相加会导致越界崩溃
            if (a.cost.Length != b.cost.Length)
            {
                throw new ArgumentException("两个 cost 数组的长度必须相同才能相加！");
            }

            //创建全新的对象和数组，绝不污染原始数据
            MyCost result = new MyCost(a.cost.Length);

            //执行相加逻辑
            for (int i = 0; i < a.cost.Length; i++)
            {
                result.cost[i] = a.cost[i] - b.cost[i];
            }

            //返回全新的对象
            return result;
        }


        #endregion

    }

    public class AStarNode : IComparable //Petri网模型可达图中的状态节点
    {
        #region Properties

        public AStarNode Parent//父节点
        {
            set
            {
                FParent = value;
            }
            get
            {
                return FParent;
            }
        }
        private AStarNode FParent;

        public AStarNode GoalNode //目标节点
        {
            set
            {
                FGoalNode = value;
            }
            get
            {
                return FGoalNode;
            }
        }
        private AStarNode FGoalNode;

        public MyCost Cost //g值(The accumulative cost of the path until now.)
        {
            set
            {
                FCost = new MyCost(value);
            }
            get
            {
                return FCost;
            }
        }
        private MyCost FCost;

        public MyCost GoalEstimate //h值(The estimated cost to the goal from here.)
        {
            set
            {
                FGoalEstimate = new MyCost(value);
            }
            get
            {
                return FGoalEstimate;
            }
        }
        private MyCost FGoalEstimate;

        public MyCost TotalCost//f值(The cost plus the estimated cost to the goal from here.)
        {
            set
            {
                FTotalCost = new MyCost(value);
            }
            get
            {
                return FTotalCost;
            }
        }
        private MyCost FTotalCost;

        public int[] M//节点的标识
        {
            get
            {
                return FM;
            }
        }
        private int[] FM;

        public MyCost[] Mr//该标识下所有位置的剩余处理时间
        {
            get
            {
                return FMr;
            }
        }
        private MyCost[] FMr;

        public int TFireFrom//产生本标识所实施的变迁
        {
            get
            {
                return FTFireFrom;
            }
            set
            {
                FTFireFrom = value;
            }
        }
        private int FTFireFrom;

        public int[] EnabledTransitions//本标识中可实施变迁的集合
        {
            get
            {
                return FEnabledTransitions;
            }
            set
            {
                Array.Copy(value, FEnabledTransitions, value.Length);
            }
        }
        private int[] FEnabledTransitions;

        public int MarkingDepth//标识深度
        {
            get
            {
                return FMarkingDepth;
            }
            set
            {
                FMarkingDepth = value;
            }
        }
        private int FMarkingDepth;

        public MyCost Delt//从父标识到某变迁实施得到本标识所用时间
        {//从父marking到transition实施得到本marking所需时间
            set
            {
                FDelt = new MyCost(value);
            }
            get
            {
                return FDelt;
            }
        }
        private MyCost FDelt;

        //private int[] u = new int[AStar.nt];//控制向量u
        private MyCost delt = new MyCost();//变迁变为可实施所需时间

        //通过变迁的发射放入输出库所中的托肯必须等待一定时间后才可利用，并且该时间等于该库所的时延
        // M(k)- 和 Mr(k)- 分别表示：变迁发射前，那刻 "系统的标识" 和 "剩余处理时间向量"
        // M(k)+ 和 Mr(k)+ 分别表示：变迁发射后，输入托肯已移走但输出托肯还未加入时 "系统的标识" 和 "剩余处理时间向量"
        private int[] MF = new int[AStar.np];//标识状态M(k)-
        private int[] MZ = new int[AStar.np];//标识状态M(k)+
        private MyCost[] MrF = new MyCost[AStar.np];//标识状态Mr(k)-
        private MyCost[] MrZ = new MyCost[AStar.np];//标识状态Mr(k)+

        #endregion

        #region Constructors

        //AStarNode(父节点, 目标节点, g值, h值, f值, 节点的标志, 该标识下所有位置的剩余处理时间, 产生本标识所实施的变迁, 标志的深度, 从父标识到变迁实施得到本标识所用时间)
        public AStarNode(AStarNode AParent, AStarNode AGoalNode, MyCost ACost, MyCost AGoalEstimate, MyCost ATotalCost, int[] AM, MyCost[] AMr, int ATFireFrom, int AMarkingDepth, MyCost ADelt)
        {
            FParent = AParent;
            FGoalNode = AGoalNode;
            FCost = ACost;
            FGoalEstimate = AGoalEstimate;
            FTotalCost = ATotalCost;
            FM = new int[AStar.np];
            Array.Copy(AM, FM, AM.Length);
            FMr = new MyCost[AStar.np];
            Array.Copy(AMr, FMr, AMr.Length);
            FTFireFrom = ATFireFrom;
            FEnabledTransitions = new int[AStar.nt];
            FMarkingDepth = AMarkingDepth;
            FDelt = ADelt;
        }
        #endregion

        #region Public Methods

        public bool IsGoal()
        {//判断本节点的M和Mr是否与GoalNode一致
            if (!IsSameStatePlusMr(FGoalNode))//判断M和Mr是否相等
                return false;
            for (int i = 0; i < this.Mr.Length; ++i)
                if (!this.Mr[i].Equals(FGoalNode.Mr[i]))
                    return false;
            return true;
        }

        public virtual bool IsSameState(AStarNode ANode)//判断某节点的标识M是否和本节点一致
        {//只判断M
            if (ANode == null)
                return false;
            if (FM.Length != ANode.FM.Length)
                return false;
            for (int i = 0; i < FM.Length; ++i)
                if (FM[i] != ANode.FM[i])
                    return false;
            return true;
        }

        public virtual bool IsSameStatePlusMr(AStarNode ANode)//判断某节点的M和Mr是否和本节点一致
        {//判断M和Mr
            if (!IsSameState(ANode))
                return false;
            if (FMr.Length != ANode.FMr.Length)
                return false;
            for (int i = 0; i < FMr.Length; ++i)
                if (!FMr[i].Equals(ANode.FMr[i]))
                    return false;
            return true;
        }

        public virtual bool IsSameNode(AStarNode ANode)//判断某节点是否和本节点一致
        {
            if (!IsSameStatePlusMr(ANode)) //判断M和Mr
                return false;
            if (!Cost.Equals(ANode.Cost)) //判断g值
                return false;
            if (TFireFrom != ANode.TFireFrom) //父变迁
                return false;
            if (MarkingDepth != ANode.MarkingDepth) //节点深度
                return false;
            return true;
        }

        public virtual bool DominateMr(AStarNode ANode)//判断某节点的Mr是否被本节点支配（true：相同或支配）
        {
            if (ANode == null)
                return false;
            if (FMr.Length != ANode.FMr.Length)
                return false;
            for (int i = 0; i < FMr.Length; ++i)
                if (MyCost.Dominate(FMr[i], ANode.FMr[i]) == 0 || MyCost.Dominate(FMr[i], ANode.FMr[i]) == -1)
                    return false;
            return true;
        }

        public virtual MyCost CalculateH(int method)//Calculates the estimated cost for the remaining trip to the goal.
        //h0=0;
        //h1=next hop;
        //h2=max{ei(m)};
        {

            //h0=0;
            if (method == 0)
            {
                return new MyCost();
            }
            
            //写h方法的地方











            else
                return new MyCost();
        }

        public virtual void FindEnabledTransitions()//寻找当前标识（FM[i]）下可实施的变迁，并对EnabledTransitions赋值（1：可以实施，0：不能实施）
        {
            int e;
            for (int j = 0; j < AStar.nt; ++j)
            {
                e = 1;
                for (int i = 0; i < AStar.np; ++i)
                {
                    if (AStar.LMINUS[i, j] != 0 && FM[i] < AStar.LMINUS[i, j]) //变迁可以实施的条件：当前标志的库所大于变迁所需的输入库所（ M(p) > I(p, t) ）
                    {
                        e = 0;
                        continue;
                    }
                }
                EnabledTransitions[j] = e; //EnabledTransitions = new int[AStar.nt];
            }
        }

        public virtual void GetSuccessors(ArrayList ASuccessors, int hmethod) //获得当前节点的所有子节点，并添加到列表中
        {
            //寻找当前标识下可实施的变迁
            FindEnabledTransitions();

            for (int i = 0; i < FEnabledTransitions.Length; ++i)
            {
                

                
            }//for循环结束

        }

        public virtual void PrintNodeInfo(int loop) //打印当前节点的信息
        {
            
            Console.WriteLine();
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            return IsSameState((AStarNode)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return 1;
            //return (TotalCost.Equals( (obj as AStarNode).TotalCost) );
        }

        #endregion
    }

    /// <summary>
    /// Base class for pathfinding nodes, it holds no actual information about the map. 
    /// An inherited class must be constructed from this class and all virtual methods must be 
    /// implemented. Note, that calling base() in the overridden methods is not needed.
    /// </summary>
    public sealed class AStar //Petri网模型运行所需的全局属性和行为
    {
        #region Private Fields
        private AStarNode FStartNode;//起始节点
        private AStarNode FGoalNode;//目标节点
        private Heap FOpenList;//Open列表
        private Heap FClosedList;//Close列表
        private ArrayList FSuccessors;//子节点列表
        private ArrayList FExpandedList;//已扩展节点列表
        private ArrayList FSolution;//结果方案列表
        private int FNExpandedNode;//已扩展节点数
        private ArrayList FSolution_Goals;//结果方案目标

        #endregion

        #region Properties
        public static MyCost[] t;//各库所的操作代价
        public static int[,] LPLUS;//关联矩阵L+
        public static int[,] LMINUS;//关联矩阵L-

        public static int np;//Petri网中位置数(含资源)
        public static int nt;//Petri网中变迁数
        public static int nrs;//Petri网中资源位置数
        public static int max_attribute_num; //Petri网中属性数量

        public static int[] StartM;//开始节点的标识向量
        public static int[] GoalM;//目标节点的标识向量
        public static MyCost[] StartMr;//开始节点的剩余处理时间向量
        public static MyCost[] GoalMr;//目标节点的剩余处理时间向量

        public ArrayList Solution//结果方案列表
        {
            get
            {
                return FSolution;
            }
        }
        public int NExpandedNode//已扩展节点数
        {
            get
            {
                return FNExpandedNode;
            }
            set
            {
                FNExpandedNode = value;
            }
        }

        #endregion

        #region Constructors

        public AStar(string initfile, string matrixfile)//构造函数
        {
            FOpenList = new Heap();
            FClosedList = new Heap();
            FSuccessors = new ArrayList();
            FSolution = new ArrayList();
            FExpandedList = new ArrayList();
            FSolution_Goals = new ArrayList();

            Read_initfile(initfile);
            Read_matrixfile(matrixfile);

            /*
            Console.WriteLine("Petri网中位置数(含资源)：" + np);
            Console.WriteLine("Petri网中变迁数：" + nt);
            Console.WriteLine("Petri网中资源位置数：" + nrs);
            Console.WriteLine("初始marking：");
            for (int i = 0; i < np; i++)
            {
                Console.Write(StartM[i] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("目标marking：");
            for (int i = 0; i < np; i++)
            {
                Console.Write(GoalM[i] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("伴随矩阵L+：");
            for (int i = 0; i < np; ++i)
            {
                for (int j = 0; j < nt; ++j)
                {
                    Console.Write(LPLUS[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("伴随矩阵L-：");
            for (int i = 0; i < np; ++i)
            {
                for (int j = 0; j < nt; ++j)
                {
                    Console.Write(LMINUS[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            */

            StartMr = new MyCost[np];
            GoalMr = new MyCost[np];
            for (int i = 0; i < np; ++i)
            {
                StartMr[i] = new MyCost();
                GoalMr[i] = new MyCost();
            }

        }

        #endregion

        #region Private Methods

        private static void Read_initfile(string filename)
        {
            StreamReader SR;
            try
            {
                SR = File.OpenText(filename);
            }
            catch
            {
                Console.Write(filename + " open failed!");
                return;
            }
            string S;
            string[] SubS;

            //init文件的第一行
            {
                S = SR.ReadLine();
                SubS = S.Split(new char[] { ' ' });//string[]不指定大小就可以使用

                //Petri网中位置数(含资源)
                np = SubS.Length;

                //初始marking
                StartM = new int[np];
                for (int i = 0; i < SubS.Length; ++i)
                {
                    StartM[i] = Convert.ToInt32(SubS[i]);
                }
            }

            //init文件的第二行
            {
                S = SR.ReadLine();
                SubS = S.Split(new char[] { ' ' });

                //Petri网中资源位置数
                nrs = 0;
                t = new MyCost[np]; //各位置的操作代价
                for (int i = 0; i < SubS.Length; ++i)
                {
                    t[i] = new MyCost();
                    if (SubS[i].Length != 1)
                    {
                        for(int j = 0;j<AStar.max_attribute_num;j++)
                        t[i].cost[j] = Convert.ToInt32(SubS[i].Split(new char[] { ',' })[j]);
                        nrs++;
                    }
                }
                /*t = new decimal[np]; //各位置的操作时间
                for (int i = 0; i < SubS.Length; ++i)
                {
                    if (Convert.ToInt32(SubS[i]) != 0)
                    {
                        t[i] = Convert.ToInt32(SubS[i]);
                        nrs++;
                    }
                }*/
            }

            //init文件的第三行
            {
                S = SR.ReadLine();
                SubS = S.Split(new char[] { ' ' });

                //目标marking
                GoalM = new int[np];
                for (int i = 0; i < SubS.Length; ++i)
                {
                    GoalM[i] = Convert.ToInt32(SubS[i]);
                }
            }

            SR.Close();
            return;
        }

        private static void Read_matrixfile(string filename)
        {
            StreamReader SR;
            try
            {
                SR = File.OpenText(filename);
            }
            catch
            {
                Console.Write(filename + " open failed!");
                return;
            }
            string S;

            //Petri网中变迁数
            nt = 0;

            S = SR.ReadLine();
            while (S != null)
            {
                ++nt;
                S = SR.ReadLine();
            }
            SR.Close();

            StreamReader SRR;
            try
            {
                SRR = File.OpenText(filename);
            }
            catch
            {
                Console.Write(filename + " open failed!");
                return;
            }

            //临时矩阵 nt * np
            int[,] temp = new int[nt, np];

            S = SRR.ReadLine();
            string[] SubS;
            int n = 0;
            while (S != null)
            {
                SubS = S.Split(new char[] { ' ' });
                for (int i = 0, j = 0; i < SubS.Length && j < np; ++i)
                {
                    if (SubS[i].Equals("1"))
                    {
                        temp[n, j] = 1;
                        ++j;
                    }
                    else if (SubS[i].Equals("0"))
                    {
                        temp[n, j] = 0;
                        ++j;
                    }
                    else if (SubS[i].Equals("-1"))
                    {
                        temp[n, j] = -1;
                        ++j;
                    }
                }
                S = SRR.ReadLine();
                n++;
            }

            /*//matri.txt输入矩阵
            for (int i = 0; i<nt; ++i)
            {
                for (int j = 0; j < np; ++j)
                {
                    Console.Write(temp[i, j] + " ");
                }
                Console.WriteLine();
            }*/

            //伴随矩阵L+
            LPLUS = new int[np, nt];

            //伴随矩阵L-
            LMINUS = new int[np, nt];

            for (int i = 0; i < nt; ++i)
            {
                for (int j = 0; j < np; ++j)
                {
                    if (temp[i, j] == 1)
                    {
                        LPLUS[j, i] = 1;
                    }
                    else
                    {
                        LPLUS[j, i] = 0;
                    }

                    if (temp[i, j] == -1)
                    {
                        LMINUS[j, i] = 1;
                    }
                    else
                    {
                        LMINUS[j, i] = 0;
                    }
                }
            }

            SRR.Close();
            return;
        }

        // HList按FTotalCost排好序了，将Node插入相同FTotalCost的第一个位置
        private int SortAdd(Heap HList, AStarNode Node)//将节点插入到相同FTotalCost值的第一个位置
        {
            int position = 0;
            for (int i = 0; i < HList.Count; ++i)
            {
                AStarNode LNode = (AStarNode)HList[i];
                for(int j = 0; j < AStar.max_attribute_num; j++)
                {
                    if (LNode.TotalCost.cost[j] > Node.TotalCost.cost[j])
                        break;
                    else if (LNode.TotalCost.cost[j] == Node.TotalCost.cost[j])
                    {
                        if (LNode.TotalCost.cost[j] >= Node.TotalCost.cost[j])
                            break;
                        else
                            ++position;
                    }
                    else
                        ++position;
                }
                
            }
            if (position == HList.Count)
                HList.Add(Node);//加到末尾处
            else
                HList.Insert(position, Node);
            return position;
        }

        private void PrintNodeList(object ANodeList)//输出某列表中所有节点的信息
        {
            Console.WriteLine("\nNode list:");
            int i = 0;
            foreach (AStarNode n in (ANodeList as IEnumerable))
            {
                n.PrintNodeInfo(i++);
            }
            Console.WriteLine("=======================================================================================================");
        }

        #endregion

        #region Public Methods

        //向屏幕输出，并写文件result.txt,有重载
        public void PrintSolution()
        {
            Console.WriteLine("The number of undominated results:{0}", FSolution_Goals.Count);
            Console.WriteLine("The number of expanded markings:{0}", FExpandedList.Count);

            Console.WriteLine("File writing...");
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("./result.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Cannot open result.txt for writing.");
                return;
            }
            Console.SetOut(writer);

            Console.WriteLine("************* FExpandedList *************");
            Console.WriteLine("The number of expanded markings:{0}", NExpandedNode);
            PrintNodeList(FExpandedList);//向文件输出FExpandedList

            Console.WriteLine();
            Console.WriteLine("************* Node list *************");
            Console.WriteLine("The number of undominated results:{0}", FSolution.Count);
            for (int i = FSolution.Count - 1; i >= 0; --i)
                PrintNodeList(FSolution[i]);//向文件输出FSolution
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
            Console.WriteLine("File(A* result.txt) writing complete.");

        }

        public void FindPath(AStarNode AStartNode, AStarNode AGoalNode, int hmethod, bool printScreen)
        {
            //hmethod:所用启发函数h
            //h0=0;
            //h1=next hop;
            //h2=max{ei(m)};

            //printScreen:是否向屏幕打印每个扩展节点的信息

            FStartNode = AStartNode;
            FGoalNode = AGoalNode;

            FOpenList.Clear();
            FClosedList.Clear();
            FSuccessors.Clear();
            FSolution.Clear();
            FExpandedList.Clear();
            NExpandedNode = 0;
            FSolution_Goals.Clear();

            int loop = 0;
            FOpenList.Add(FStartNode);
            while (FOpenList.Count >= 0)
            {
                if (FOpenList.Count == 0)
                {
                    //回溯构造出路径
                    for (int i = 0; i < FSolution_Goals.Count; ++i)
                    {
                        FSolution.Insert(0, new ArrayList());
                        AStarNode tempNode = FSolution_Goals[i] as AStarNode;
                        while (tempNode != null)
                        {
                            (FSolution[0] as ArrayList).Insert(0, tempNode);
                            tempNode = tempNode.Parent;
                        }
                        PrintNodeList(FSolution[0]);//向屏幕输出FSolution
                    }
                    break; //程序正常退出
                }

                /*
                Console.WriteLine("open列表：");
                for (int i = 0; i < FOpenList.Count; ++i)
                {
                    (FOpenList[i] as AStarNode).PrintNodeInfo(-1);
                }
                Console.WriteLine("closed列表");
                for (int i = 0; i < FClosedList.Count; ++i)
                {
                    (FClosedList[i] as AStarNode).PrintNodeInfo(-2);
                }*/

                //OPEN列表中的第一个节点
                AStarNode NodeCurrent = (AStarNode)FOpenList[0];
                FOpenList.Remove(FOpenList[0]);	//除去要扩展的节点


                if (NodeCurrent.IsGoal())
                {
                    Console.WriteLine("Find a possible result:()");

                    bool sign = true;
                    for (int i = 0; i < FSolution_Goals.Count; ++i)
                    {
                        if (MyCost.Dominate(NodeCurrent.Cost, (FSolution_Goals[i] as AStarNode).Cost) == 1)
                        {
                            FSolution_Goals.RemoveAt(i--);
                        }
                        else if (MyCost.Dominate(NodeCurrent.Cost, (FSolution_Goals[i] as AStarNode).Cost) == -1 || MyCost.Dominate(NodeCurrent.Cost, (FSolution_Goals[i] as AStarNode).Cost) == 2)
                        {
                            sign = false;
                            break;
                        }
                    }
                    if (sign)
                        FSolution_Goals.Add(NodeCurrent); //移到SOLUTION_GOALS中
                }
                else
                {
                    //每一个节点扩展前，都要和已经找到路径的节点做比较，若代价已经被其支配，就没有继续找下去的必要了。
                    bool check = true;
                    for (int i = 0; i < FSolution_Goals.Count; ++i)
                    {
                        if (MyCost.Dominate((FSolution_Goals[i] as AStarNode).Cost, NodeCurrent.TotalCost) == 1 || MyCost.Dominate((FSolution_Goals[i] as AStarNode).Cost, NodeCurrent.TotalCost) == 2)
                        {
                            check = false;
                            break;
                        }
                    }

                    if (check)
                    {
                        //把当前节点的所有子节点加入到FSuccessors
                        FSuccessors.Clear();
                        NodeCurrent.GetSuccessors(FSuccessors, hmethod);

                        if (printScreen)
                            NodeCurrent.PrintNodeInfo(loop++);//打印当前节点的信息

                        foreach (AStarNode NodeSuccessor in FSuccessors)
                        {
                            //NodeSuccessor.PrintNodeInfo(-3);

                            // ----------------------------若生成的节点已在OPEN列表中----------------------------
                            AStarNode NodeOpen = null;
                            int openNum = 0;
                            bool AddOpen = true;
                            for (; openNum < FOpenList.Count; openNum++)
                            {
                                if (NodeSuccessor.IsSameState((AStarNode)FOpenList[openNum]))
                                {
                                    NodeOpen = (AStarNode)FOpenList[openNum]; //扩展出的子节点已经在OPEN列表中
                                    if (MyCost.Dominate(NodeSuccessor.Cost, NodeOpen.Cost) == 1) //新生成节点代价支配已存在节点
                                    {
                                        if (NodeSuccessor.DominateMr(NodeOpen)) //新生成节点Mr支配已存在节点
                                            FOpenList.RemoveAt(openNum--);
                                    }
                                    //else if (MyCost.Dominate(NodeSuccessor.Cost, NodeOpen.Cost) == 0) //两个节点代价不互相支配
                                    //{
                                    //    SortAdd(FOpenList, NodeSuccessor);
                                    //}
                                    else if (MyCost.Dominate(NodeSuccessor.Cost, NodeOpen.Cost) == -1) //新生成节点代价被已存在节点支配
                                    {
                                        if (NodeOpen.DominateMr(NodeSuccessor))
                                            AddOpen = false;
                                    }
                                    else if (MyCost.Dominate(NodeSuccessor.Cost, NodeOpen.Cost) == 2) //两个节点代价相同
                                    {
                                        if (NodeOpen.DominateMr(NodeSuccessor))
                                            AddOpen = false;
                                        else if (NodeSuccessor.DominateMr(NodeOpen))
                                            FOpenList.RemoveAt(openNum--);
                                    }
                                }
                            }

                            // ----------------------------若生成的节点已在CLOSE列表中----------------------------
                            AStarNode NodeClosed = null;
                            int closeNum = 0;
                            bool AddClosed = true;
                            for (; AddClosed && closeNum < FClosedList.Count; closeNum++)
                            {
                                if (NodeSuccessor.IsSameState((AStarNode)FClosedList[closeNum]))
                                {
                                    NodeClosed = (AStarNode)FClosedList[closeNum]; //扩展出的子节点已经在CLOSE列表中
                                    if (MyCost.Dominate(NodeSuccessor.Cost, NodeClosed.Cost) == 1) //新生成节点代价支配已存在节点
                                    {
                                        if (NodeSuccessor.DominateMr(NodeClosed)) //新生成节点Mr支配已存在节点
                                        {
                                            for (int i = 0; i < FOpenList.Count; i++)
                                            {
                                                if (((AStarNode)FOpenList[i]).Parent.IsSameNode(NodeClosed)) //父节点为已存在节点的节点
                                                {
                                                    FOpenList.RemoveAt(i--);
                                                }
                                            }
                                            FClosedList.RemoveAt(closeNum--);
                                        }
                                    }
                                    //else if (MyCost.Dominate(NodeSuccessor.Cost, NodeClosed.Cost) == 0) //两个节点代价不互相支配
                                    //{
                                    //    SortAdd(FOpenList, NodeSuccessor);
                                    //}
                                    else if (MyCost.Dominate(NodeSuccessor.Cost, NodeClosed.Cost) == -1) //新生成节点代价被已存在节点支配
                                    {
                                        if (NodeClosed.DominateMr(NodeSuccessor))
                                            AddClosed = false;
                                    }
                                    else if (MyCost.Dominate(NodeSuccessor.Cost, NodeClosed.Cost) == 2) //两个节点代价相同
                                    {
                                        if (NodeClosed.DominateMr(NodeSuccessor))
                                            AddClosed = false;
                                        else if (NodeSuccessor.DominateMr(NodeClosed))
                                        {
                                            for (int i = 0; i < FOpenList.Count; i++)
                                            {
                                                if (((AStarNode)FOpenList[i]).Parent.IsSameNode(NodeClosed)) //父节点为已存在节点的节点
                                                {
                                                    FOpenList.RemoveAt(i--);
                                                }
                                            }
                                            FClosedList.RemoveAt(closeNum--);
                                        }
                                    }
                                }
                            }

                            // ---------------------若生成的节点既不在OPEN列表中，也不在CLOSE列表中--------------------
                            if (AddOpen && AddClosed)
                                SortAdd(FOpenList, NodeSuccessor);


                        } //foreach (AStarNode NodeSuccessor in FSuccessors)结束

                        FExpandedList.Add(NodeCurrent);
                        if (FSuccessors.Count > 0) //若当前节点没有子节点，则当前节点为死锁
                        {
                            //NExpandedNode没加入死锁节点，所以比FExpandedList.Count可能要小 (运行发现和FExpandedList.Count一样)
                            ++NExpandedNode;//已扩展节点数
                        }
                    } //if(check)
                }

                SortAdd(FClosedList, NodeCurrent);

            }//while (FOpenList.Count > 0) 结束

        }//FindPath

        #endregion
    }

}