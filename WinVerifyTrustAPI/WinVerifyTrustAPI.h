// WinVerifyTrustAPI.h

#pragma once

using namespace System;

namespace WinVerifyTrustAPI
{
    public ref class WinVerifyTrustWrapper
    {
    public:
        bool WinVerifyTrustFile(String^ sFilename);
    };
}
