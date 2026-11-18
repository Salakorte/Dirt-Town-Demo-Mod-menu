#pragma once
#include <cmath>
namespace freecam {
struct Pose { double x,y,z,pad; float yaw,pitch,fov,roll; };
static_assert(sizeof(Pose)==0x30);
static bool valid(const Pose& p){return std::isfinite(p.x)&&std::isfinite(p.y)&&std::isfinite(p.z)&&fabs(p.x)<10000&&fabs(p.y)<10000&&fabs(p.z)<10000&&std::isfinite(p.yaw)&&std::isfinite(p.pitch)&&std::isfinite(p.fov)&&p.fov>0&&p.fov<180&&std::isfinite(p.roll);}
static bool move(Pose& p,float forward,float right,float up,float dx,float dy,float dt,bool fast){
    if(!valid(p)||!std::isfinite(dt)||!std::isfinite(dx)||!std::isfinite(dy)||!std::isfinite(forward)||!std::isfinite(right)||!std::isfinite(up))return false;
    Pose next=p;dt=fmaxf(0,fminf(dt,0.05f));
    next.yaw=remainderf(next.yaw+fmaxf(-200,fminf(dx,200))*0.003f,6.2831853f);
    next.pitch=fmaxf(-1.5f,fminf(1.5f,next.pitch-fmaxf(-200,fminf(dy,200))*0.003f));
    // Same basis as native projection 27D770: (sin(yaw)*cos(pitch), sin(pitch), -cos(yaw)*cos(pitch)).
    float sy=sinf(next.yaw),cy=cosf(next.yaw),sp=sinf(next.pitch),cp=cosf(next.pitch);
    float length=sqrtf(forward*forward+right*right+up*up),step=dt*(fast?30.f:8.f)/fmaxf(1,length);
    next.x+=(sy*cp*forward+cy*right)*step;
    next.y+=(sp*forward+up)*step;
    next.z+=(-cy*cp*forward+sy*right)*step;
    if(!valid(next))return false;p=next;return true;
}
static int selfTest(){
    Pose p{0,0,0,0,0,0,55,0};if(!move(p,1,0,0,0,0,.05f,false)||fabs(p.z+.4)>.001||fabs(p.x)>.001)return 1;
    p={0,0,0,0,0,0,55,0};move(p,0,1,1,0,0,.05f,false);if(fabs(sqrt(p.x*p.x+p.y*p.y)-.4)>.001)return 2;
    p={0,0,0,0,0,0,55,0};move(p,1,0,0,0,0,10,true);if(fabs(p.z+1.5)>.001)return 3;
    p={0,0,0,0,0,0,55,0};if(move(p,1,0,0,NAN,0,.01f,false)||p.z!=0)return 4;
    p={9999.9,0,0,0,0,0,55,0};if(move(p,0,1,0,0,0,.05f,true)||p.x!=9999.9)return 5;
    p={0,0,0,0,0,1.49f,55,0};move(p,0,0,0,0,-100,.01f,false);if(p.pitch>1.5f||p.fov!=55)return 6;
    return 0;
}
}
