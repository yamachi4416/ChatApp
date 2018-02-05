using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ChatApp.SharedResources;

namespace ChatApp.Features.UserAvatar.Models
{
    public class UploadAvatarModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [DataType(DataType.Upload)]
        [Display(Name = nameof(ImageFile))]
        public IFormFile ImageFile { get; set; }
    }
}