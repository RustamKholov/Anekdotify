using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Comments
{
    public class CommentUpdateDTO
    {

        [MaxLength(280, ErrorMessage = "Content cannot be over 280 characters")]
        public string CommentText { get; set; } = string.Empty;
    }
}