//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2006 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

#ifndef AK_SOUND_ENGINE_DLL_H_
#define AK_SOUND_ENGINE_DLL_H_

#include <AK/SoundEngine/Common/AkSoundEngine.h>
#include <AK/MusicEngine/Common/AkMusicEngine.h>
#include <AK/SoundEngine/Common/AkModule.h>
#include <AK/SoundEngine/Common/AkStreamMgrModule.h>

#if defined(__ANDROID__) || defined(AK_LINUX)
#define MONOCHAR char
#define CONVERT_MONOCHAR_TO_OSCHAR(__SRC, __DST) CONVERT_CHAR_TO_OSCHAR(__SRC, __DST)
#define CONVERT_MONOCHAR_TO_CHAR(__SRC, __DST) __DST = const_cast<char*>(__SRC)
#else
#define MONOCHAR wchar_t
#define CONVERT_MONOCHAR_TO_OSCHAR(__SRC, __DST) CONVERT_WIDE_TO_OSCHAR(__SRC, __DST)
#define CONVERT_MONOCHAR_TO_CHAR(__SRC, __DST) __DST = (char*)AkAlloca( (1 + wcslen( __SRC )) * sizeof(char)); AKPLATFORM::AkWideCharToChar(__SRC, wcslen( __SRC ) + 1, __DST)
#endif

namespace AK
{
    namespace AkSoundEngine
    {
		extern "C"
		{
			AKRESULT Init( 
				AkMemSettings *     in_pMemSettings,
				AkStreamMgrSettings *  in_pStmSettings,
				AkDeviceSettings *  in_pDefaultDeviceSettings,
				AkInitSettings *    in_pSettings,
				AkPlatformInitSettings * in_pPlatformSettings,
				AkMusicSettings *	in_pMusicSettings,
				AkUInt32		in_preparePoolSizeByte
				);
			void     Term();

			AKRESULT SetObjectPosition( AkGameObjectID in_GameObjectID, 
				AkReal32 PosX, AkReal32 PosY, AkReal32 PosZ, 
				AkReal32 OrientationX, AkReal32 OrientationY, AkReal32 OrientationZ, 
				AkUInt32 in_ulListenerIndex = AK_INVALID_LISTENER_INDEX);

			AKRESULT SetListenerPosition( AkReal32 FrontX, AkReal32 FrontY, AkReal32 FrontZ, 
				AkReal32 TopX, AkReal32 TopY, AkReal32 TopZ, 
				AkReal32 PosX, AkReal32 PosY, AkReal32 PosZ, 
				AkUInt32 in_ulListenerIndex);

			bool IsInitialized();

		}
    }
}

#endif //AK_SOUND_ENGINE_DLL_H_
