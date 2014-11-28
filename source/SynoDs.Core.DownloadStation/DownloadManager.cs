﻿using SynoDs.Core.Api;
using SynoDs.Core.Contracts.Synology;
using SynoDs.Core.Exceptions;

namespace SynoDs.Core.DownloadStation
{
    using CrossCutting.Common;
    using Dal.DownloadStation.Task;
    using Dal.Enums;
    using Dal.HttpBase;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// DownloadStation client class.
    /// </summary>
    public sealed class DownloadManager : IDownloadProvider
    {
        private readonly IOperationProvider _operationProvider;
        private readonly IAuthenticationProvider _authenticationProvider;
        public const string DlSessionName = "DownloadStation";
        private string _sessionId = string.Empty;

        private DsClient DsClient { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DownloadManager(IOperationProvider operationProvider, IAuthenticationProvider authenticationProvider)
        {
            Validate.ArgumentIsNotNullOrEmpty(operationProvider);
            Validate.ArgumentIsNotNullOrEmpty(operationProvider);

            this._operationProvider = operationProvider;
            this._authenticationProvider = authenticationProvider;


        }

        /// <summary>
        /// Retrieves a list of tasks that are available on the DownloadStation
        /// </summary>
        /// <param name="offset">For pagination purposes.</param>
        /// <param name="limit">Limit the number of tasks to retrieve.</param>
        /// <param name="additionalInfo">Additional information <see cref="TaskAdditionalInfoValues"/></param>
        /// <returns>A list of Tasks, <see cref="TaskListResponse"/></returns>
        public async Task<TaskListResponse> ListTasksAsync(int offset = 0, int limit = -1,
            TaskAdditionalInfoValues[] additionalInfo = null)
        {
            await PerformLoginIfRequired();

            var requestParams = new RequestParameters
            {
                {"_sid", _sessionId}
            };

            if (offset != 0)
                requestParams.Add("offset", offset.ToString());

            if (limit != -1)
                requestParams.Add("limit", limit.ToString());

            if (additionalInfo != null && additionalInfo.Length != 0)
            {
                requestParams.Add("additional", string.Join(",", additionalInfo).ToLower());
            }

            if (requestParams.Count > 0)
            {
                return await _operationProvider.PerformOperationAsync<TaskListResponse>(requestParams);
            }
            return await _operationProvider.PerformOperationAsync<TaskListResponse>(null);
        }


        /// <summary>
        /// Calls the AuthenticationProvider to login if the SessionId is still emtpy. 
        /// </summary>
        /// <returns></returns>
        private async Task PerformLoginIfRequired()
        {
            if (_authenticationProvider.Sid == string.Empty)
            {
                var loginResult = await _authenticationProvider.LoginAsync();
                if (loginResult)
                {
                    this._sessionId = _authenticationProvider.Sid;
                }
                else throw new SynologyException("Error logging in.");
            }
        }

        /// <summary>
        /// Gets the information on the Task(s) id's supplied. 
        /// </summary>
        /// <param name="taskList">The ID's of the tasks to get information on.</param>
        /// <param name="additionalInfo">Additional info to request. <see cref="TaskAdditionalInfoValues"/></param>
        /// <returns>A task information result. <see cref="TaskGetInfoResponse"/></returns>
        public async Task<TaskGetInfoResponse> GetTaskInfoAsync(IList<string> taskList,
            TaskAdditionalInfoValues[] additionalInfo = null)
        {
            Validate.ArgumentIsNotNullOrEmpty(taskList);

            await PerformLoginIfRequired();

            var requestParams = new RequestParameters
            {
                {"id", string.Join(",", taskList)},
                {"_sid", _sessionId }
            };

            if (additionalInfo != null)
                requestParams.Add("additional", string.Join(",", additionalInfo).ToLower());

            return await _operationProvider.PerformOperationAsync<TaskGetInfoResponse>(requestParams);
        }

        /// <summary>
        /// Creates a Download task on the DownloadStation using the supplied URL and additional optional parameters.
        /// </summary>
        /// <param name="taskUrl">The URI for the download to create. Can be any valid URI.</param>
        /// <param name="userName">Optional username if the download requires authentication.</param>
        /// <param name="password">Optional password if the download requires authentication.</param>
        /// <param name="unzipPass">Unzip password if it is a zip file download and it has a password.</param>
        /// <param name="fileStream">FileStream for uploading a torrent file directly from your client.</param>
        /// <returns>A creation response, <see cref="CreateTaskResponse"/></returns>
        public async Task<CreateTaskResponse> CreateTaskAsync(string taskUrl, string userName = "", 
            string password = "", string unzipPass = "", Stream fileStream = null)
        {

            Validate.ArgumentIsNotNullOrEmpty(taskUrl);

            await PerformLoginIfRequired();

            // Todo: refactor parameter parsing
            var requestParams = new RequestParameters
            {
                { "uri", taskUrl },
                {"_sid", _sessionId }
            };
            if (userName != string.Empty)
                requestParams.Add("username", userName);

            if (password != string.Empty)
                requestParams.Add("password", password);

            if (unzipPass != string.Empty)
                requestParams.Add("unzip_password", unzipPass);
            
            if (fileStream != null && fileStream.Length >0)
                return await _operationProvider.PerformOperationWithFileAsync<CreateTaskResponse>(requestParams, fileStream);

            return await _operationProvider.PerformOperationAsync<CreateTaskResponse>(requestParams);
        }

        /// <summary>
        /// Deletes a Task(s) from the DownloadStation
        /// </summary>
        /// <param name="taskList">List of Task ID's to delete.</param>
        /// <param name="forceComplete">Delete tasks and force to move uncompleted download files to the destination.</param>
        /// <returns>A delete response, with a list of ID's and a boolean value indicating if it was deleted correctly.
        ///  <see cref="DeleteTaskResponse"/></returns>
        public async Task<DeleteTaskResponse> DeleteTaskAsync(IList<string> taskList, bool forceComplete)
        {
            Validate.ArgumentIsNotNullOrEmpty(taskList);
            
            await PerformLoginIfRequired();

            var requestParams = new RequestParameters
            {
                {"id", string.Join(",", taskList)},
                {"force_complete", forceComplete ? "true" : "false"},
                {"_sid", _sessionId }
            };

            return await _operationProvider.PerformOperationAsync<DeleteTaskResponse>(requestParams);
        }
        
        /// <summary>
        /// Pause Tasks.
        /// </summary>
        /// <param name="taskList">The list of tasks to pause.</param>
        /// <returns>A list of Task Id's with the boolean value indicating if it was paused correctly.</returns>
        public async Task<PauseTaskResponse> PauseTaskAsync(IList<string> taskList)
        {
            Validate.ArgumentIsNotNullOrEmpty(taskList);

            await PerformLoginIfRequired();

            var requestParams = new RequestParameters
            {
                {"id", string.Join(",", taskList)},
                {"_sid", _sessionId }
            };

            return await _operationProvider.PerformOperationAsync<PauseTaskResponse>(requestParams);
        }

        /// <summary>
        /// Resume Tasks
        /// </summary>
        /// <param name="taskList">The list of tasks to resume.</param>
        /// <returns>A list of Id's with the boolean value indicating if it was properly resumed.</returns>
        public async Task<ResumeTaskResponse> ResumeTaskAsync(IList<string> taskList)
        {
            Validate.ArgumentIsNotNullOrEmpty(taskList);
            
            await PerformLoginIfRequired();

            var requestParams = new RequestParameters
            {
                {"id", string.Join(",", taskList)},
                {"_sid", _sessionId }
            };

            return await _operationProvider.PerformOperationAsync<ResumeTaskResponse>(requestParams);
        }
    }
}
