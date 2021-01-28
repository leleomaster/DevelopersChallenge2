using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Nibo.Conciliacao.Bancaria.Models
{
    public class FileModel
    {
        [Required(ErrorMessage = "Please select file.")]
        [Display(Name = "Browse File")]
        public HttpPostedFileBase[] files { get; set; }

    }
}