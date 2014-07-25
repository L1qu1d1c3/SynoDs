﻿namespace SynoDs.Core.Api.FileStation
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Dal.Enums;
    using Dal.FileStation.CreateFolder;
    using Dal.HttpBase;
    using Dal.FileStation.Rename;

    /// <summary>
    /// Contains the File and Folder related operations
    /// Like Create, Rename, CopyMove.
    /// </summary>
    public partial class FileStation : DsClientBase
    {
        public FileStation(string userName, string password, Uri uri) : base(userName,password,uri)
        {
            SessionName = "FileStation";
        }

        /// <summary>
        /// Creates a folder with the given "name" in the given folderPath. See parameter descriptions:
        /// </summary>
        /// <param name="folderPathList">One or more shared folder paths</param>
        /// <param name="nameList">Name of the folder to create. The number of paths must be the same as the number of names in the name parameter. 
        /// The first folder_path parameter corresponds to the first name parameter.
        /// </param>
        /// <param name="forceParent">If force_parent is "true," and folder_path does not exist, the folder_path will be created. Default value is "false".
        /// If false, folderPath must exist or a false value will be returned.
        /// </param>
        /// <param name="additional"></param>
        /// <returns>The CreateFolderResponse (with information regarding the created folder)</returns>
        public async Task<CreateFolderResponse> CreateFolderAsync(IList<string> folderPathList, IList<string> nameList, bool forceParent = false, 
            CreateFolderAdditionalValues[] additional = null)
        {
            if (folderPathList.Count != nameList.Count)
                throw new ArgumentException("The number of folderPaths supplied, must be the same as the number of folders to create.");
            
            var requestParams = new RequestParameters
            {
                {"folder_path", WebUtility.UrlEncode(string.Join(",", folderPathList)) },
                {"name", string.Join(",", nameList)},
                {"force_parent", forceParent ? "true" : "false"}
            };

            if (additional != null && additional.Length > 0)
            {
                requestParams.Add("additional", string.Join(",", additional).ToLower());
            }

            return await PerformOperationAsync<CreateFolderResponse>(requestParams);
        }

        public async Task<RenameResponse> RenameAsync(IList<string> pathList, IList<string> nameList, CreateFolderAdditionalValues[] additional = null)
        {
            if (pathList.Count != nameList.Count)
                throw new ArgumentException("The number of path to the items to rename and the number of names have to be the same.");

            var requestParams = new RequestParameters
            {
                {"path", WebUtility.UrlEncode(string.Join(",", pathList))},
                {"name", string.Join(",", nameList)}
            };

            if (additional != null && additional.Length > 0)
            {
                requestParams.Add("additional", string.Join(",", additional).ToLower());
            }

            return await PerformOperationAsync<RenameResponse>(requestParams);
        }
    }
}
