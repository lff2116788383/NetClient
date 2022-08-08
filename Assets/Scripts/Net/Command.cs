using System;
using System.Collections.Generic;
using System.Text;

namespace Command
{
    //ADMIN CMD
    public enum ADMIN
    {
        CMD_ADMIN_LOGIN = 0x1001
    }

    //CLIENT CMD

    public enum CLIENT
    {
        CMD_CLIENT_HEARTBEAT_REQ = 0x3000, //心跳请求

        CMD_CLIENT_LOGIN_REQ = 0x3001, //登录请求

        CMD_CLIENT_REGISTER_REQ = 0x3002, //注册请求

        CMD_CLIENT_LOGOUT_REQ = 0x3003,//登出请求


        //房间地图模块
        CMD_CLIENT_GET_TABLES_INFO_REQ = 0x3010, // 获取所有房间信息请求

    CMD_CLIENT_ENTER_TABLE_REQ = 0x3011, // 进入房间请求

    CMD_CLIENT_LEAVE_TABLE_REQ = 0x3012, // 离开房间请求


    //角色模块
        CMD_CLIENT_GET_ROLES_INFO_REQ = 0x3020 ,//获取角色信息请求

    CMD_CLIENT_CREATE_ROLE_REQ = 0x3021, //创建角色请求

    CMD_CLIENT_DEL_ROLE_REQ = 0x3022, //删除角色请求


    //商店模块
        CMD_CLIENT_GET_STORE_INFO_REQ = 0x3030, //获取商店信息请求

    CMD_CLIENT_STORE_BUY_REQ = 0x3031, //商店购买请求

    CMD_CLIENT_STORE_SELL_REQ = 0x3032, //商店出售请求



    //战斗模块
        CMD_CLIENT_MOVE_REQ = 0x3090 ,//移动请求

    CMD_CLIENT_ATTACK_REQ = 0x3091, //攻击动作请求

    CMD_CLIENT_HIT_REQ = 0x3092 ,//攻击伤害请求

    CMD_CLIENT_DIE_REQ = 0x3093 ,//角色死亡请求


    }


    //SERVER CMD
    public enum SERVER
    {
        CMD_SERVER_HEARTBEAT_RESP = 0x4000, //心跳响应

        CMD_SERVER_LOGIN_RESP = 0x4001, //登录结果响应

        CMD_SERVER_REGISTER_RESP = 0x4002, //注册结果响应

        CMD_SERVER_LOGOUT_RESP = 0x4003, //登出结果响应


        CMD_SERVER_GET_TABLES_INFO_RESP = 0x4005, //获取所有房间信息结果响应

    CMD_SERVER_ENTER_TABLE_RESP = 0x4006, //进入房间结果响应

    //角色模块
        CMD_SERVER_GET_ROLES_INFO_RESP = 0x4020, //获取角色信息请求

    CMD_SERVER_CREATE_ROLE_RESP = 0x4021, //创建角色请求

    CMD_SERVER_DEL_ROLE_RESP = 0x4022, //删除角色请求


    //商店模块
        CMD_SERVER_GET_STORE_INFO_RESP = 0x4030, //获取商店信息请求

    CMD_SERVER_STORE_BUY_RESP = 0x4031, //商店购买请求

    CMD_SERVER_STORE_SELL_RESP = 0x4032, //商店出售请求



    CMD_SERVER_MOVE_RESP = 0x4010, //移动结果响应


    CMD_SERVER_KICK_RESP = 0x1003 //踢出结果响应
    }


    //SERVER BROCAST CMD
    public enum BROCAST
    {
        CMD_SERVER_BROCAST_LOGIN_SUCC = 0x5000,//广播用户登录成功
    }
}
