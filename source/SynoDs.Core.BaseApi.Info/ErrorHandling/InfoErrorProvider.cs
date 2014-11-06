﻿namespace SynoDs.Core.BaseApi.Info.ErrorHandling
{
    using Api;
    using Interfaces;

    public class InfoErrorProvider : ErrorProviderBase
    {
        public InfoErrorProvider() : this(new InfoErrorRepository())
        {
        }

        public InfoErrorProvider(IErrorRepository errorRepository) : base(errorRepository)
        {
        }
    }
}
