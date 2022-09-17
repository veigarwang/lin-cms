﻿
using FreeSql.DataAnnotations;
using LinCms.Entities;

namespace LinCms.Plugins.Poem.Domain
{
    [Table(Name = "lin_poem")]
    public class LinPoem : FullAuditEntity
    {

        /// <summary>
        /// 作者
        /// </summary>
        [Column(StringLength = 50)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 内容，以/来分割每一句，以|来分割宋词的上下片
        /// </summary>
        [Column(DbType = "text")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 朝代
        /// </summary>
        [Column(StringLength = 50)]
        public string Dynasty { get; set; } = string.Empty;

        /// <summary>
        /// 配图
        /// </summary>
        public string Image { get; set; } = string.Empty;

        /// <summary>
        /// 标题
        /// </summary>
        [Column(StringLength = 50)]
        public string Title { get; set; } = string.Empty;


    }
}
