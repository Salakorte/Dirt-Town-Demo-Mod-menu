#pragma once
#include "vendor/imgui/imgui.h"
namespace terrain {
alignas(32) static unsigned char camera[64]{};
static void* cameraWorld=nullptr;
static void* activeWorld=nullptr;
static unsigned captureCalls=0;
static DWORD cameraSeen=0,lastApply=0;
static bool armed=false,continuous=false,wasDown=false,levelPicked=false,pickLevel=false;
static int mode=0;
static float radius=2,strength=0.15f,hardness=0.2f,level=0;
static std::string status="Pincel desactivado";
static const char* names[]={"Subir / anadir material","Bajar","Nivelar","Suavizar"};
static const char* operations[]={"raise","lower","flatten","smooth"};
static const unsigned char types[]={1,3,7,8};
static void setSmallString(void* event,size_t offset,const char* value){
    size_t n=strlen(value);if(n>15)throw "Nombre demasiado largo para el descriptor";
    auto& s=at<GnuString>(event,offset);s.data=s.storage;s.length=n;memcpy(s.storage,value,n+1);
}
static bool hit(void* world,float sx,float sy,float width,float height,double* result){
    if(cameraWorld!=world||GetTickCount()-cameraSeen>1500||width<1||height<1)return false;
    using Pick=bool(__fastcall*)(void*,void*,float,float,float,float,void*,void*);
    return reinterpret_cast<Pick>(base+0x27D960)(world,camera,sx,sy,width,height,result,nullptr);
}
static float ground(void* world,float x,float z){return reinterpret_cast<float(__fastcall*)(void*,float,float)>(base+0x93CB90)(world,x,z);}
static bool project(const double* point,float width,float height,ImVec2& result){
    using Project=bool(__fastcall*)(void*,const void*,float,float,void*);
    return reinterpret_cast<Project>(base+0x27D770)(camera,point,width,height,&result);
}
static void apply(void* world,int material,const double* point){
    if(at<unsigned char>(world,0xEE18))throw "Los pinceles requieren una partida individual";
    if(at<uintptr_t>(world,0x17648))throw "El terreno sigue actualizandose; espera";
    if(mode<0||mode>3||!materialOK(world,material)||!std::isfinite(radius)||radius<0.5f||radius>10||!std::isfinite(strength)||strength<0.01f||strength>1||!std::isfinite(hardness)||hardness<0||hardness>1)throw "Parametros de pincel no validos";
    if(!std::isfinite(point[0])||!std::isfinite(point[1])||!std::isfinite(point[2])||fabs(point[0])>10000||fabs(point[2])>10000)throw "Punto fuera del terreno";
    if(mode==2&&(!std::isfinite(level)||fabs(level-point[1])>10))throw "La altura objetivo debe estar a menos de 10 m del terreno";
    standalone::ensureBackup();
    alignas(32) unsigned char event[0x1000]{};
    reinterpret_cast<void(__fastcall*)(void*)>(base+0x1317F20)(event);
    struct Cleanup{void* event;~Cleanup(){reinterpret_cast<void(__fastcall*)(void*)>(base+0x1318900)(event);}} cleanup{event};
    setSmallString(event,8,operations[mode]);event[0x28]=types[mode];
    auto materialRecord=(void*)(at<uintptr_t>(world,0x15B0)+size_t(material)*0x128);
    setSmallString(event,0x30,at<char*>(materialRecord,0));
    at<unsigned>(event,0)=at<unsigned>(world,0xF804);
    at<float>(event,0x50)=float(point[0]);at<float>(event,0x54)=float(point[2]);
    at<float>(event,0x58)=radius;at<float>(event,0x5C)=hardness;
    at<float>(event,0x60)=strength;at<float>(event,0x64)=mode==2?level:0;
    event[0x5A0]=0; // native "over everything" layer; supported by all four tools
    auto before=at<uintptr_t>(world,0x1100)-at<uintptr_t>(world,0x10F8);
    float oldHeight=ground(world,float(point[0]),float(point[2]));
    reinterpret_cast<void(__fastcall*)(void*,void*)>(base+0xA02A80)(world,event);
    auto after=at<uintptr_t>(world,0x1100)-at<uintptr_t>(world,0x10F8);
    if(after<=before)throw "El juego rechazo el pincel; consulta dirttown.log";
    char message[200];snprintf(message,sizeof(message),"%s aplicado en %.1f, %.1f | altura previa %.2f m",names[mode],point[0],point[2],oldHeight);status=message;
}
}
extern "C" {void* terrainOriginalUi=nullptr;void terrainUiHook();}
extern "C" void __fastcall terrainCapture(void*,void* source){
    terrain::captureCalls++;auto world=terrain::activeWorld;
    if(!worldOK(world)||!readable(source,0x30))return;
    auto p=(double*)source;auto f=(float*)((char*)source+0x20);
    if(!std::isfinite(p[0])||!std::isfinite(p[1])||!std::isfinite(p[2])||!std::isfinite(f[0])||!std::isfinite(f[1])||!std::isfinite(f[2]))return;
    memcpy(terrain::camera,source,0x30);terrain::cameraWorld=world;terrain::cameraSeen=GetTickCount();
}
