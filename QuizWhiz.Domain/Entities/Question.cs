﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        [ForeignKey("QuizId")]
        public required int QuizId { get; set; }

        [Required]
        [ForeignKey("QuestionTypeId")]
        public required int QuestionTypeId { get; set; }

        [Required]
        public required string QuestionText { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        public Quiz Quiz { get; set; }

        public QuestionType QuestionType { get; set; }
    }
}
