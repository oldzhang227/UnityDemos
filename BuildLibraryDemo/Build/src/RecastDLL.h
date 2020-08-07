
#ifdef _GNUC_

#include <jni.h>
#define DLL_EXPORT JNIEXPORT

#else
	#ifdef SIMPLEDLL_EXPORT
	#define DLL_EXPORT __declspec(dllexport)
	#else
	#define DLL_EXPORT
	#endif
#endif


extern "C"
{


	DLL_EXPORT int TestAPI();



}





