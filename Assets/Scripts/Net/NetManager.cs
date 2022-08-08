using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Command;
//namespace Game.script.net
//{
//    class NetClient
//    {
//    }
//}


public static class NetManager
{
	//定义套接字
	static Socket socket;
	//接收缓冲区
	static ICPacket readBuff;

	static byte[] buffer = new byte[(int)ICPacket.Packet.MAX_USER_PACKET_LEN];
	//写入队列
	static Queue<ICPacket> writeQueue;
	//是否正在连接
	static bool isConnecting = false;
	//是否正在关闭
	static bool isClosing = false;
	//消息列表
	static List<ICPacket> msgList = new List<ICPacket>();
	//消息列表长度
	static int msgCount = 0;
	//每一次Update处理的消息量
	readonly static int MAX_MESSAGE_FIRE = 30;
	//是否启用心跳
	public static bool isUsePing = true;
	//心跳间隔时间
	public static int pingInterval = 10;
	//上一次发送PING的时间
	static DateTime lastPingTime;
	//上一次收到PONG的时间
	static DateTime lastPongTime;

	//事件
	public enum NetEvent
	{
		ConnectSucc = 1,
		ConnectFail = 2,
		Close = 3,
	}
	//事件委托类型
	public delegate void EventListener(String err);
	//事件监听列表
	private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
	//添加事件监听
	public static void AddEventListener(NetEvent netEvent, EventListener listener)
	{
		//添加事件
		if (eventListeners.ContainsKey(netEvent))
		{
			eventListeners[netEvent] += listener;
		}
		//新增事件
		else
		{
			eventListeners[netEvent] = listener;
		}
	}
	//删除事件监听
	public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
	{
		if (eventListeners.ContainsKey(netEvent))
		{
			eventListeners[netEvent] -= listener;
		}
	}
	//分发事件
	private static void FireEvent(NetEvent netEvent, String err)
	{
		if (eventListeners.ContainsKey(netEvent))
		{
			eventListeners[netEvent](err);
		}
	}


	//消息委托类型
	public delegate void MsgListener(ICPacket msgBase);
	//消息监听列表
	private static Dictionary<Int16, MsgListener> msgListeners = new Dictionary<Int16, MsgListener>();
	//添加消息监听
	public static void AddMsgListener(Int16 msgName, MsgListener listener)
	{
		//添加
		if (msgListeners.ContainsKey(msgName))
		{
			msgListeners[msgName] += listener;
		}
		//新增
		else
		{
			msgListeners[msgName] = listener;
		}
	}
	//删除消息监听
	public static void RemoveMsgListener(Int16 msgName, MsgListener listener)
	{
		if (msgListeners.ContainsKey(msgName))
		{
			msgListeners[msgName] -= listener;
		}
	}
	//分发消息
	private static void FireMsg(Int16 msgName, ICPacket msgBase)
	{
		if (msgListeners.ContainsKey(msgName))
		{
			msgListeners[msgName](msgBase);
		}
	}


	//连接
	public static void Connect(string ip, int port)
	{
		//状态判断
		if (socket != null && socket.Connected)
		{
			Console.WriteLine("Connect fail, already connected!");
			//Debug.Log("Connect fail, already connected!");
			return;
		}
		if (isConnecting)
		{
			Console.WriteLine("Connect fail, isConnecting");
			//Debug.Log("Connect fail, isConnecting");
			return;
		}
		//初始化成员
		InitState();
		//参数设置
		socket.NoDelay = true;
		//Connect
		isConnecting = true;
		socket.BeginConnect(ip, port, ConnectCallback, socket);
	}

