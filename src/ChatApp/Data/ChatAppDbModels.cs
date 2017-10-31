using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChatApp.SharedResources;

namespace ChatApp.Data
{
    public class EntityBase
    {
        [Required]
        public DateTimeOffset? CreatedDate { get; set; }

        [Required]
        public DateTimeOffset? UpdatedDate { get; set; }

        [Required]
        [MaxLength(128)]
        public string CreatedById { get; set; }

        [Required]
        [MaxLength(128)]
        public string UpdatedById { get; set; }
    }

    public class ChatRoom : EntityBase
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public Guid? ChatRoomAvatarId { get; set; }

        public virtual ICollection<ChatMessage> ChatMessages { get; set; }

        public virtual ICollection<ChatRoomMember> ChatRoomMembers { get; set; }
    }

    public class ChatRoomMember : EntityBase
    {
        public long? Id { get; set; }

        [Required]
        public Guid ChatRoomId { get; set; }

        [Required]
        [MaxLength(128)]
        public string UserId { get; set; }

        public bool IsAdmin { get; set; }

        public virtual ChatRoom ChatRoom { get; set; }
    }

    public class ChatMessage : EntityBase
    {
        public long? Id { get; set; }

        [Required]
        public Guid ChatRoomId { get; set; }

        [MaxLength(128)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Message { get; set; }
    }

    public class ChatRoomAvatar
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid? ChatRoomId { get; set; }

        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom ChatRoom { get; set; }

        [Required]
        [MaxLength(30)]
        [RegularExpression("image/png")]
        public string ContentType { get; set; }

        [Required]
        [MaxLength(300000, ErrorMessage = SharedResource.MaxLength)]
        [Display(Name = "ImageFile")]
        public byte[] Content { get; set; }
    }

    public class UserAvatar
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(38)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [MaxLength(30)]
        [RegularExpression("image/png")]
        public string ContentType { get; set; }

        [Required]
        [MaxLength(300000, ErrorMessage = SharedResource.MaxLength)]
        [Display(Name = "ImageFile")]
        public byte[] Content { get; set; }
    }

}
