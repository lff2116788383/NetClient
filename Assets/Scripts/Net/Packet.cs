using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//为了兼容所有的机型，我们规定，写入缓冲区的数字，必须按照小端模式来存储
//即GetBytes都要判断本机是否小端存储 BitConverter.ToInt16
public class BasePacket {
		public byte[] data;//报数据缓冲区
		public Int32 index;//写数据索引游标
		public Int32 headLen;  //包头长度
		public Int32 bodyLenIndex;  //包体长度字段起始位置，默认2字节

        public BasePacket()
        { 
        }


		public Int32 GetTotalLen()
		{
			return headLen+GetBodyLen();
		}

		public byte[] GetData()
{
			return data;
	}

		public byte[] GetBody()
		{
            return data.Skip(headLen).ToArray();
		}

    public Int32 GetBodyLen()
    {
        //if (!BitConverter.IsLittleEndian)//非小端格式 反转
        //{
        //    Array.Reverse(data,bodyLenIndex,data.Length-bodyLenIndex);
        //}
        //Console.WriteLine("GetBodyLen two bytes:0x{0:x2},0x{1:x2}",data[bodyLenIndex],data[bodyLenIndex+1]);
        return (Int32)BitConverter.ToInt16(data, bodyLenIndex);
    }

    public Int32 GetHeadLen()
		{
			return headLen;
		}

    public void Copy(byte[] value)
    {
        Array.Resize(ref data, data.Length + value.Length);
        Array.Copy(value, 0, data, data.Length - value.Length, value.Length);
        index += GetHeadLen();
    }

    public void Refer(byte[] bytes)
		{
			data = bytes;
			index = GetHeadLen();
		}

		/**********************基本数据读取**********************/
		public byte ReadByte() 
		{
			byte value =0;
			if (index < data.Length)
			{
				value = data[index];
				index++;
			}
			return value;
		}


		//读取所有(除了包头)不改变index
		public byte[] ReadAll()
		{
        byte[] value = new byte[data.Length - index];
        if (index < data.Length)
        {

            value = data.Skip(index).ToArray();
            //Array.Copy(data, index, value, 0, data.Length-index);

        }
        return value;
		}

        public string ReadString()
        {
            string value = "";
            if (index + 4 <= data.Length)
            {
            Int32 strLen = BitConverter.ToInt32(data,index);
                index += 4;
                if (index + strLen <= data.Length)
                {
                    //包中的字符串是'\0'结尾，在C/C++中不会有任何问题，但是golang的string内部
                    //都是字节序，'\0'并无特殊对待，作为正常的字符处理，这里需要剔除末尾的'\0'
                    if (0 == data[index + strLen - 1])
                    {

                        byte[] newBytes = new byte[strLen - 1];
                        Array.Copy(data, index, newBytes, 0, newBytes.Length);
                        value = Encoding.UTF8.GetString(newBytes);

                    }
                    else
                    {
                        byte[] newBytes = new byte[strLen];
                        Array.Copy(data, index, newBytes, 0, newBytes.Length);
                        value = Encoding.UTF8.GetString(newBytes);

                        //value = string(p.data[p.index : p.index + strLen])
                    }
                    index += strLen;
                }
            }
            return value;
        }

    //转换字节数组为小端格式
    public void LittleEndian(byte[] source, Int32 sourceIndex)
    {
        if (!BitConverter.IsLittleEndian)//非小端格式 反转
        {
            Array.Reverse(source, sourceIndex, source.Length - sourceIndex);
        }
    }
    //小端读取
    public Int16 ReadInt16()
    {
        Int16 value = -1;
        if (index + 2 <= (data.Length))
        {

            value = BitConverter.ToInt16(data, index);

            index += 2;
        }

        byte[] buf = BitConverter.GetBytes(value);//默认小端格式
        
        //Console.WriteLine("本机小端存储");

        return value;
    }

    public Int32 ReadInt32()
    {
        Int32 value = -1;
        if (index + 4 <= (data.Length))
        {
            value = BitConverter.ToInt32(data, index);
            index += 4;
        }
        byte[] buf = BitConverter.GetBytes(value);

        return value;
    }

    public Int64 ReadInt64()
        {
        Int64 value = -1;
        if (index + 8 <= data.Length)
        {
            value = BitConverter.ToInt64(data, index);
            index += 8;
        }
        byte[] buf = BitConverter.GetBytes(value);

        return value;
    }

    public float ReadFloat()
    {
        return float.Parse(ReadString());
    }


    /**********************基本数据写入**********************/
    public void WriteByte(byte value)
    {

        Array.Resize(ref data, data.Length + 1);
        data[data.Length - 1] = value;

      
    }

    public void  WriteBytes(byte[] value)
		{
        //给data扩容 
        Array.Resize(ref data, data.Length + value.Length);
        value.CopyTo(data,data.Length - value.Length);
    }

