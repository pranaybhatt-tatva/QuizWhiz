﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    internal class UserLeaderboardResponseDTO
    {
        public PaginationDTO Pagination { get; set; } = new PaginationDTO();

        public List<QuizParticipantsDTO> QuizParticipants { get; set; } = new List<QuizParticipantsDTO>();
    }
}
