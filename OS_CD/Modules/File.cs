﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace OS_CD {
    #region 别名

    using FileNodeId = Int32;
    using UserId = Int32;

    #endregion
    public class FileNode
    {
        public int ID { get; set; }
        public string name { get; set; }
        //权限表 string为权限 使用rwx 分别表示读写执行.User为对应
        private Dictionary<UserId, string> powerList;
        //所属父文件节点
        public FileNodeId fatherFileNodeId { get; set; }
        //文件事件记录
        public EventInfo eventInfo = new EventInfo();

        public FileNode(int ID, string name)
        {
            this.ID = ID;
            this.name = name;
        }

        public virtual void SetPower(UserId userId, bool read, bool writer, bool execute)
        {
            powerList[userId] = (read ? "r" : "_") + (writer ? "w" : "_") + (execute ? "x" : "_");
        }

        public virtual bool CheckReadPower(UserId userId)
        {
            return powerList[userId][0] == 'r' ? true : false;
        }

        public virtual bool CheckWriterPower(UserId userId)
        {
            return powerList[userId][1] == 'w' ? true : false;
        }

        public virtual bool CheckExecutePower(UserId userId)
        {
            return powerList[userId][2] == 'x' ? true : false;
        }

    }

    public class File : FileNode
    {
        public List<int> blockIdList = new List<int>();
        public FileBody fileBody { get; }

        public File(int ID, string name) : base(ID, name)
        {
            fileBody = new FileBody();
        }

        public int GetFileSize()
        {
            //假设文件的附属信息多占用一个单元的内存
            return fileBody.GetSize() + 1;
        }
    }

    public class FileBody
    {
        private string content;
       
        public FileBody()
        {
            content = "";
        }


        public FileBody(FileBody fileBody)
        {
            //复制构造函数
            SetContent(fileBody.content);
        }

        public void Copy(FileBody fileBody)
        {
            //复制函数
            SetContent(fileBody.content);
        }
        public void SetContent(string content)
        {    //设置内容
            this.content = content;
            //
            fileBodyChangeEvent?.Invoke();
        }

        public void AddContetnt(string content)
        {    //追加
            SetContent(this.content + content);
        }

        public int GetSize()
        {
            byte[] bytes = Encoding.Default.GetBytes(content);
            return bytes.Length;
        }

        public delegate void FileBodyChangeHandler();

        public event FileBodyChangeHandler fileBodyChangeEvent;

    }

    public class Folder : FileNode
    {
        public List<int> subFileNodeIdList = new List<int>();

        public bool AddFileNode(int index)
        {
            if (!subFileNodeIdList.Contains(index))
            {
                subFileNodeIdList.Add(index);
                return true;
            }
            else return false;
        }

        public Folder(int ID, string name) : base(ID, name)
        {

        }

        public bool IsExist(int fileNodeID)
        {
            return subFileNodeIdList.Contains(fileNodeID) ? true : false;
        }
    }

    public class EventInfo
    {
        private Dictionary<FileEvent, Dictionary<UserId, List<DateTime>>> allEventTimeList
                                        = new Dictionary<FileEvent, Dictionary<int, List<DateTime>>>();

        public KeyValuePair<UserId, DateTime> GetLastEventTime(FileEvent fileEvent)
        {
            if (!allEventTimeList.ContainsKey(fileEvent))
            {
                return new KeyValuePair<int, DateTime>(0, DateTime.MinValue);
            }

            var eventTimeList = allEventTimeList[fileEvent];
            var keyvalue = new KeyValuePair<UserId, DateTime>();

            foreach (var userTimeListPair in eventTimeList)
            {
                if (userTimeListPair.Value.Max() > keyvalue.Value)
                {
                    keyvalue = new KeyValuePair<int, DateTime>(userTimeListPair.Key,
                        userTimeListPair.Value.Max());
                }
            }
            return keyvalue;
        }

        public bool AddEventTime(FileEvent fileEvent, UserId userId, DateTime time)
        {
            if (fileEvent == FileEvent.Create &&
                allEventTimeList.ContainsKey(fileEvent))
            {
                Debug.Print("there must be something wrong to reset the file create datetime\n");
                return false;
            }
            {
                if (!allEventTimeList.ContainsKey(fileEvent))
                {
                    allEventTimeList.Add(fileEvent, new Dictionary<int, List<DateTime>>());
                }

                if (!allEventTimeList[fileEvent].ContainsKey(userId))
                {
                    allEventTimeList[fileEvent].Add(userId, new List<DateTime>());
                }
                allEventTimeList[fileEvent][userId].Add(time);
                return true;
            }
        }
    }
}