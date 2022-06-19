using FreeSql.DataAnnotations;
using System;

namespace LinCms.Entities
{
    [Table(Name = "book")]
    public class Book : FullAduitEntity
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [Column(IsVersion = true)]
        public int Version { get; set; }

        /// <summary>
        /// 国际标准书号
        /// </summary>        
        [Column(StringLength = 13)]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// 书名
        /// </summary>
        [Column(StringLength = 25)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 副标题
        /// </summary>
        [Column(StringLength = 30)]
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>
        /// 书籍类别
        /// </summary>
        public short BookType { get; set; }

        /// <summary>
        /// 第一著者类型
        /// </summary>
        public short AuthorType1 { get; set; }

        /// <summary>
        /// 第一著者
        /// </summary>
        [Column(StringLength = 12)]
        public string Author1 { get; set; } = string.Empty;

        /// <summary>
        /// 第二著者类型
        /// </summary>
        public short AuthorType2 { get; set; }

        /// <summary>
        /// 第二著者
        /// </summary>
        [Column(StringLength = 12)]
        public string Author2 { get; set; } = string.Empty;

        /// <summary>
        /// 第三著者类型
        /// </summary>
        public short AuthorType3 { get; set; }

        /// <summary>
        /// 第三著者
        /// </summary>
        [Column(StringLength = 12)]
        public string Author3 { get; set; } = string.Empty;

        /// <summary>
        /// 出版公司
        /// </summary>
        [Column(StringLength = 20)]
        public string Press { get; set; } = string.Empty;

        /// <summary>
        /// 印刷公司
        /// </summary>
        [Column(StringLength = 20)]
        public string Printing { get; set; } = string.Empty;

        /// <summary>
        /// 开本
        /// </summary>
        [Column(StringLength = 30)]
        public string Kaiben { get; set; } = string.Empty;

        /// <summary>
        /// 字数
        /// </summary>
        public decimal WordCount { get; set; } = decimal.Zero;

        /// <summary>
        /// 印张
        /// </summary>
        public decimal Yinzhang { get; set; } = decimal.Zero;

        /// <summary>
        /// 版次
        /// </summary>
        [Column(StringLength = 12)]
        public string EditionNumber { get; set; } = string.Empty;

        /// <summary>
        /// 印次
        /// </summary>
        [Column(StringLength = 14)]
        public string Impression { get; set; } = string.Empty;

        /// <summary>
        /// 定价
        /// </summary>
        public decimal Price { get; set; } = decimal.Zero;

        /// <summary>
        /// 册数
        /// </summary>
        [Column(StringLength = 5)]
        public string Volumes { get; set; } = string.Empty;

        /// <summary>
        /// 封面图
        /// </summary>
        [Column(StringLength = 200)]
        public string Cover { get; set; } = string.Empty;

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
        [Column(StringLength = 1000)]
        public string Summary { get; set; } = string.Empty;
    }

}