    public void WriteString(string value)
    {
        byte[] s=Encoding.UTF8.GetBytes(value);

        Array.Resize(ref s,s.Length+1);
        s[s.Length - 1] = 0;

        WriteInt32((Int32)s.Length);

        WriteBytes(s);
}

    //小端写入
    public void WriteInt16(Int16 value)
    {
        //转换成byte[] 判断大小端类型 写入
        byte[] buf = new byte[2];

        buf = BitConverter.GetBytes(value); //默认的小端格式

        Array.Resize(ref data, data.Length + buf.Length);
        buf.CopyTo(data, data.Length - buf.Length);
    }

    public void WriteInt32Ex(int value)
        {
            
            this.WriteInt32((Int32)value);
       }

    public void WriteInt32(Int32 value)
    {
        byte[] buf = new byte[4];
        buf = BitConverter.GetBytes(value); //默认的小端格式

        Array.Resize(ref data, data.Length + buf.Length);
        buf.CopyTo(data, data.Length - buf.Length);
    }

    public void WriteInt64(Int64 value)
    {
        byte[] buf = new byte[8];
        buf = BitConverter.GetBytes(value); //默认的小端格式

        Array.Resize(ref data, data.Length + buf.Length);
        buf.CopyTo(data, data.Length - buf.Length);
    }

    public void WriteFloat(float value)
    {
        WriteString(value.ToString());
    }

