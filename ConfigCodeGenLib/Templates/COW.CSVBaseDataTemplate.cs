//using GCommon;
using System;
using System.Linq;

namespace COW {
    public abstract class CSVBaseData {
        public abstract void ParseData(long index, int fieldCount, string[] headers, string[] values);
        public abstract string GetPrimaryKey();
    }

    public class AutoPickupConfig : CSVBaseData {
        /// <summary>
        /// 条目ID
        /// </summary>
        public uint Id { get; private set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Desc { get; private set; }
        /// <summary>
        /// 自动拾取的东西
        /// </summary>
        public int[] ItemType { get; private set; }
        /// <summary>
        /// 子类型
        /// </summary>
        public uint[] ItemSubType { get; private set; }
        /// <summary>
        /// 下拉菜单子列
        /// </summary>
        public uint[] MenuType { get; private set; }
        /// <summary>
        /// 道具id
        /// </summary>
        public uint itemid { get; private set; }
        /// <summary>
        /// 翻译的Key
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// 默认是否开启
        /// </summary>
        public bool IsOpen { get; private set; }
        /// <summary>
        /// 默认优先级排序
        /// </summary>
        public int InitialOrder { get; private set; }
        /// <summary>
        /// 默认数量
        /// </summary>
        public int InitialNum { get; private set; }
        /// <summary>
        /// 拾取下限
        /// </summary>
        public int PickupMin { get; private set; }
        /// <summary>
        /// 拾取上限
        /// </summary>
        public int PickupMax { get; private set; }
        /// <summary>
        /// 步进单位
        /// </summary>
        public uint Unit { get; private set; }

        public override void ParseData(long index, int fieldCount, string[] headers, string[] values)
        {
            // Id = ReadUInt(Id, headers, values);
            // Desc = ReadString(Desc, headers, values);
            // ItemType = ReadIntArray(ItemType, headers, values);
            // ItemSubType = ReadUIntArray(ItemSubType, headers, values);
            // MenuType = ReadUIntArray(MenuType, headers, values);
            // itemid = ReadUInt(itemid, headers, values);
            // Text = ReadString(Text, headers, values);
            // IsOpen = ReadBoolean(IsOpen, headers, values);
            // InitialOrder = ReadInt(InitialOrder, headers, values);
            // InitialNum = ReadInt(InitialNum, headers, values);
            // PickupMin = ReadInt(PickupMin, headers, values);
            // PickupMax = ReadInt(PickupMax, headers, values);
            // Unit = ReadUInt(Unit, headers, values);
        }

        public override string GetPrimaryKey()
        {
            return "Id";
        }
    }
}