	//初始化状态
	private static void InitState()
	{
		//Socket
		socket = new Socket(AddressFamily.InterNetwork,
			SocketType.Stream, ProtocolType.Tcp);
		//接收缓冲区
		readBuff = new ICPacket();
		//写入队列
		writeQueue = new Queue<ICPacket>();
		//是否正在连接
		isConnecting = false;
		//是否正在关闭
		isClosing = false;
		//消息列表
		msgList = new List<ICPacket>();
		//消息列表长度
		msgCount = 0;
		//上一次发送PING的时间
		lastPingTime = DateTime.Now;
		//上一次收到PONG的时间
		lastPongTime = DateTime.Now;
        //监听PONG协议
        if (!msgListeners.ContainsKey((Int16)SERVER.CMD_SERVER_HEARTBEAT_RESP))
        {
            AddMsgListener((Int16)SERVER.CMD_SERVER_HEARTBEAT_RESP, OnMsgPong);
        }
    }

	//Connect回调
	private static void ConnectCallback(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket)ar.AsyncState;
			socket.EndConnect(ar);
			Console.WriteLine("Socket Connect Succ ");
			//Debug.Log("Socket Connect Succ ");
			FireEvent(NetEvent.ConnectSucc, "");
			isConnecting = false;


			//开始异步接收
			
