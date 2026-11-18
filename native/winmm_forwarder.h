#pragma once
#include "winmm_exports.h"
extern "C" {void* winmmTargets[WINMM_COUNT]{};}
static INIT_ONCE winmmOnce=INIT_ONCE_STATIC_INIT;
static HMODULE realWinmm=nullptr;
static BOOL CALLBACK loadWinmm(PINIT_ONCE,void*,void**){
    wchar_t path[MAX_PATH];UINT length=GetSystemDirectoryW(path,MAX_PATH);
    if(!length||length+11>=MAX_PATH)return FALSE;
    wcscat_s(path,L"\\winmm.dll");realWinmm=LoadLibraryExW(path,nullptr,LOAD_LIBRARY_SEARCH_SYSTEM32);
    return realWinmm!=nullptr;
}
extern "C" uintptr_t __fastcall winmmMissing(){SetLastError(ERROR_PROC_NOT_FOUND);return 0;}
extern "C" void* __fastcall winmmResolve(unsigned index){
    if(index>=WINMM_COUNT)return (void*)winmmMissing;
    void* result=nullptr;
    if(InitOnceExecuteOnce(&winmmOnce,loadWinmm,nullptr,nullptr)){
        auto entry=winmmExports[index];result=(void*)GetProcAddress(realWinmm,entry.name?entry.name:MAKEINTRESOURCEA(entry.ordinal));
    }
    if(!result)result=(void*)winmmMissing;
    InterlockedExchangePointer(&winmmTargets[index],result);return result;
}
