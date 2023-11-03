using IGeekFan.FreeKit.Extras.AuditEntity;
using System;

namespace LinCms.v1.Encyclopedias
{
    public class EncyclopediaDto : EntityDto<long>
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; init; }

        /// <summary>
        /// 上次修改时间
        /// </summary>
        public DateTime UpdateTime { get; init; }

        /// <summary>
        /// 词条名称
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 读音
        /// </summary>
        public string Pronunciation { get; init; }

        /// <summary>
        /// 别名
        /// </summary>
        public string Alias { get; init; }

        /// <summary>
        /// 释名
        /// </summary>
        public string Explanation { get; init; }

        /// <summary>
        /// 词条类别
        /// </summary>
        public short ItemType { get; init; }

        /// <summary>
        /// 词条类别名
        /// </summary>
        public string ItemTypeName { get; set; }
        
        /// <summary>
        /// 原文
        /// </summary>
        public string OriginalText { get; init; }

        /// <summary>
        /// 郭注
        /// </summary>
        public string Guozhu { get; init; }

        /// <summary>
        /// 图赞
        /// </summary>
        public string Tuzan { get; init; }

        /// <summary>
        /// 集解
        /// </summary>
        public string Jijie { get; init; }

        /// <summary>
        /// 作用
        /// </summary>
        public string Effect { get; init; }

        /// <summary>
        /// 出处
        /// </summary>
        public string Provenance { get; init; }

        /// <summary>
        /// 图片
        /// </summary>
        public string Picture { get; init; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; init; }
    }
}