			socket.BeginReceive(buffer, 0, buffer.Length, 0, ReceiveCallback, socket);

		}
		catch (SocketException ex)
		{
			Console.WriteLine("Socket Connect fail{0}", ex.ToString());
			//Debug.Log("Socket Connect fail " + ex.ToString());
			FireEvent(NetEvent.ConnectFail, ex.ToString());
			isConnecting = false;
		}
	}


	//关闭连接
	public static void Close()
	{
		//状态判断
		if (socket == null || !socket.Connected)
		{
			return;
		}
		if (isConnecting)
		{
			return;
		}
		//还有数据在发送
		if (writeQueue.Count > 0)
		{
			isClosing = true;
		}
		//没有数据在发送
		else
		{
			socket.Close();
			FireEvent(NetEvent.Close, "");
		}
	}

	//发送数据
	public static void Send(ICPacket msg)
	{
		//状态判断
		if (socket == null || !socket.Connected)
		{
			return;
		}
		if (isConnecting)
		{
			return;
		}
		if (isClosing)
		{
			return;
		}
		
		//写入队列
		ICPacket ba = new ICPacket();
		ba.Copy(msg.GetData());

		int count = 0;  //writeQueue的长度
		lock (writeQueue)
		{
			//入队操作
			writeQueue.Enqueue(ba);
			count = writeQueue.Count;
		}
		//send
		if (count == 1)
		{
			socket.BeginSend(ba.GetData(), 0, ba.GetData().Length,
				0, SendCallback, socket);
		}
	}

	//Send回调
	public static void SendCallback(IAsyncResult ar)
	{

		//获取state、EndSend的处理
		Socket socket = (Socket)ar.AsyncState;
		//状态判断
		if (socket == null || !socket.Connected)
		{
			return;
		}
		//EndSend 返回发送的数据
		int count = socket.EndSend(ar);
		//获取写入队列第一条数据            
		ICPacket ba;
		lock (writeQueue)
		{
			//获取头部
			ba = writeQueue.Peek();
		}
		//如果发送的数据长度与队列头部的数据长度不一致 再次发送  否则说明消息发送成功 头部消息出队
		if (ba.GetData().Length != count)
		{
			//数据发送不完整	继续发送  
			socket.BeginSend(ba.data, count, ba.data.Length-count,
				0, SendCallback, socket);
		}
		

		if(ba.GetData().Length == count)
		{
			lock (writeQueue)
			{
				//出队操作
				ba = writeQueue.Dequeue();

				Console.WriteLine("send cmd:0x{0:x4}", ba.GetCmd());
				Console.WriteLine("send data:{0}", BitConverter.ToString(ba.GetData()));
			}
		}
		//正在关闭
		else if (isClosing)
		{
			socket.Close();
		}
	}



	//Receive回调
	public static void ReceiveCallback(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket)ar.AsyncState;
			//获取接收数据长度
			int length = socket.EndReceive(ar);
			if (length > 0)
			{
				byte[] newBytes = new byte[length];
				Array.Copy(buffer, 0, newBytes, 0, length);

				//Console.WriteLine("recv length:{0}, data:{1}", length, BitConverter.ToString(newBytes));

				ICPacket packet = new ICPacket();

				packet.Copy(newBytes);
			
				packet.Decrypt();

				//Console.WriteLine("recv cmd:0x{0:x2}", packet.GetCmd());

              

                //添加到消息队列
                lock (msgList)
                {
                    msgList.Add(packet);
                    msgCount++;
                }
            }

			//清空数据，重新开始异步接收
			buffer = new byte[buffer.Length];
			//Console.WriteLine("重置缓冲区大小：{0}", buffer.Length);

			socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

			//socket.BeginReceive(readBuff.data, readBuff.data.Length-bodeylen,
			//		bodeylen, 0, ReceiveCallback, socket);
		}
		catch (SocketException ex)
		{
			Console.WriteLine("Socket Receive fail{0}", ex.ToString());
			//Debug.Log("Socket Receive fail" + ex.ToString());
		}
	}

	//数据处理
	//public static void OnReceiveData()
	//{
	//	//消息长度
	//	if (readBuff.length <= 2)
	//	{
	//		return;
	//	}
	//	//获取消息体长度
	//	int readIdx = readBuff.readIdx;
	//	byte[] bytes = readBuff.bytes;
	//	Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
	//	if (readBuff.length < bodyLength)
	//		return;
	//	readBuff.readIdx += 2;
	//	//解析协议名
	//	int nameCount = 0;
	//	string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
	//	if (protoName == "")
	//	{
	//		Console.WriteLine("OnReceiveData MsgBase.DecodeName fail");
	//		//Debug.Log("OnReceiveData MsgBase.DecodeName fail");
	//		return;
	//	}
	//	readBuff.readIdx += nameCount;
	//	//解析协议体
	//	int bodyCount = bodyLength - nameCount;
	//	MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
	//	readBuff.readIdx += bodyCount;
	//	readBuff.CheckAndMoveBytes();
	//	//添加到消息队列
	//	lock (msgList)
	//	{
	//		msgList.Add(msgBase);
	//		msgCount++;
	//	}
	//	//继续读取消息
	//	if (readBuff.length > 2)
	//	{
	//		OnReceiveData();
	//	}
	//}

	//Update
	public static void Update()
	{
		MsgUpdate();
		PingUpdate();
	}

	//更新消息
	public static void MsgUpdate()
	{
		//Console.WriteLine("MsgUpdate...");
		//初步判断，提升效率
		if (msgCount == 0)
		{
			return;
		}
		//重复处理消息
		for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
		{
			//获取第一条消息
			ICPacket msgBase = null;
			lock (msgList)
			{
				if (msgList.Count > 0)
				{
					msgBase = msgList[0];
					msgList.RemoveAt(0);
					msgCount--;
				}
			}
			//分发消息
			if (msgBase != null)
			{
				FireMsg((short)msgBase.GetCmd(), msgBase);
			}
			//没有消息了
			else
			{
				break;
			}
		}
	}

	//发送PING协议
	private static void PingUpdate()
	{
		//是否启用
		if (!isUsePing)
		{
			return;
		}
		//发送PING
		//计算时间间隔的绝对值
		TimeSpan ads = DateTime.Now .Subtract(lastPingTime).Duration();
		if (ads.TotalSeconds> pingInterval)
        {
            //MsgPing msgPing = new MsgPing();
			ICPacket msgPing = new ICPacket();
			msgPing.Begin((Int16)CLIENT.CMD_CLIENT_HEARTBEAT_REQ);
			msgPing.End();

			Send(msgPing);
            lastPingTime = DateTime.Now;
        }
		//检测PONG时间
		ads = DateTime.Now.Subtract(lastPongTime).Duration();
		if (ads.TotalSeconds > pingInterval * 4)
        {
            Close();
        }
    }

	//监听PONG协议
	private static void OnMsgPong(ICPacket msgBase)
	{
		Console.WriteLine("Recv MsgPong");

		lastPongTime = DateTime.Now;
	}

}
