﻿using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Application.Interface;
using QuizWhiz.Application.Interfaces;
using QuizWhiz.DataAccess.Data;
using QuizWhiz.Domain.Helpers;

namespace QuizWhiz.API.Controllers
{
    [Route("api/quiz")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IQuizService _quizService;

        public QuizController(ApplicationDbContext context, IConfiguration configuration, IQuizService quizService)
        {
            _context = context;
            _configuration = configuration;
            _quizService = quizService;
        }

        [HttpPost("create-new-quiz")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [CustomAuthorize("Admin")]
        public async Task<ResponseDTO> CreateNewQuiz([FromBody] QuizDTO quizDTO)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return await _quizService.CreateNewQuizAsync(quizDTO);
        }
    }
}