    public string Debug()
    {
        return string.Format("headLen({0}) bodyLen({1}) totalLen({2} dataLen{3})",
            this.GetHeadLen(),
            this.GetBodyLen(),
            this.GetTotalLen(),
            this.data.Length
        );
    }

}
public class ICPacket : BasePacket
    {
        public bool isEncrypted;

        public enum Version
        {
            //主版本号
            SERVER_PACEKTVER_NORMAL = 2,//常规版本

            SERVER_PACEKTVER_MERGE = 3,//国内外命令字合并使用版本号。

            //子版本号
            SERVER_SUBPACKETVER_FIRST = 1,
            SERVER_SUBPACKETVER_NORMAL = 2,
            SERVER_SUBPACKETVER_ZIP = 3, // 压缩版协议ZIP
            SERVER_SUBPACKETVER_FZIP = 4,// 完全压缩协议 收发都经过zip压缩
        }

        public enum Packet
		{
			IC_PACKET_HEADER_LENGTH = 13,
			MONEY_PACKET_HEADER_LENGTH = 9,
			CMS_PACKET_HEADER_LENGTH = 10,
			BPT_MAX_PACKET_LENGTH = 2 * 1024,
			BPT_EXTERNAL_PACKET_HEADER_LENGTH = 12,
			BPT_INTERNAL_PACKET_HEADER_LENGTH = 22,
			LOG_PACKET_HEADER_LENGTH = 6,
			LOG_MAX_PACKET_LENGTH = 10 * 1024,
			SLT_PACKET_HEADER_LENGTH = 16,
			MAX_USER_PACKET_LEN = 20 * 1024, // 业务层包体最大20KB
		}

        public ICPacket()
        {

            headLen = (int)Packet.IC_PACKET_HEADER_LENGTH;
            bodyLenIndex = 6;
            data = new byte[(int)0];
        }





        //发送字节映射表
        byte[] sendByteMap = new byte[]{
    0x70, 0x2F, 0x40, 0x5F, 0x44, 0x8E, 0x6E, 0x45, 0x7E, 0xAB, 0x2C, 0x1F, 0xB4, 0xAC, 0x9D, 0x91,
    0x0D, 0x36, 0x9B, 0x0B, 0xD4, 0xC4, 0x39, 0x74, 0xBF, 0x23, 0x16, 0x14, 0x06, 0xEB, 0x04, 0x3E,
    0x12, 0x5C, 0x8B, 0xBC, 0x61, 0x63, 0xF6, 0xA5, 0xE1, 0x65, 0xD8, 0xF5, 0x5A, 0x07, 0xF0, 0x13,
    0xF2, 0x20, 0x6B, 0x4A, 0x24, 0x59, 0x89, 0x64, 0xD7, 0x42, 0x6A, 0x5E, 0x3D, 0x0A, 0x77, 0xE0,
    0x80, 0x27, 0xB8, 0xC5, 0x8C, 0x0E, 0xFA, 0x8A, 0xD5, 0x29, 0x56, 0x57, 0x6C, 0x53, 0x67, 0x41,
    0xE8, 0x00, 0x1A, 0xCE, 0x86, 0x83, 0xB0, 0x22, 0x28, 0x4D, 0x3F, 0x26, 0x46, 0x4F, 0x6F, 0x2B,
    0x72, 0x3A, 0xF1, 0x8D, 0x97, 0x95, 0x49, 0x84, 0xE5, 0xE3, 0x79, 0x8F, 0x51, 0x10, 0xA8, 0x82,
    0xC6, 0xDD, 0xFF, 0xFC, 0xE4, 0xCF, 0xB3, 0x09, 0x5D, 0xEA, 0x9C, 0x34, 0xF9, 0x17, 0x9F, 0xDA,
    0x87, 0xF8, 0x15, 0x05, 0x3C, 0xD3, 0xA4, 0x85, 0x2E, 0xFB, 0xEE, 0x47, 0x3B, 0xEF, 0x37, 0x7F,
    0x93, 0xAF, 0x69, 0x0C, 0x71, 0x31, 0xDE, 0x21, 0x75, 0xA0, 0xAA, 0xBA, 0x7C, 0x38, 0x02, 0xB7,
    0x81, 0x01, 0xFD, 0xE7, 0x1D, 0xCC, 0xCD, 0xBD, 0x1B, 0x7A, 0x2A, 0xAD, 0x66, 0xBE, 0x55, 0x33,
    0x03, 0xDB, 0x88, 0xB2, 0x1E, 0x4E, 0xB9, 0xE6, 0xC2, 0xF7, 0xCB, 0x7D, 0xC9, 0x62, 0xC3, 0xA6,
    0xDC, 0xA7, 0x50, 0xB5, 0x4B, 0x94, 0xC0, 0x92, 0x4C, 0x11, 0x5B, 0x78, 0xD9, 0xB1, 0xED, 0x19,
    0xE9, 0xA1, 0x1C, 0xB6, 0x32, 0x99, 0xA3, 0x76, 0x9E, 0x7B, 0x6D, 0x9A, 0x30, 0xD6, 0xA9, 0x25,
    0xC7, 0xAE, 0x96, 0x35, 0xD0, 0xBB, 0xD2, 0xC8, 0xA2, 0x08, 0xF3, 0xD1, 0x73, 0xF4, 0x48, 0x2D,
    0x90, 0xCA, 0xE2, 0x58, 0xC1, 0x18, 0x52, 0xFE, 0xDF, 0x68, 0x98, 0x54, 0xEC, 0x60, 0x43, 0x0F,
};

        //接收字节映射表
        byte[] recvByteMap = new byte[]{
    0x51, 0xA1, 0x9E, 0xB0, 0x1E, 0x83, 0x1C, 0x2D, 0xE9, 0x77, 0x3D, 0x13, 0x93, 0x10, 0x45, 0xFF,
    0x6D, 0xC9, 0x20, 0x2F, 0x1B, 0x82, 0x1A, 0x7D, 0xF5, 0xCF, 0x52, 0xA8, 0xD2, 0xA4, 0xB4, 0x0B,
    0x31, 0x97, 0x57, 0x19, 0x34, 0xDF, 0x5B, 0x41, 0x58, 0x49, 0xAA, 0x5F, 0x0A, 0xEF, 0x88, 0x01,
    0xDC, 0x95, 0xD4, 0xAF, 0x7B, 0xE3, 0x11, 0x8E, 0x9D, 0x16, 0x61, 0x8C, 0x84, 0x3C, 0x1F, 0x5A,
    0x02, 0x4F, 0x39, 0xFE, 0x04, 0x07, 0x5C, 0x8B, 0xEE, 0x66, 0x33, 0xC4, 0xC8, 0x59, 0xB5, 0x5D,
    0xC2, 0x6C, 0xF6, 0x4D, 0xFB, 0xAE, 0x4A, 0x4B, 0xF3, 0x35, 0x2C, 0xCA, 0x21, 0x78, 0x3B, 0x03,
    0xFD, 0x24, 0xBD, 0x25, 0x37, 0x29, 0xAC, 0x4E, 0xF9, 0x92, 0x3A, 0x32, 0x4C, 0xDA, 0x06, 0x5E,
    0x00, 0x94, 0x60, 0xEC, 0x17, 0x98, 0xD7, 0x3E, 0xCB, 0x6A, 0xA9, 0xD9, 0x9C, 0xBB, 0x08, 0x8F,
    0x40, 0xA0, 0x6F, 0x55, 0x67, 0x87, 0x54, 0x80, 0xB2, 0x36, 0x47, 0x22, 0x44, 0x63, 0x05, 0x6B,
    0xF0, 0x0F, 0xC7, 0x90, 0xC5, 0x65, 0xE2, 0x64, 0xFA, 0xD5, 0xDB, 0x12, 0x7A, 0x0E, 0xD8, 0x7E,
    0x99, 0xD1, 0xE8, 0xD6, 0x86, 0x27, 0xBF, 0xC1, 0x6E, 0xDE, 0x9A, 0x09, 0x0D, 0xAB, 0xE1, 0x91,
    0x56, 0xCD, 0xB3, 0x76, 0x0C, 0xC3, 0xD3, 0x9F, 0x42, 0xB6, 0x9B, 0xE5, 0x23, 0xA7, 0xAD, 0x18,
    0xC6, 0xF4, 0xB8, 0xBE, 0x15, 0x43, 0x70, 0xE0, 0xE7, 0xBC, 0xF1, 0xBA, 0xA5, 0xA6, 0x53, 0x75,
    0xE4, 0xEB, 0xE6, 0x85, 0x14, 0x48, 0xDD, 0x38, 0x2A, 0xCC, 0x7F, 0xB1, 0xC0, 0x71, 0x96, 0xF8,
    0x3F, 0x28, 0xF2, 0x69, 0x74, 0x68, 0xB7, 0xA3, 0x50, 0xD0, 0x79, 0x1D, 0xFC, 0xCE, 0x8A, 0x8D,
    0x2E, 0x62, 0x30, 0xEA, 0xED, 0x2B, 0x26, 0xB9, 0x81, 0x7C, 0x46, 0x89, 0x73, 0xA2, 0xF7, 0x72,
};

    public int encryptPacket(ICPacket outPacket)
    {
        if (outPacket.isEncrypted)
        {
            return 0;
        }

        Console.WriteLine("data:{0}", BitConverter.ToString(outPacket.GetData()));

        int total = outPacket.GetTotalLen();

        int headlen = outPacket.GetHeadLen();

        //byte[] Getdata = outPacket.GetData();
        Console.WriteLine("total:{0},headlen:{1}", total,headlen);

        byte code = 0;

        for (int i = headlen; i < total; i++)
        {
            code += outPacket.GetData()[i];
            //0c
            outPacket.data[i] = sendByteMap[outPacket.GetData()[i]];
        }
        
           Console.WriteLine("{0:x4}", outPacket.GetData()[total-1]);

        Console.WriteLine("data encryp begin:{0}", BitConverter.ToString(outPacket.GetData()));
        outPacket.SetCheckCode((byte)(~code + 1));
       Console.WriteLine("data encryp end:{0}", BitConverter.ToString(outPacket.GetData()));
        return 0;
        }

     

        public int decryptPacket(ICPacket inPacket)
        {
            int total = inPacket.GetTotalLen();
            int headlen = inPacket.GetHeadLen();
            //byte[] Getdata = inPacket.GetData();

            byte code = inPacket.GetCheckCode();
            for (int i = headlen; i < total; i++)
            {
                data[i] = recvByteMap[data[i]];
                code += data[i];
            }

            if (0 != code)
            {
                Console.WriteLine("dectypt failed code=0 =%d", code);

                return -1;
            }

            return 0;
        }

        /**********************IC包加解密**********************/
        public int Encrypt()
        {
            return encryptPacket(this);
        }

        public int Decrypt()
        {
            return decryptPacket(this);
        }

        public void  SetBegin(Int16 cmd, byte version, byte subVersion) {

            this.WriteBytes(Encoding.UTF8.GetBytes("IC"));
            this.WriteInt16(cmd);

            this.WriteByte(version);

            this.WriteByte(subVersion);

            this.WriteInt16(0);

            this.WriteByte(0);

            this.WriteInt32(0);
}

        // 默认写版本号
        public void Begin(Int16 cmd)
        {
       
            this.WriteBytes(Encoding.UTF8.GetBytes("IC"));
            this.WriteInt16(cmd);
            this.WriteByte((byte)Version.SERVER_PACEKTVER_NORMAL);

            this.WriteByte((byte)Version.SERVER_SUBPACKETVER_FIRST);

            this.WriteInt16(0);

            this.WriteByte(0);

            this.WriteInt32(0);
        }

        public void SetEnd()
        {
            int bodylen = data.Length - GetHeadLen();

            byte[] newBytes = new byte[data.Length - 6];

            BitConverter.ToInt16(newBytes, bodylen);
        }

        //默认加密
        public void End()
        {
        UInt16 bodylen = (UInt16)(data.Length - GetHeadLen());

        data[6] = (byte)bodylen;
        data[6+1] = (byte)(bodylen >> 8);

        //BitConverter.ToUInt16(data,6);
      
        //加密处理
        this.Encrypt();
    }

    public void SetCheckCode(byte code)
        {
            data[8] = code;
            isEncrypted = true;
        }

        public byte GetCheckCode()
        {
            return data[8];
        }

        public bool IsEncrypted()
        {
            return this.isEncrypted;
        }

        public UInt16 GetCmd()
        {
            return BitConverter.ToUInt16(data, 2);

        }

        public byte GetVersion()
        {
            return this.data[4];
        }

        public byte GetSubVersion()
        {
            return this.data[5];
        }

        public UInt32 GetSequence()
        {
            return BitConverter.ToUInt32(data, 9);
        }

   
}




