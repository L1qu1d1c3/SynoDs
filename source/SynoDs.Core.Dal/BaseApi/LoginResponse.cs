﻿using SynoDs.Core.Dal.Attributes;
using SynoDs.Core.Dal.Enums;
using SynoDs.Core.Dal.HttpBase;

namespace SynoDs.Core.Dal.BaseApi
{
    using System.Runtime.Serialization;
    
    /// <summary>
    /// Represents the login result. Which comes with the SID for the session. And the True / False flag.
    /// </summary>
    [DataContract]
    [Api(RootApi.API, ChildApi.Auth)]
    [AuthenticationRequired(false)]
    public class LoginResponse
         : ResponseWrapper<Login>
    {

    }
}
