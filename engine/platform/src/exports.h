//
// Created by fcors on 12/19/2025.
//
#pragma once

#if defined(_WIN32) || defined(__CYGWIN__)
    #define RETRO_API __declspec(dllexport)
#else
    #if __GNUC__ >= 4
        #define RETRO_API __attribute__((visibility("default")))
    #else
        #define RETRO_API
    #endif
#endif