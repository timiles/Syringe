using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Hubs;
using Syringe.Core.Runner.Messaging;
using Syringe.Core.Tests.Results;
using Syringe.Service.Controllers.Hubs;

namespace Syringe.Service.Parallel
{
    public class TaskPublisher : ITaskPublisher
    {
        private readonly ITaskGroupProvider _taskGroupProvider;
        private readonly IHubConnectionContext<ITaskMonitorHubClient> _hubConnectionContext;
        private readonly Dictionary<Type, Action<ITaskMonitorHubClient, IMessage>> _messageMappings;

        public TaskPublisher(ITaskGroupProvider taskGroupProvider,
            IHubConnectionContext<ITaskMonitorHubClient> hubConnectionContext)
        {
            _taskGroupProvider = taskGroupProvider;
            _hubConnectionContext = hubConnectionContext;

            _messageMappings = new Dictionary<Type, Action<ITaskMonitorHubClient, IMessage>>
            {
                { typeof (TestResultMessage), SendCompletedTask },
                { typeof (TestFileGuidMessage), SendTestFileGuid }
            };
        }

        public void Start(int taskId, IObservable<IMessage> resultSource)
        {
            string taskGroup = _taskGroupProvider.GetGroupForTask(taskId);
            resultSource.Subscribe(result => OnMessage(taskGroup, result));
        }

        private void OnMessage(string taskGroup, IMessage result)
        {
            ITaskMonitorHubClient clientGroup = _hubConnectionContext.Group(taskGroup);
            _messageMappings[result.GetType()](clientGroup, result);
        }

        private void SendCompletedTask(ITaskMonitorHubClient clientGroup, IMessage message)
        {
            TestResult result = ((TestResultMessage)message).TestResult;
            CompletedTaskInfo info = new CompletedTaskInfo
            {
                Success = result.Success,
                Position = result.Position
            };

            clientGroup.OnTaskCompleted(info);
        }

        private void SendTestFileGuid(ITaskMonitorHubClient clientGroup, IMessage message)
        {
            TestFileGuidMessage testFileGuidMessage = (TestFileGuidMessage)message;
            clientGroup.OnTestFileGuid(testFileGuidMessage.ResultId.ToString());
        }
    }
}