using System;
using System.ComponentModel.DataAnnotations;

namespace LinCms.v1.Encyclopedias
{
    public class CreateUpdateEncyclopediaDto
    {
        /// <summary>
        /// 词条名称
        /// </summary>
        [Required(ErrorMessage = "必须传入词条名称")]
        [StringLength(20, ErrorMessage = "名称应小于20字符")]
        public string Name { get; init; }

        /// <summary>
        /// 读音
        /// </summary>
        [StringLength(45, ErrorMessage = "读音应小于45字符")]
        public string Pronunciation { get; init; }

        /// <summary>
        /// 别名
        /// </summary>
        [StringLength(25, ErrorMessage = "别名应小于25字符")]
        public string Alias { get; init; }

        /// <summary>
        /// 释名
        /// </summary>
        [StringLength(20, ErrorMessage = "释名应小于20字符")]
        public string Explanation { get; init; }

        /// <summary>
        /// 词条类别
        /// </summary>
        public short ItemType { get; init; }

        /// <summary>
        /// 原文
        /// </summary>
        [StringLength(10000, ErrorMessage = "原文应小于10000字符")]
        public string OriginalText { get; init; }

        /// <summary>
        /// 郭注
        /// </summary>
        [StringLength(4000, ErrorMessage = "郭注应小于4000字符")]
        public string Guozhu { get; init; }

        /// <summary>
        /// 图赞
        /// </summary>
        [StringLength(93, ErrorMessage = "图赞应小于93字符")]
        public string Tuzan { get; init; }

        /// <summary>
        /// 集解
        /// </summary>
        [StringLength(10000, ErrorMessage = "集解应小于10000字符")]
        public string Jijie { get; init; }

        /// <summary>
        /// 作用
        /// </summary>
        [StringLength(20, ErrorMessage = "作用应小于20字符")]
        public string Effect { get; init; }

        /// <summary>
        /// 出处
        /// </summary>
        [StringLength(90, ErrorMessage = "出处应小于90字符")]
        public string Provenance { get; init; }

        /// <summary>
        /// 图片
        /// </summary>
        [StringLength(200, ErrorMessage = "图片应小于200字符")]
        public string Picture { get; init; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1000, ErrorMessage = "备注应小于1000字符")]
        public string Remarks { get; init; }
    }
}
