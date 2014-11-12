﻿using System.Net;
using SynoDs.Core.Dal.HttpBase;
using SynoDs.Core.Interfaces;
using SynoDs.Core.Interfaces.Synology;

namespace SynoDs.Core.Api
{
    public class RequestProvider : IRequestProvider
    {
        private readonly IAttributeReader _attributeReader;

        public RequestProvider(IAttributeReader attributeReader, IInformationProvider informationProvider)
        {
            this._attributeReader = attributeReader;
        }

        /// <summary>
        /// Todo: Implement
        /// </summary>
        /// <param name="requestParameters">The Request params.</param>
        /// <returns></returns>
        public string PrepareRequest<TResult>(RequestParameters requestParameters)
        {
            var api = _attributeReader.ReadApiNameFromT<TResult>();
            var method = _attributeReader.ReadMethodAttributeFromT<TResult>();

            var requestBase = new RequestBase
            {
                ApiName = api,
                Method = method
            };

            if (requestParameters != null)
                requestBase.RequestParameters = CleanRequestParams(requestParameters);    

         //   requestBase.Path = inform
            return "";
        }

        public RequestParameters CleanRequestParams(RequestParameters dirtyRequestParameters)
        {
            if (dirtyRequestParameters == null) return null;

            var cleanParams = new RequestParameters();
            
            foreach (var kvp in dirtyRequestParameters)
            {
                cleanParams.Add(kvp.Key, WebUtility.UrlEncode(kvp.Value));
            }

            return cleanParams;
        }
    }
}
