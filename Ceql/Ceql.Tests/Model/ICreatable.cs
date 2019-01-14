using System;
using Ceql.Contracts.Attributes;

namespace Ceql.Tests.Model
{
    public interface ICreatable
    {
        [Field("CREATION_DT")]
        DateTime CreationDate { get;}
    }
}
