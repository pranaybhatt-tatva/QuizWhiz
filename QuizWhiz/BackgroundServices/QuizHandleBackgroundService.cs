﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuizWhiz.Application.Interface;
using System.Threading;
using Timer = System.Threading.Timer;
using Microsoft.AspNetCore.SignalR;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Domain.Entities;

public class QuizHandleBackgroundService : BackgroundService
{
    private readonly ILogger<QuizHandleBackgroundService> _logger;
    private readonly QuizServiceManager _quizServiceManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<QuizHub> _hubContext;
    private readonly string _quizId;
    public DateTime QuizScheduleTime = DateTime.Now;
    /*private Timer _timer;*/
    private int TimerSeconds=0;
    private bool Timer = true;
    public List<GetQuestionsDTO> _questions;
    public int QuestionNo=0;
    public QuizHandleBackgroundService(ILogger<QuizHandleBackgroundService> logger, string quizId, IServiceScopeFactory serviceScopeFactory, IHubContext<QuizHub> hubContext, QuizServiceManager quizServiceManager)
    {
        _logger = logger;
        _quizId = quizId;
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
        _quizServiceManager = quizServiceManager;   
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        /*_logger.LogInformation("Quiz {QuizId} BackgroundService is starting.", _quizId);*/

        while (!stoppingToken.IsCancellationRequested)
        { 
            /*_logger.LogInformation("Quiz {QuizId} is active.", _quizId);*/
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                if (TimerSeconds == 0)
                {
                    var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                    var Questions = await quizService.GetAllQuestions(_quizId);
                    var ContestTime = await quizService.GetQuizTime(_quizId);
                    DateTime ContestStartTime = (DateTime)ContestTime.Data;
                    QuizScheduleTime = ContestStartTime;
                    _questions = Questions.Data as List<GetQuestionsDTO>;
                }
                QuizHandleMethod(null);
                /*_timer = new Timer(QuizHandleMethod, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));*/
                await Task.Delay(1000, stoppingToken);
            }
        }
        /*_logger.LogInformation("Quiz {QuizId} BackgroundService is stopping.", _quizId);*/
    }
    private async void QuizHandleMethod(object state)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            ++TimerSeconds;
            _logger.LogInformation($"Timed Hosted Service is working. Timer seconds: {TimerSeconds}");

            if (TimerSeconds > 30 && Timer)
            {
                Timer = false;
                TimerSeconds = 1;
            }
            if (Timer)
            {
                DateTime notifyTime = QuizScheduleTime.AddMinutes(-5);
                var currentTime = DateTime.Now;
                var remainingTime = QuizScheduleTime - currentTime;
                if (currentTime >= notifyTime)
                {
                    await _hubContext.Clients.All.SendAsync($"ReceiveRemainingTime_{_quizId}", remainingTime.Minutes,remainingTime.Seconds);
                }
            }
            else
            {
                if (QuestionNo >= _questions.Count)
                {
                    _quizServiceManager.StopQuizService(_quizId);
                    return ;
                }
                var Question = _questions.ElementAt(QuestionNo);
                var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                var CorrectAnswer=await quizService.GetCorrectAnswer(Question.QuestionId);
                List<string>options= new List<string>();
                foreach(var ele in Question.Options)
                {
                    options.Add(ele.OptionText!.ToString());
                }
                SendQuestionDTO sendQuestionDTO = new SendQuestionDTO()
                {
                    Question=Question.Question,
                    Options=options
                };
                if (TimerSeconds <=16)
                {
                    await _hubContext.Clients.All.SendAsync($"ReceiveQuestion_{_quizId}",QuestionNo + 1, sendQuestionDTO, TimerSeconds);
                }
                else if(TimerSeconds>=17 && TimerSeconds<20)
                {
                    await _hubContext.Clients.All.SendAsync($"ReceiveAnswer_{_quizId}", QuestionNo + 1, CorrectAnswer.Data, TimerSeconds);
                }
                else if(TimerSeconds==20)
                {
                    TimerSeconds = 0;
                    await _hubContext.Clients.All.SendAsync($"ReceiveAnswer_{_quizId}", QuestionNo + 1, CorrectAnswer, TimerSeconds);
                    ++QuestionNo;
                }
            }
      }
    }
  
}