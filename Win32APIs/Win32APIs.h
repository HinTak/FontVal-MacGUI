// Win32APIs.h

#pragma once

#include <Shlobj.h>


using namespace System;

namespace Win32APIs
{
    public ref class MultiByte
    {

    public:

        static bool IsCodePageInstalled(unsigned int CodePage)
        {
            return (IsValidCodePage(CodePage) != 0);
        }

        static bool IsCodePageLeadByte(unsigned int CodePage, unsigned char c)
        {
            return (IsDBCSLeadByteEx(CodePage, c) != 0);
        }


        static unsigned int GetCodePageMaxCharSize(unsigned int CodePage)
        {
            unsigned int MaxCharSize = 0;

            CPINFO cpi;
            if (GetCPInfo(CodePage, &cpi) != 0)
            {
                MaxCharSize = cpi.MaxCharSize;
            }

            return MaxCharSize;
        }


        static array<BYTE>^  GetCodePageLeadByteRanges(unsigned int CodePage)
        {
			array<BYTE>^ LeadByteRanges = gcnew array<BYTE>(0);

            CPINFO cpi;
            if (GetCPInfo(CodePage, &cpi) != 0)
            {
                int nRanges = 0;
                for (int i=0; i<5; i++)
                {
                    if (cpi.LeadByte[i*2] != 0 && cpi.LeadByte[i*2+1] != 0)
                        nRanges++;
                }

				LeadByteRanges = gcnew array<BYTE>(nRanges*2);
                for (int i=0; i<nRanges*2; i++)
                {
                    LeadByteRanges[i] = cpi.LeadByte[i];
                }
            }

            return LeadByteRanges;
        }


        static int MultiByteCharToUnicodeChar(unsigned int CodePage, wchar_t c)
        {
            int nReturnChar = -1;

            char inputbuf[2];
            wchar_t outputbuf[2];

            int nBytes = (c < 256) ? 1 : 2;
            if (nBytes == 1)
            {
                inputbuf[0] = (char)c;
            }
            else
            {
                inputbuf[0] = (char)(c>>8);
                inputbuf[1] = (char)c;
            }

            if (MultiByteToWideChar(CodePage, MB_ERR_INVALID_CHARS, inputbuf, nBytes, outputbuf, 2) != 0)
            {
                nReturnChar = outputbuf[0];
            }


            return nReturnChar;
        }

        static String^ MultiByteStringToUnicodeString(array<unsigned char>^ inputbuf, int nBytes, unsigned int CodePage)
        {
            String^ s = nullptr;

			pin_ptr<unsigned char> PinnedInputBuf = &(inputbuf[0]);

            int bufsize = MultiByteToWideChar(CodePage, MB_ERR_INVALID_CHARS, (LPCSTR)PinnedInputBuf, nBytes, NULL, 0);
            wchar_t * outputbuf = new wchar_t [bufsize+1];
            outputbuf[bufsize] = 0;


            if (MultiByteToWideChar(CodePage, MB_ERR_INVALID_CHARS, (LPCSTR)PinnedInputBuf, nBytes, outputbuf, nBytes) != 0)
            {
                s = gcnew String(outputbuf);
            }

            if (outputbuf != NULL) delete outputbuf;

            return s;
        }

    };

    public ref class SH
    {

    public:
        static String^ BrowseForFolder()
        {
            String^ strResult = nullptr;

            // We're going to use the shell to display a
            // "Choose Directory" dialog box for the user.

            LPMALLOC lpMalloc;  // pointer to IMalloc


            if (::SHGetMalloc(&lpMalloc) != NOERROR)
                return strResult; // failed to get allocator  char szDisplayName[_MAX_PATH];
            char szBuffer[_MAX_PATH];
            char szDisplayName[_MAX_PATH];
            char *lpszTitle = "Browse for folder";
            UINT ulFlags = BIF_RETURNONLYFSDIRS;


            BROWSEINFO browseInfo;
            browseInfo.hwndOwner = NULL;
            // set root at Desktop
            browseInfo.pidlRoot = NULL;
            browseInfo.pszDisplayName = szDisplayName;
            browseInfo.lpszTitle = lpszTitle;   // passed in
            browseInfo.ulFlags = ulFlags;   // also passed in
            browseInfo.lpfn = NULL;      // not used
            browseInfo.lParam = 0;      // not used     LPITEMIDLIST lpItemIDList;

            LPITEMIDLIST lpItemIDList;
            if ((lpItemIDList = ::SHBrowseForFolder(&browseInfo)) != NULL)
            {
                // Get the path of the selected folder from the
                // item ID list.
                if (::SHGetPathFromIDList(lpItemIDList, szBuffer))
                {
                    // At this point, szBuffer contains the path
                    // the user chose.
                    if (szBuffer[0] == '\0')
                    {
                        // SHGetPathFromIDList failed, or
                        // SHBrowseForFolder failed.
                        //AfxMessageBox(IDP_FAILED_GET_DIRECTORY, MB_ICONSTOP|MB_OK);
                        return strResult;
                    }

                    // We have a path in szBuffer!
                    // Return it.
                    strResult = gcnew String(szBuffer);
                    return strResult;
                }
                else
                {
                    // The thing referred to by lpItemIDList
                    // might not have been a file system object.
                    // For whatever reason, SHGetPathFromIDList
                    // didn't work!
                    //AfxMessageBox(IDP_FAILED_GET_DIRECTORY, MB_ICONSTOP|MB_OK);
                    return strResult; // strResult is empty
                }
                lpMalloc->Free(lpItemIDList);
                lpMalloc->Release();
            }// If we made it this far, SHBrowseForFolder failed.
            return strResult;
        }
    };
}
