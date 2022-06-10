using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinCms.Base.BaseItems
{
    public class CreateUpdateBaseItemDto : IValidatableObject
    {
        public int BaseTypeId { get; set; }
        [Required(ErrorMessage = "编码为必填项")]
        [StringLength(50, ErrorMessage = "名称应小于50字符")]
        public string ItemCode { get; set; }
        [Required(ErrorMessage = "字典类别为必填项")]
        [StringLength(50, ErrorMessage = "名称应小于50字符")]
        public string ItemName { get; set; }
        [StringLength(50, ErrorMessage = "名称应小于50字符")]
        public string ItemDetails { get; set; }
        public bool Status { get; set; }
        public int? SortCode { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BaseTypeId == 0)
            {
                yield return new ValidationResult("请选择类别", new List<string>() { "BaseTypeId" });
            }
        }
    }
}
