// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。

#include "pch.h"

BOOL APIENTRY DllMain(HMODULE,
						const DWORD  ulReasonForCall,
						LPVOID
)
{
	switch (ulReasonForCall)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
	default:
		break;
	}
	return TRUE;
}