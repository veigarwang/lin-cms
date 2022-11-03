using System;
using IGeekFan.FreeKit.Extras.AuditEntity;

namespace LinCms.Base.BaseItems
{
    public class BaseItemDto : EntityDto
    {
        public int BaseTypeId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemDetails { get; set; }
        public bool Status { get; set; }
        public int? SortCode { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
