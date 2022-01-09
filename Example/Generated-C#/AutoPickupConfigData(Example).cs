
using GCommon;
using System;
using System.Linq;

namespace COW {
    public class AutoPickupConfigData : CSVBaseData
    {
        public int Id;
        public uint[] ItemTypes;
        public uint[] SubTypes;
        public int[] MenuType;
        public uint ItemId;
        public string LocKey;
        public bool IsDefaultOpen;
        public int InitialOrder;
        public int InitialNum;
        public int PickupMin;
        public int PickupMax;
        public int PickupUnit;

        public override void ParseData(long index, int fieldCount, string[] headers, string[] values)
        {
            Id = ReadInt("Id", headers, values, 0);
            ItemTypes = ReadUIntArray("ItemType", headers, values, 0 ,';');
            SubTypes = ReadUIntArray("ItemSubType" , headers , values, 0 ,';');
            MenuType = ReadIntArray("MenuType", headers, values, 0, ';');
            ItemId = ReadUInt("itemid", headers, values);
            LocKey = ReadString("Text", headers, values, string.Empty);
            IsDefaultOpen = ReadBoolean("IsOpen",headers , values , true);
            InitialOrder = ReadInt("InitialOrder", headers, values);
            InitialNum = ReadInt("InitialNum", headers, values);
            PickupMin = ReadInt("PickupMin", headers, values);
            PickupMax = ReadInt("PickupMax", headers, values);
            PickupUnit = ReadInt("Unit", headers, values, 1); // avoid divided by zero
        }

        /// <summary>
        /// auto pick up settings from different setting group may have the same primary key
        /// primary key cannot be changed because the foremost PlayerPrefs are saved with PrimaryKey not NewKey
        /// please ignore related log warnings
        /// </summary>
        /// <remarks>http://wiki.jingle.cn/pages/viewpage.action?pageId=98201909</remarks>
        /// <returns></returns>
        public override string GetPrimaryKey()
        {
            return string.Empty;
        }
    }
}
