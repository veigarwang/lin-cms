using System;
using LinCms.Entities;

namespace LinCms.v1.Books
{
    public class BookDto : EntityDto
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; init; }

        /// <summary>
        /// 国际标准书号
        /// </summary>
        public string Isbn { get; set; }

        /// <summary>
        /// 书名
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; init; }

        /// <summary>
        /// 完整书名
        /// </summary>
        public string ShowName { get; set; }

        /// <summary>
        /// 书籍类别编码
        /// </summary>
        public short BookType { get; init; }

        /// <summary>
        /// 书籍类别名称
        /// </summary>
        public string BookTypeName { get; set; }

        /// <summary>
        /// 第一著者类型
        /// </summary>
        public short AuthorType1 { get; init; }

        /// <summary>
        /// 第一著者
        /// </summary>
        public string Author1 { get; init; }

        /// <summary>
        /// 第二著者类型
        /// </summary>
        public short AuthorType2 { get; init; }

        /// <summary>
        /// 第二著者
        /// </summary>
        public string Author2 { get; init; }

        /// <summary>
        /// 第三著者类型
        /// </summary>
        public short AuthorType3 { get; init; }

        /// <summary>
        /// 第三著者
        /// </summary>
        public string Author3 { get; init; }

        /// <summary>
        /// 出版公司
        /// </summary>
        public string Press { get; init; }

        /// <summary>
        /// 印刷公司
        /// </summary>
        public string Printing { get; init; }

        /// <summary>
        /// 开本
        /// </summary>
        public string Kaiben { get; init; }

        /// <summary>
        /// 字数
        /// </summary>
        public decimal WordCount { get; init; } = decimal.Zero;

        /// <summary>
        /// 印章
        /// </summary>
        public decimal Yinzhang { get; init; } = decimal.Zero;

        /// <summary>
        /// 版次
        /// </summary>
        public string EditionNumber { get; init; }

        /// <summary>
        /// 印次
        /// </summary>
        public string Impression { get; init; }

        /// <summary>
        /// 定价
        /// </summary>
        public decimal Price { get; init; }

        /// <summary>
        /// 册数
        /// </summary>
        public string Volumes { get; init; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string Cover { get; init; }

        /// <summary>
        /// 购买日期
        /// </summary>
        public DateTime DatePurchased { get; init; }

        /// <summary>
        /// 已读
        /// </summary>
        public bool IsRead { get; init; }

        /// <summary>
        /// 评分
        /// </summary>
        public short Rate { get; init; }

        /// <summary>
        /// 读后感
        /// </summary>
        public string Summary { get; init; }

        public DateTime CreateTime { get; init; }

        public DateTime UpdateTime { get; init; }
    }
}
