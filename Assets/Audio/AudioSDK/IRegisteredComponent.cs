using System;

namespace AudioSDK
{
    public interface IRegisteredComponent
    {
        Type GetRegisteredComponentBaseClassType();
    }
}
