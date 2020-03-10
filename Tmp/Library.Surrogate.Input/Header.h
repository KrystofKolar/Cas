#pragma once

#ifdef LIBRARY_SURROGATE_EXPORT
#define LIBRARY_SURROGATE_DIR __declspec(dllexport)
#else
#define LIBRARY_SURROGATE_DIR __declspec(dllimport)
#endif

extern "C" LIBRARY_SURROGATE_DIR int SuperPrint();
