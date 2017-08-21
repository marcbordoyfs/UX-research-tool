#pragma once

#include <memory>
#include <map>
#include <unordered_map>
#include <string>

#ifdef _WIN32
#include <Windows.h>

#ifdef AFFDEXSDK_EXPORTS
#define AFFDEXSDK __declspec(dllexport)
#else //AFFDEXSDK_EXPORTS
#define AFFDEXSDK __declspec(dllimport)
#endif //AFFDEXSDK_EXPORTS

#else  //_WIN32

typedef unsigned char byte;
#define AFFDEXSDK

#endif //_Win32

#ifdef __GNUC__
#define DEPRECATED __attribute__((deprecated))
#elif defined(_MSC_VER)
#define DEPRECATED __declspec(deprecated)
#else
#pragma message("WARNING: You need to implement DEPRECATED for this compiler")
#define DEPRECATED
#endif



/// <summary>
/// Affdex SDK unmanaged C++ namespace
/// </summary>
namespace affdex
{
    /*
    * Copyright (c) 2015 - 2016 Affectiva, Inc. All rights reserved.
    */
    /// <summary>
    /// Type for specifying filesystem paths.
    /// </summary>
#ifdef _WIN32
    typedef std::wstring path;
#else //_WIN32
    typedef std::string path;
#endif

    /// <summary>
    /// Face detector configuration
    /// </summary>
    enum class FaceDetectorMode
    {
        /// <summary>
        /// To target faces occupying a large area.
        /// </summary>
        LARGE_FACES,
        /// <summary>
        /// To target faces occupying a small area.
        /// </summary>
        SMALL_FACES
    };

    /// <summary>
    /// Struct array_deleter - for proper deletion of array contents for shared pointer of array
    /// </summary>
    template<  typename T >
    struct array_deleter
    {
        void operator () (T const * p)
        {
            delete[] p;
        }

    };

    /// <summary>
    /// Face identifier
    /// </summary>
    typedef int FaceId;

    /// <summary>
    /// Container for storing
    /// </summary>
    typedef std::unordered_map<std::string, float> MetricsMap;
    typedef std::shared_ptr<byte> ByteBuffer;
    const float DEFAULT_PROCESSING_FRAMERATE = 30;
    const unsigned int DEFAULT_MAX_NUM_FACES = 1;


}
