using Command;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NetClient
{
    class A
    {
        public void FunctionA(System.Action action)
        {
            //*********
            Console.WriteLine("我是函数A");
            action.Invoke();
            //*********
        }
    }
    class B
    {
        public void FunctionB()
        {
            Console.WriteLine("我是函数B");
        }
    }

    class Program
    {
        //显示连接失败
        private static bool showConnFail = false;
        //ip和地址
        private static string ip = "124.221.192.174";
        private static int port = 1777;

        static int uid = 0;
        static int tid = 0;


        //上一次发送同步信息的时间
        private static DateTime lastSendSyncTime = DateTime.Now;
        //同步帧率
        public static float syncInterval = 0.05f;


        static void Main(string[] args)
        {

            //A a = new A();
            //B b = new B();
            //a.FunctionA(delegate () { b.FunctionB(); });
            //return;

            Logger.Instance.Debug("Client Start");

            Logger.Instance.Debug("Debug Test");
            Logger.Instance.Info("InfoTest");
            Logger.Instance.Warning("Warning Test");
            Logger.Instance.Error("Error Test");

            Start();

            new Thread(() =>
            {
                Thread.Sleep(500);
                SendMessage();
            }).Start();
            new Thread(() =>
            {
                Thread.Sleep(500);
                Update();
            }).Start();
        }




        static void Start()
        {
            //关闭心跳
            NetManager.isUsePing = true;

            //协议监听
            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_LOGIN_RESP, OnMsgLogin);  //登录
            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_REGISTER_RESP, OnMsgRegister);  //注册
            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_CREATE_ROLE_RESP, OnMsgCreateRole);  //创建角色

            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_GET_TABLES_INFO_RESP, OnMsgGetTablesInfo);  //获取所有房间信息
            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_ENTER_TABLE_RESP, OnMsgEnterTable);  //进入房间
            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_MOVE_RESP, OnMsgMove);  //移动
            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_KICK_RESP, OnMsgKick);  //踢出

            NetManager.AddMsgListener((short)SERVER.CMD_SERVER_GET_STORE_INFO_RESP, OnMsgGetStoreInfo);  //商店信息
           
            //网络事件监听
            NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
            NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
            //连接服务器
            NetManager.Connect(ip, port);
        }

        static void SendMessage()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("等待发送信息，回车->发送，close->断开连接");
                    string s = Console.ReadLine();

                    string[] parameter = Regex.Split(s, "\\s+", RegexOptions.IgnoreCase);
                    //for (int i = 0; i < parameter.Length; i++)
                    //{
                    //    Console.Write(parameter[i] + '\n');
                    //}

                    switch (parameter[0])
                    {
                        case "close":
                            NetManager.Close();
                            break;
                        case "help":
                            Console.WriteLine("NO HELP INFO");
                            break;

                        case "login":
                            if (parameter.Length < 3)
                                break;
                            if (parameter[1] == "-a")
                            {
                                //单个连接测试登录
                                if (parameter[parameter.Length - 1] == "test")
                                    UserLogin();
                            }
                            if (parameter[1] == "-ex")
                            {
                                //多个连接测试登录
                                int nums = int.Parse(parameter[2]);

                                Console.WriteLine("Login nums:{0}", nums);

                            }

                            break;
                        case "register":
                            UserRegister();
                            break;
                        case "create"://创建角色
                            if (parameter.Length < 2)
                                break;
                            if (parameter[1] == "role")
                            {
                                CreateRole();
                            }
                            break;
                        case "get"://获取所有房间信息
                            if (parameter.Length < 3)
                                break;
                            if (parameter[1] == "tables")
                            {
                                if (parameter[2] == "info")
                                {
                                    GetTablesInfo();
                                }
                            }


                            if (parameter[1] == "store")
                            {
                                if (parameter[2] == "info")
                                {
                                    GetStoreInfo();
                                }
                            }

                            break;
                        case "enter"://进入房间
                            if (parameter.Length < 2)
                                break;
                            if (parameter[1] == "table")
                            {

                                EnterTable();
                                
                            }
                            break;
                        case "move":
                            SyncUpdate();
                            break;
                        case "brocast":
                            ICPacket inpacket = new ICPacket();
                            inpacket.Begin(0x3001);

                            inpacket.End();
                            break;

                        default:
                            break;

                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static void Update()
        {
            while (true)
            {
                NetManager.Update();
                //SyncUpdate();
            }


        }


        //连接成功回调
        static void OnConnectSucc(string err)
        {
            Console.WriteLine("OnConnectSucc");
            //Debug.Log("OnConnectSucc");
        }

        //连接失败回调
        static void OnConnectFail(string err)
        {
            showConnFail = true;
            Console.WriteLine("OnConnectFail");
            //PanelManager.Open<TipPanel>(err);
        }

        static void OnMsgLogin(ICPacket msg)
        {
            Console.WriteLine("Recv MsgLogin");
            Console.WriteLine("recv length:{0}, data:{1}", msg.GetData().Length, BitConverter.ToString(msg.GetData()));

            //登录返回包的命令字为0x4001
            //返回字段:    登录结果错误码 ret = msg.ReadByte()  0代表登录成功 其他代表登录错误     用户uid =

            int ret = msg.ReadByte();
            if (ret != 0)
            {
                Console.WriteLine("login err ,ret:{0}", ret);
                return;
            }

            Console.WriteLine("Login Succ ,ret:{0}", ret);

            uid = msg.ReadInt32();
            Console.WriteLine("Recv UID:{0}", uid);

        }

        static void OnMsgRegister(ICPacket msg)
        {
            Console.WriteLine("Recv Msg Register");

            int ret = msg.ReadByte();
            if (ret != 0)
            {
                Console.WriteLine("Register err ,ret:{0}", ret);
                return;
            }

            Console.WriteLine("Register Succ ,ret:{0}", ret);
        }

        static void OnMsgCreateRole(ICPacket msg)
        {
            Console.WriteLine("Recv Msg Create Role");

            int ret = msg.ReadByte();
            if (ret != 0)
            {
                Console.WriteLine("create role err ,ret:{0}", ret);
                return;
            }

            Console.WriteLine("Create Role Succ,ret:{0}", ret);
        }
        static void OnMsgGetTablesInfo(ICPacket msg)
        {
            byte tableNum = msg.ReadByte();
            for (int i = 0; i < tableNum; i++)
            {
                Logger.Instance.Info(string.Format("tid:{0},name:{1},type:{2},online:{3}", msg.ReadInt32(), msg.ReadString(),msg.ReadInt32(), msg.ReadInt32()));
            }

        }

        static void OnMsgEnterTable(ICPacket msg)
        {
            byte ret=msg.ReadByte();
            Int32 userid = msg.ReadInt32();
            Int32 tableid = msg.ReadInt32();
            msg.ReadInt32();
            msg.ReadString();
            if (ret != 0)
            {
                Logger.Instance.Warning(string.Format("uid:{0} enter table:{1} fail,ret:{2}", userid, tableid, ret));
                return;
            }

            Logger.Instance.Info(string.Format("uid:{0} enter table:{1} succ", userid, tableid));
            //uid tid roleid name
            //msg.ReadInt32();
            //msg.ReadInt32();
            //msg.ReadInt32();
            //msg.ReadInt32();

            

        }
        static void OnMsgMove(ICPacket msg)
        {
            Logger.Instance.Info(string.Format("uid:{0},tid:{1} move to x:{2},y:{3},z:{4}",msg.ReadInt32(),msg.ReadInt32(),msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()));
        }

        static void OnMsgKick(ICPacket msg)
        {
            Logger.Instance.Info("Your account login in other place");
        }

        static void OnMsgGetStoreInfo(ICPacket msg)
        {
            Int32 goodsNum = msg.ReadInt32();

            for (int i =0;i<goodsNum;i++)
            {
                Int32 id=msg.ReadInt32();
                string name = msg.ReadString();
               Int64 price =msg.ReadInt64();
                Int64 sellprice = msg.ReadInt64();
                Int64 num = msg.ReadInt64();

                Logger.Instance.Info(string.Format("id:{0},name:{1},price:{2},sellprice:{3},num:{4}",id, name,price, sellprice, num));
            }
        
        }


        static void UserLogin()
        {
            //Console.WriteLine("Please Input username:");
            //string username = Console.ReadLine();
            //Console.WriteLine("Please Input password:");
            //string password = Console.ReadLine();

            Console.WriteLine("Please Input userId:");
           

            ICPacket outPack = new ICPacket();
            outPack.Begin((short)CLIENT.CMD_CLIENT_LOGIN_REQ);
            //outPack.WriteString(username);
            //outPack.WriteString(password);
            outPack.WriteInt32(1001);
            outPack.End();

            NetManager.Send(outPack);
            //Send(inpacket);
        }

        static void UserRegister()
        {
            Console.WriteLine("Please Input username:");
            string username = Console.ReadLine();
            Console.WriteLine("Please Input password:");
            string password = Console.ReadLine();

            ICPacket outPack = new ICPacket();
            outPack.Begin((short)CLIENT.CMD_CLIENT_REGISTER_REQ);
            outPack.WriteString(username);
            outPack.WriteString(password);
            outPack.End();

            NetManager.Send(outPack);
            //Send(inpacket);
        }

        static void CreateRole()
        {
            //创建角色并取名
            Console.WriteLine("Please Select Create Role Type:1.Archer 2.Warrior 3.wizard");   //射手 战士 法师
            string role_id = Console.ReadLine();

            Console.WriteLine("Please Input role name:");
            string name = Console.ReadLine();



            //发送字段 uid  roleId  name
            ICPacket outPack = new ICPacket();
            outPack.Begin((Int16)CLIENT.CMD_CLIENT_CREATE_ROLE_REQ);

            outPack.WriteInt32(uid);
            outPack.WriteInt32(int.Parse(role_id));
            outPack.WriteString(name);

            outPack.End();

            NetManager.Send(outPack);

        }

        static void GetTablesInfo()
        {
            //发送字段 uid  roleId  name
            ICPacket outPack = new ICPacket();
            outPack.Begin((Int16)CLIENT.CMD_CLIENT_GET_TABLES_INFO_REQ);
            outPack.End();

            NetManager.Send(outPack);
        }

        static void GetStoreInfo()
        {
            //发送字段 uid  roleId  name
            ICPacket outPack = new ICPacket();
            outPack.Begin((Int16)CLIENT.CMD_CLIENT_GET_STORE_INFO_REQ);
            outPack.WriteByte(1);
            outPack.End();

            NetManager.Send(outPack);
        }

        static void EnterTable()
        {
            Console.WriteLine("Please Input table id:");
            string s = Console.ReadLine();
            tid = Convert.ToInt32(s);
            ICPacket outPack = new ICPacket();
            outPack.Begin((Int16)CLIENT.CMD_CLIENT_ENTER_TABLE_REQ);
            outPack.WriteInt32(uid);
            outPack.WriteInt32(tid);
            outPack.WriteInt32(12);//role_id
            outPack.WriteString("name");
            outPack.End();

            NetManager.Send(outPack);
        }

            //发送同步协议
            static void SyncUpdate()
        {
            //Console.WriteLine("SyncUpdate");

            //TimeSpan ads = DateTime.Now.Subtract(lastSendSyncTime).Duration();
            //if (ads.TotalMilliseconds < syncInterval)
            //{
            //    return;
            //}
            //lastSendSyncTime = DateTime.Now;

            //移动协议

            //发送字段 x,y,z平移 旋转 缩放等   以及玩家id用来同步其他客户端

            float x=1.25f, y = 2.34f, z = 4.88f, rx = 1.0f, ry = 1.0f, rz = 1.0f, sx = 1.0f, sy = 1.0f, sz = 1.0f;

            ICPacket outPack = new ICPacket();
            outPack.Begin((Int16)CLIENT.CMD_CLIENT_MOVE_REQ);

            
            outPack.WriteInt32(uid);
            outPack.WriteInt32(tid);
            outPack.WriteFloat(x);
            outPack.WriteFloat(y);
            outPack.WriteFloat(z);

            outPack.End();
            Logger.Instance.Info(string.Format("uid:{0},tid:{1} move",uid,tid));

            NetManager.Send(outPack);

        }



    }

    
}
