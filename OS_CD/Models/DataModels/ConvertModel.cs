﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_CD.Models {
    public enum FileType {
        File,
        Folder
    }
    /// <summary>
    /// 用于显示树状图的转换模型
    /// </summary>
    public class TFileNode {
        public string Name { get; set; }
        public int ID { get; set; }

        public int PID { get; set; }

        public List<TFileNode> Contains { get; set; }

        public void AddFileNode(TFileNode f) {
            Contains.Add(f);
        }

        public TFileNode(int id, int pid = 0, string name = "") {
            Contains = new List<TFileNode>();
            PID = pid;
            Name = name;
        }

        private TFileNode FromFileNode(FileNode f) {
            return new TFileNode(f.ID, f.fatherFileNodeId, f.name);
        }

        public TFileNode(FileNode f) {
            Contains = new List<TFileNode>();
            Name = f.name;
            ID = f.ID;
            PID = f.fatherFileNodeId;
        }
    }
}