// This is the main DLL file.

#include "stdafx.h"
#include <windows.h>
#include <softpub.h>

#include "WinVerifyTrustAPI.h"



bool WinVerifyTrustAPI::WinVerifyTrustWrapper::WinVerifyTrustFile(String^ sFilename)
{
    bool bRet = false;

    
    // init the guid

    GUID g = WINTRUST_ACTION_GENERIC_VERIFY_V2;


    // init the WINTRUST_FILE_INFO structure
    
    wchar_t sWideFilename[_MAX_PATH];
    int i; // JJF
    for ( i=0; i<sFilename->Length; i++)
    {
        sWideFilename[i] = sFilename[i];
    }
    sWideFilename[i] = 0;

    WINTRUST_FILE_INFO wfi;
    memset(&wfi, 0, sizeof(wfi));
    wfi.cbStruct = sizeof(wfi);
    wfi.pcwszFilePath = sWideFilename;
    wfi.hFile = NULL;


    // init the WINTRUST_DATA structure

    WINTRUST_DATA wd;
    memset(&wd, 0, sizeof(wd));
    wd.cbStruct = sizeof(wd);
    wd.pPolicyCallbackData = NULL;
    wd.pSIPClientData      = NULL;
    wd.dwUIChoice          = WTD_UI_NONE;
    wd.fdwRevocationChecks = WTD_REVOKE_NONE;
    wd.dwUnionChoice       = WTD_CHOICE_FILE;
    wd.pFile               = &wfi;
    wd.dwStateAction       = 0;
    wd.hWVTStateData       = NULL;


    // call the WinVerifyTrust Win32 API

    HRESULT hr = WinVerifyTrust((HWND)INVALID_HANDLE_VALUE, &g, &wd);

    if (SUCCEEDED(hr))
    {
        bRet = true;
    }

    return bRet;
}
