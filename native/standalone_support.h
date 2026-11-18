#pragma once
#include <bcrypt.h>
#include <filesystem>
namespace standalone {
static std::filesystem::path localRoot(){
    wchar_t folder[32768];DWORD n=GetEnvironmentVariableW(L"LOCALAPPDATA",folder,32768);
    if(!n||n>=32768)throw "LOCALAPPDATA no disponible";
    return std::filesystem::path(folder)/L"DirtTownMenu";
}
static bool exactBuild(const wchar_t* filename){
    const unsigned char expected[]={0xD8,0xFD,0xDD,0x70,0x17,0x78,0xB9,0xC4,0xE1,0x9F,0xFE,0xED,0xC6,0x82,0x43,0xD4,0x9B,0xAC,0x98,0x3C,0x87,0xE6,0x90,0x0C,0xC6,0xD1,0x4F,0x69,0x97,0xC0,0xE8,0x98};
    HANDLE file=CreateFileW(filename,GENERIC_READ,FILE_SHARE_READ|FILE_SHARE_WRITE|FILE_SHARE_DELETE,nullptr,OPEN_EXISTING,0,nullptr);if(file==INVALID_HANDLE_VALUE)return false;
    BCRYPT_ALG_HANDLE algorithm=nullptr;BCRYPT_HASH_HANDLE hash=nullptr;bool valid=false;
    if(BCryptOpenAlgorithmProvider(&algorithm,BCRYPT_SHA256_ALGORITHM,nullptr,0)>=0&&BCryptCreateHash(algorithm,&hash,nullptr,0,nullptr,0,0)>=0){
        unsigned char buffer[65536],digest[32];DWORD n;bool success=true;
        while(true){if(!ReadFile(file,buffer,sizeof(buffer),&n,nullptr)){success=false;break;}if(!n)break;if(BCryptHashData(hash,buffer,n,0)<0){success=false;break;}}
        if(success&&BCryptFinishHash(hash,digest,sizeof(digest),0)>=0)valid=memcmp(digest,expected,32)==0;
    }
    if(hash)BCryptDestroyHash(hash);if(algorithm)BCryptCloseAlgorithmProvider(algorithm,0);CloseHandle(file);return valid;
}
static void ensureBackup(){
    static bool done=false;if(done)return;
    wchar_t folder[32768];DWORD n=GetEnvironmentVariableW(L"APPDATA",folder,32768);if(!n||n>=32768)throw "APPDATA no disponible para copiar partidas";
    auto source=std::filesystem::path(folder)/L"Dirt Town Demo";
    if(!std::filesystem::is_directory(source))throw "No se encuentra la carpeta de partidas";
    SYSTEMTIME t;GetLocalTime(&t);wchar_t stamp[96];swprintf_s(stamp,L"%04u%02u%02u-%02u%02u%02u-%03u-%lu",t.wYear,t.wMonth,t.wDay,t.wHour,t.wMinute,t.wSecond,t.wMilliseconds,GetCurrentProcessId());
    auto destination=localRoot()/L"backups"/stamp;
    std::filesystem::create_directories(destination);
    for(auto it=std::filesystem::recursive_directory_iterator(source);it!=std::filesystem::recursive_directory_iterator();++it){
        auto path=it->path();auto name=path.filename().wstring();
        if(it->is_directory()&&(name==L"log"||name==L"dumps")){it.disable_recursion_pending();continue;}
        if(!it->is_regular_file()||path.extension()==L".log"||path.extension()==L".dmp")continue;
        auto size=it->file_size();auto time=it->last_write_time();auto target=destination/std::filesystem::relative(path,source);
        std::filesystem::create_directories(target.parent_path());std::filesystem::copy_file(path,target);
        if(std::filesystem::file_size(target)!=size||it->file_size()!=size||it->last_write_time()!=time)throw "La partida cambio durante la copia; vuelve al menu y reintenta";
    }
    done=true;
}
}
