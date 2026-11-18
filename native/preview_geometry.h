#pragma once
#include <cmath>
#include "vendor/imgui/imgui.h"
namespace preview {
// Clip before calling the game's renderer: never submit NaN, infinity or unbounded edges.
static bool clip(ImVec2& a,ImVec2& b,float width,float height){
    if(!std::isfinite(a.x)||!std::isfinite(a.y)||!std::isfinite(b.x)||!std::isfinite(b.y)||!std::isfinite(width)||!std::isfinite(height)||width<=0||height<=0)return false;
    double x=a.x,y=a.y,u=b.x,v=b.y;
    auto code=[&](double px,double py){return (px<0?1:px>width?2:0)|(py<0?4:py>height?8:0);};
    for(int n=0;n<8;n++){
        int ca=code(x,y),cb=code(u,v);if(!(ca|cb)){a=ImVec2(float(x),float(y));b=ImVec2(float(u),float(v));return true;}
        if(ca&cb)return false;int c=ca?ca:cb;double nx,ny;
        if(c&8){if(v==y)return false;nx=x+(u-x)*(height-y)/(v-y);ny=height;}
        else if(c&4){if(v==y)return false;nx=x+(u-x)*(-y)/(v-y);ny=0;}
        else if(c&2){if(u==x)return false;ny=y+(v-y)*(width-x)/(u-x);nx=width;}
        else{if(u==x)return false;ny=y+(v-y)*(-x)/(u-x);nx=0;}
        if(!std::isfinite(nx)||!std::isfinite(ny))return false;
        if(c==ca){x=nx;y=ny;}else{u=nx;v=ny;}
    }return false;
}
static int selfTest(){
    ImVec2 a(10,20),b(90,70);if(!clip(a,b,100,80)||a.x!=10||b.x!=90)return 1;
    a=ImVec2(-100,20);b=ImVec2(200,20);if(!clip(a,b,100,80)||a.x!=0||b.x!=100)return 2;
    a=ImVec2(-100,-20);b=ImVec2(-10,-1);if(clip(a,b,100,80))return 3;
    a=ImVec2(NAN,0);b=ImVec2(20,20);if(clip(a,b,100,80))return 4;
    a=ImVec2(0,0);b=ImVec2(INFINITY,20);if(clip(a,b,100,80))return 5;
    a=ImVec2(-1e30f,40);b=ImVec2(1e30f,40);if(!clip(a,b,100,80)||a.x!=0||b.x!=100)return 6;
    a=ImVec2(20,-200);b=ImVec2(20,200);if(!clip(a,b,100,80)||a.y!=0||b.y!=80)return 7;
    return 0;
}
}
