﻿using System;
using System.ComponentModel.DataAnnotations;

namespace LinCms.v1.Books
{
    public class CreateUpdateBookDto
    {
        /// <summary>
        /// 国际标准书号
        /// </summary>
        [Required(ErrorMessage = "必须传入ISBN")]
        [StringLength(13, ErrorMessage = "ISBN应小于13字符")]
        public string ISBN { get; set; }

        /// <summary>
        /// 书名
        /// </summary>
        [Required(ErrorMessage = "必须传入书名")]
        [StringLength(25, ErrorMessage = "书名应小于25字符")]
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        [StringLength(30, ErrorMessage = "副标题应小于30字符")]
        public string Subtitle { get; set; }

        /// <summary>
        /// 书籍类别
        /// </summary>
        public short BookType { get; init; }

        /// <summary>
        /// 第一著者类型
        /// </summary>
        [Required(ErrorMessage = "必须传入第一著者类型")]
        public short AuthorType1 { get; set; }

        /// <summary>
        /// 第一著者
        /// </summary>
        [Required(ErrorMessage = "必须传入第一著者")]
        [StringLength(12, ErrorMessage = "第一著者应小于12字符")]
        public string Author1 { get; set; }

        /// <summary>
        /// 第二著者类型
        /// </summary>
        public short AuthorType2 { get; set; }

        /// <summary>
        /// 第二著者
        /// </summary>
        [StringLength(12, ErrorMessage = "第二著者应小于12字符")]
        public string Author2 { get; set; }

        /// <summary>
        /// 第三著者类型
        /// </summary>
        public short AuthorType3 { get; set; }

        /// <summary>
        /// 第三著者
        /// </summary>
        [StringLength(12, ErrorMessage = "第三著者应小于12字符")]
        public string Author3 { get; set; }

        /// <summary>
        /// 出版公司
        /// </summary>
        [Required(ErrorMessage = "必须传入出版公司")]
        [StringLength(20, ErrorMessage = "出版公司应小于20字符")]
        public string Press { get; set; }

        /// <summary>
        /// 印刷公司
        /// </summary>
        [StringLength(20, ErrorMessage = "印刷公司应小于20字符")]
        public string Printing { get; set; }

        /// <summary>
        /// 开本
        /// </summary>
        [StringLength(30, ErrorMessage = "开本应小于30字符")]
        public string Kaiben { get; set; }

        /// <summary>
        /// 字数
        /// </summary>
        public decimal WordCount { get; set; }

        /// <summary>
        /// 印章
        /// </summary>
        public decimal Yinzhang { get; set; }

        /// <summary>
        /// 版次
        /// </summary>
        [StringLength(12, ErrorMessage = "版次应小于12字符")]
        public string EditionNumber { get; set; }

        /// <summary>
        /// 印次
        /// </summary>
        [StringLength(14, ErrorMessage = "印次应小于14字符")]
        public string Impression { get; set; }

        /// <summary>
        /// 定价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 册数
        /// </summary>
        [StringLength(5, ErrorMessage = "册数应小于5字符")]
        public string Volumes { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [StringLength(200, ErrorMessage = "封面图应小于200字符")]
        public string Cover { get; set; }

        /// <summary>
        /// 购买日期
        /// </summary>
        public DateTime DatePurchased { get; set; }

        /// <summary>
        /// 已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public short Rate { get; set; }

        /// <summary>
        /// 读后感
        /// </summary>
        [StringLength(1000, ErrorMessage = "读后感应小于1000字符")]
        public string Summary { get; set; }
    }
}
