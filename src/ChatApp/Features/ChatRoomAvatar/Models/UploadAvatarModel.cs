using System.ComponentModel.DataAnnotations;
using ChatApp.SharedResources;
using Microsoft.AspNetCore.Http;

namespace ChatApp.Features.ChatRoomAvatar.Models
{
    public class UploadAvatarModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [DataType(DataType.Upload)]
        [Display(Name = nameof(ImageFile))]
        public IFormFile ImageFile { get; set; }
    }
}