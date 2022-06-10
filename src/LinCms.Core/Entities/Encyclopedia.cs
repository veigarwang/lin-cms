using FreeSql.DataAnnotations;
using System;

namespace LinCms.Entities
{
    [Table(Name = "encyclopedia")]
    public class Encyclopedia : FullAduitEntity
    {
        /// <summary>
        /// 词条名称
        /// </summary>
        [Column(StringLength = 25)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 读音
        /// </summary>
        [Column(StringLength = 30)]
        public string Pronunciation { get; set; } = string.Empty;

        /// <summary>
        /// 别名
        /// </summary>
        [Column(StringLength = 25)]
        public string Alias { get; set; } = string.Empty;

        ///<summary>
        /// 释名
        /// </summary>
        [Column(StringLength = 30)]
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// 词条类别
        /// </summary>
        public short ItemType { get; set; }

        /// <summary>
        /// 原文
        /// </summary>
        [Column(StringLength = 1000)]
        public string OriginalText { get; set; } = string.Empty;

        /// <summary>
        /// 郭注
        /// </summary>
        [Column(StringLength = 500)]
        public string Guozhu { get; set; } = string.Empty;

        /// <summary>
        /// 图赞
        /// </summary>
        [Column(StringLength = 60)]
        public string Tuzan { get; set; } = string.Empty;

        /// <summary>
        /// 集解
        /// </summary>
        [Column(StringLength = 500)]
        public string Jijie { get; set; } = string.Empty;

        /// <summary>
        /// 作用
        /// </summary>
        [Column(StringLength = 30)]
        public string Effect { get; set; } = string.Empty;

        /// <summary>
        /// 出处
        /// </summary>
        [Column(StringLength = 20)]
        public string Provenance { get; set; } = string.Empty;

        /// <summary>
        /// 图片
        /// </summary>
        [Column(StringLength = 200)]
        public string Picture { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [Column(StringLength = 1000)]
        public string Remarks { get; set; } = string.Empty;
    }

}
