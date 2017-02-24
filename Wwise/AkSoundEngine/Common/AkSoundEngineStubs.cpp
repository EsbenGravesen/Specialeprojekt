//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include <AK/SoundEngine/Common/AkTypes.h>

#if defined(AK_METRO) || defined(AK_WINPHONE) || (defined(AK_USE_METRO_API) && !defined(AK_XBOXONE))
	#define AK_IMPLEMENT_THREAD_EMULATION
	#include <AK/Tools/Win32/ThreadEmulation.h>
#endif // #ifdef AK_METRO

#define GCC_HASCLASSVISIBILITY
#include "AkSoundEngineStubs.h"
#include <AK/Tools/Common/AkAssert.h>
#if defined (__APPLE__)
	#include <malloc/malloc.h>
	#include <sys/mman.h>
	#include <AK/Tools/Mac/AkPlatformFuncs.h>
#else
	#if ! defined (__PPU__) && ! defined (__SPU__)
		#include <stdlib.h>
	#else
		#include <ppu/include/stdlib.h>
	#endif // #if ! defined (__PPU__) && ! defined (__SPU__)
#endif // #if defined (__APPLE__)

#include <AK/SoundEngine/Common/AkMemoryMgr.h>
#include <AK/SoundEngine/Common/IAkStreamMgr.h>
#include "AkFilePackageLowLevelIOBlocking.h"
CAkFilePackageLowLevelIOBlocking g_lowLevelIO;
AkMemPoolId g_PrepareEventPoolId;

#ifndef AK_OPTIMIZED
#include <AK/Comm/AkCommunication.h>
#endif // #ifndef AK_OPTIMIZED

#include <string>

#ifdef AK_ANDROID
#include "../Android/jni/AkUnityAndroidIO.h"
#endif // #ifdef AK_ANDROID

#include "AkCallbackSerializer.h"

#ifdef AK_XBOXONE
#include <xaudio2.h>
#endif

#if  defined AK_ANDROID || defined AK_LINUX || defined AK_MAC_OS_X
#include <dlfcn.h>
#endif

#ifdef AK_WIIU
FSClient g_hFSClient;
#endif


// Register plugins that are static linked in this DLL.  Others will be loaded dynamically.
#include <AK/Plugin/AkCompressorFXFactory.h>					// Compressor
#include <AK/Plugin/AkDelayFXFactory.h>							// Delay
#include <AK/Plugin/AkMatrixReverbFXFactory.h>					// Matrix reverb
#include <AK/Plugin/AkMeterFXFactory.h>							// Meter
#include <AK/Plugin/AkExpanderFXFactory.h>						// Expander
#include <AK/Plugin/AkParametricEQFXFactory.h>					// Parametric equalizer
#include <AK/Plugin/AkGainFXFactory.h>							// Gain
#include <AK/Plugin/AkPeakLimiterFXFactory.h>					// Peak limiter
#include <AK/Plugin/AkRoomVerbFXFactory.h>						// RoomVerb
#include <AK/Plugin/AkSilenceSourceFactory.h>					// Silence generator
#include <AK/Plugin/AkSineSourceFactory.h>						// Sine wave generator
#include <AK/Plugin/AkToneSourceFactory.h>						// Tone generator

// Required by codecs plug-ins
#include <AK/Plugin/AkVorbisDecoderFactory.h>
#ifdef AK_XBOX360
#include <AK/Plugin/AkXWMADecoderFactory.h>		// Note: Useable only on Xbox 360. Ok to include it on other platforms as long as it is not referenced.
#endif
#if defined AK_APPLE
#include <AK/Plugin/AkAACFactory.h>			
#endif
#if defined AK_VITA || defined AK_PS4
#include <AK/Plugin/AkATRAC9Factory.h>		// Note: Useable only on Vita. Ok to include it on other platforms as long as it is not referenced.
#endif

#include "AkSoundEngineStubs.h"
#include <AK/Tools/Common/AkMonitorError.h>

#include <AK/AkWwiseSDKVersion.h>

// Defines.

// Default memory manager settings.
#define COMM_POOL_SIZE			(256 * 1024)
#define COMM_POOL_BLOCK_SIZE	(48)

#if defined (WIN32) || defined (WIN64) || defined (_XBOX_VER)
	#define AK_PLATFORM_PATH_SEPARATOR '\\'
	#define AK_STRING_PATH_SEPERATOR AKTEXT("\\")
#else
	#define AK_PLATFORM_PATH_SEPARATOR '/'
	#define AK_STRING_PATH_SEPERATOR AKTEXT("/")
#endif // #if defined (WIN32) || defined (WIN64) || defined (_XBOX_VER)

#define AK_UNITY_DEFAULT_POOL_SIZE	(64)

#ifndef AK_OPTIMIZED
#ifndef AK_METRO
char g_GameName[AK_COMM_SETTINGS_MAX_STRING_SIZE];
#endif
#endif

AkOSChar* g_decodedBankPath = NULL;


namespace AK
{
	void * AllocHook( size_t in_size )
	{
		return malloc( in_size );
	}
	void FreeHook( void * in_ptr )
	{
		free( in_ptr );
	}
#if defined (WIN32) || _XBOX_VER >= 200
	void * VirtualAllocHook(
		void * in_pMemAddress,
		size_t in_size,
		DWORD in_dwAllocationType,
		DWORD in_dwProtect
		)
	{
#if defined(AK_METRO) || defined(AK_WINPHONE)
		return malloc( in_size );
#else
		return VirtualAlloc( in_pMemAddress, in_size, in_dwAllocationType, in_dwProtect );
#endif // #if defined(AK_METRO)

	}
	void VirtualFreeHook( 
		void * in_pMemAddress,
		size_t in_size,
		DWORD in_dwFreeType
		)
	{
#if defined(AK_METRO) || defined(AK_WINPHONE)
		free( in_pMemAddress );
#else
		VirtualFree( in_pMemAddress, in_size, in_dwFreeType );
#endif // #if defined(AK_METRO)
	}
#endif // #if defined (WIN32) || _XBOX_VER >= 200

#ifdef AK_XBOXONE
	void * APUAllocHook( 
		size_t in_size,				///< Number of bytes to allocate.
		unsigned int in_alignment	///< Alignment in bytes (must be power of two, greater than or equal to four).
		)
	{
		void * pReturn = nullptr;
		ApuAlloc( &pReturn, NULL, (UINT32) in_size, in_alignment );
		return pReturn;
	}

	void APUFreeHook( 
		void * in_pMemAddress	///< Virtual address as returned by APUAllocHook.
		)
	{
		ApuFree( in_pMemAddress );
	}
#endif

#ifdef AK_WIIU
	void * AllocMEM2Hook( size_t in_size )
	{
		return MEMAllocFromDefaultHeap( in_size );
	}

	void FreeMEM2Hook( void * in_ptr )
	{
		MEMFreeToDefaultHeap( in_ptr );
	}	
#endif
}

#ifdef AK_WIIU
extern "C"
int	rpl_entry(void * aHandle, int aReason)
{
	return 0;
}
#endif

namespace 
{
	// If in Unity Editor, user didn't append path-separator at the end of the 
	// soundbank folder path, add it automatically before concatenate it with 
	// soundbank filename.
	void SafeAppendTrailingPathSeparator(AkOSChar* io_folderPath)
	{
		size_t pathLen = AKPLATFORM::OsStrLen(io_folderPath);
		bool isZeroLenPath = ( pathLen == 0 || io_folderPath == NULL);
		if (isZeroLenPath) 
		{
			return;
		}

		AkOSChar* cstringEnd = io_folderPath + pathLen;
		AkOSChar trailingChar = *(cstringEnd-1);
		bool hasTrailingSeparator = (trailingChar == AK_PLATFORM_PATH_SEPARATOR);
		if (hasTrailingSeparator) 
		{
			return;
		}

		// We have a folder path without trailing separator
		// Here we assume input and output path buffer size is big enough
		*cstringEnd = AK_PLATFORM_PATH_SEPARATOR;

		bool isAnsiPath = ( sizeof(AkOSChar) == 1 );
		const AkOSChar CStringEndChar = isAnsiPath ? '\0' : L'0';
		*(cstringEnd+1) = CStringEndChar;

	}
}

void AkUnityAssertHook(
	const char * in_pszExpression,	///< Expression
	const char * in_pszFileName,	///< File Name
	int in_lineNumber				///< Line Number
	)
{
	size_t msgLen = strlen(in_pszExpression) + strlen(in_pszFileName) + 128;
	char* msg = (char*)AkAlloca(msgLen);
#if defined(AK_PS3) || defined(AK_ANDROID) || defined(AK_MAC_OS_X) || defined(AK_IOS) || defined(AK_LINUX) || defined(AK_WIIU)
	sprintf(msg, "AKASSERT: %s. File: %s, line: %d", in_pszExpression, in_pszFileName, in_lineNumber);
#else
	sprintf_s(msg, msgLen, "AKASSERT: %s. File: %s, line: %d", in_pszExpression, in_pszFileName, in_lineNumber);
#endif
	AKPLATFORM::OutputDebugMsg(msg);
}

// Prototype declaration
AKRESULT SaveDecodedBank(AkOSChar * in_OrigBankFile, void * in_pDecodedData, AkUInt32 in_decodedSize, bool in_bIsLanguageSpecific);
void FindDllPath(AkInitSettings *in_pSettings, AkOSChar* out_szTemp);


AKRESULT CreateDirectoryStructure(const AkOSChar* in_pszDirectory)
{
#if defined AK_WII || defined AK_3DS
	//Platforms that don't have storage
	return AK_Success;
#elif defined AK_PS3
	//PS3 can create the whole path in one call.
	CellFsErrno errNo;

	errNo = ::cellFsMkdir(in_pszDirectory, CELL_FS_S_IRWXU | CELL_FS_S_IRWXG | CELL_FS_S_IRWXO);
	if (errNo != CELL_FS_ERROR_EEXIST && errNo != CELL_FS_OK)
		return AK_Fail;
	return AK_Success;
#elif defined AK_WIIU
	//WiiU can create the whole path in one call.
	FSCmdBlock fsBlock;
	FSInitCmdBlock(&fsBlock);

	FSStatus err = FSMakeDir(&g_hFSClient, &fsBlock, in_pszDirectory, FS_RET_ALL_ERROR);
	if (err == FS_STATUS_OK || err == FS_STATUS_EXISTS)
		return AK_Success;
#else
	//All other platforms can only create the last part of the path, given the rest exist.  
	//Loop through all parts.
	size_t len = AKPLATFORM::OsStrLen(in_pszDirectory) + 1;
	AkOSChar* szPartial = (AkOSChar*)AkAlloca(len * sizeof(AkOSChar));
	memcpy(szPartial, in_pszDirectory, len * sizeof(AkOSChar));
	AkOSChar* pTerminator = szPartial;
	AkOSChar* pEnd = szPartial + len - 1;

	//Skip root.
	while (!(*pTerminator == AK_PATH_SEPARATOR[0] || *(pTerminator) == ':') && pTerminator < pEnd)
		pTerminator++;

	while ((*pTerminator == AK_PATH_SEPARATOR[0] || *(pTerminator) == ':') && pTerminator < pEnd)
		pTerminator++;

	while (pTerminator < pEnd)
	{
		//Advance to the next path separator.
		while (*pTerminator != AK_PATH_SEPARATOR[0] && pTerminator < pEnd)
			pTerminator++;

		*pTerminator = 0;

#if defined AK_WIN || defined AK_XBOX360 || defined AK_XBOXONE
		bool bSuccess = ::CreateDirectory(szPartial, NULL) == TRUE;
		if (!bSuccess && ::GetLastError() != ERROR_ALREADY_EXISTS)
			return AK_Fail;
#elif defined AK_PS4 || defined AK_VITA
		int errNo = ::sceFiosDirectoryCreateSync(NULL, szPartial);
		if (errNo != SCE_FIOS_OK && errNo != SCE_FIOS_ERROR_ALREADY_EXISTS)
			return AK_Fail;
#elif defined AK_POSIX
		int iErr = mkdir(szPartial, S_IRWXU | S_IRWXG | S_IRWXO);
		if (iErr == -1 && errno != EEXIST)
			return AK_Fail;
#else
#error Unsupported platform in CreateDirectoryStructure!
#endif
		*pTerminator = AK_PATH_SEPARATOR[0];
		pTerminator++;
	}
#endif

	return AK_Success;
}


//-----------------------------------------------------------------------------------------
// Sound Engine initialization.
//-----------------------------------------------------------------------------------------
extern "C" 
{
	AKRESULT Init( 
		AkMemSettings *     in_pMemSettings,
		AkStreamMgrSettings * in_pStmSettings,
		AkDeviceSettings *  in_pDefaultDeviceSettings,
		AkInitSettings *    in_pSettings,
		AkPlatformInitSettings * in_pPlatformSettings,
		AkMusicSettings *	in_pMusicSettings,
		AkUInt32		in_preparePoolSizeByte
		)
	{
		// Check required arguments.
		if ( !in_pMemSettings ||
			 !in_pStmSettings ||
			 !in_pDefaultDeviceSettings )
		{
			AKASSERT( !"Invalid arguments" );
			return AK_InvalidParameter;
		}

		in_pSettings->pfnAssertHook = AkUnityAssertHook;

		// Create and initialise an instance of our memory manager.
		if ( AK::MemoryMgr::Init( in_pMemSettings ) != AK_Success )
		{
			AKASSERT( !"Could not create the memory manager." );
			return AK_Fail;
		}

		// Create and initialise an instance of the default stream manager.
		if ( !AK::StreamMgr::Create( *in_pStmSettings ) )
		{
			AKASSERT( !"Could not create the Stream Manager" );
			return AK_Fail;
		}

		// Create an IO device.
#ifdef AK_ANDROID
		in_pPlatformSettings->pJavaVM = java_vm;
		if (InitAndroidIO(in_pPlatformSettings->jNativeActivity) != AK_Success)
			return AK_Fail;
#endif 

		if ( g_lowLevelIO.Init( *in_pDefaultDeviceSettings ) != AK_Success )
		{
			AKASSERT( !"Cannot create streaming I/O device" );
			return AK_Fail;
		}

#ifdef AK_IOS
		in_pPlatformSettings->audioCallbacks.interruptionCallback = AkCallbackSerializer::AudioInterruptionCallbackFunc;
#endif // #ifdef AK_IOS
		in_pSettings->BGMCallback = AkCallbackSerializer::AudioSourceChangeCallbackFunc;		

		AkOSChar szDLLPath[AK_MAX_PATH * 2] = { 0 };
		AkOSChar* pOld = in_pSettings->szPluginDLLPath;
		FindDllPath(in_pSettings, szDLLPath);				

		// Add a memory pool for PrepareBank
		if (in_preparePoolSizeByte > 0)
		{
			g_PrepareEventPoolId = AK::MemoryMgr::CreatePool(NULL, in_preparePoolSizeByte, AK_UNITY_DEFAULT_POOL_SIZE, AkMalloc);
			AK::MemoryMgr::SetPoolName(g_PrepareEventPoolId, AKTEXT("PreparePool"));
			in_pSettings->uPrepareEventMemoryPoolID = g_PrepareEventPoolId;
		}
		else
		{
			g_PrepareEventPoolId = AK_INVALID_POOL_ID;
		}

		// Initialize sound engine.
		AKRESULT res = AK::SoundEngine::Init(in_pSettings, in_pPlatformSettings);
		in_pSettings->szPluginDLLPath = pOld;
		if ( res != AK_Success )
		{
			AKASSERT( !"Cannot initialize sound engine" );
			return AK_Fail;
		}
		
		// Initialize music engine.
		if ( AK::MusicEngine::Init( in_pMusicSettings ) != AK_Success )
		{
			AKASSERT( !"Cannot initialize music engine" );
			return AK_Fail;
		}

#ifndef AK_OPTIMIZED
	#ifdef AK_XBOXONE
		try
		{
			// Make sure networkmanifest.xml is loaded by instantiating a Microsoft.Xbox.Networking object.
			auto secureDeviceAssociationTemplate = Windows::Xbox::Networking::SecureDeviceAssociationTemplate::GetTemplateByName( "WwiseDiscovery" );
		}
		catch( Platform::Exception ^e )
		{
			AKPLATFORM::OutputDebugMsg( "Wwise network sockets not found in manifest file. Profiling will be impossible." );
		}
	#endif

	#ifndef AK_METRO
		// Initialize communication.
		AkCommSettings settingsComm;
		AK::Comm::GetDefaultInitSettings( settingsComm );
		AKPLATFORM::SafeStrCpy(settingsComm.szAppNetworkName, g_GameName, AK_COMM_SETTINGS_MAX_STRING_SIZE);
		if ( AK::Comm::Init( settingsComm ) != AK_Success )
		{
			AKASSERT( !"Cannot initialize communication" );
		}
	#endif // #ifndef AK_METRO
#endif // AK_OPTIMIZED

#ifdef AK_WIIU
		FSInit();
		if(FSAddClient(&g_hFSClient, FS_RET_ALL_ERROR) != FS_STATUS_OK)
		{
			OSReport("FSAddClient failed, no file system. Saving decoded banks will not work.\n");
		}

#endif
				
		return AK_Success;
	}

	//-----------------------------------------------------------------------------------------
	// Sound Engine termination.
	//-----------------------------------------------------------------------------------------
	void Term( )
	{
		if (!AK::SoundEngine::IsInitialized())
			return;
		
		AK::SoundEngine::StopAll();

#ifndef AK_OPTIMIZED
	#ifndef AK_METRO
		AK::Comm::Term();
	#endif // #ifndef AK_METRO
#endif // AK_OPTIMIZED

		AK::MusicEngine::Term();

		AK::SoundEngine::Term();

		if (g_PrepareEventPoolId != AK_INVALID_POOL_ID)
		{
			AK::MemoryMgr::DestroyPool(g_PrepareEventPoolId);
		}

		g_lowLevelIO.Term();
		if ( AK::IAkStreamMgr::Get() )
			AK::IAkStreamMgr::Get()->Destroy();
			
		AK::MemoryMgr::Term();
	}

	void GetDefaultStreamSettings(AkStreamMgrSettings & out_settings)
	{
		AK::StreamMgr::GetDefaultSettings(out_settings);
	}

	void GetDefaultDeviceSettings(AkDeviceSettings & out_settings)
	{
		AK::StreamMgr::GetDefaultDeviceSettings(out_settings);
	}

	void GetDefaultMusicSettings(AkMusicSettings &out_settings)
	{
		AK::MusicEngine::GetDefaultInitSettings(out_settings);
	}

	void GetDefaultInitSettings(AkInitSettings & out_settings)
	{
		AK::SoundEngine::GetDefaultInitSettings(out_settings);
	}

	void GetDefaultPlatformInitSettings(AkPlatformInitSettings &out_settings)
	{
		AK::SoundEngine::GetDefaultPlatformInitSettings(out_settings);
	}

	AkUInt32 GetMajorMinorVersion()
	{
		return (AK_WWISESDK_VERSION_MAJOR << 16) | AK_WWISESDK_VERSION_MINOR;
	}

	AkUInt32 GetSubminorBuildVersion()
	{
		return (AK_WWISESDK_VERSION_SUBMINOR << 16) | AK_WWISESDK_VERSION_BUILD;
	}

	void SetGameName(const MONOCHAR* in_GameName)
	{
#ifndef AK_OPTIMIZED
#ifndef AK_METRO
		AkOSChar* GameNameOsString = NULL;
		char * CharName;
		CONVERT_MONOCHAR_TO_OSCHAR( in_GameName, GameNameOsString );
		CONVERT_OSCHAR_TO_CHAR(GameNameOsString, CharName);
		AKPLATFORM::SafeStrCpy(g_GameName, CharName, AK_COMM_SETTINGS_MAX_STRING_SIZE);
#endif
#else
		return;
#endif
	}
//-----------------------------------------------------------------------------------------
// Access to LowLevelIO's file localization.
//-----------------------------------------------------------------------------------------
	// File system interface.
	AKRESULT SetBasePath(const MONOCHAR* in_pszBasePath)
	{
		AkOSChar* basePathOsString = NULL;
		CONVERT_MONOCHAR_TO_OSCHAR( in_pszBasePath, basePathOsString );
		return g_lowLevelIO.SetBasePath( basePathOsString );
	}

	AKRESULT AddBasePath( MONOCHAR* in_pszBasePath	)
	{
#if defined __ANDROID__	|| defined AK_IOS
		AkOSChar* basePathOsString = NULL;
		CONVERT_MONOCHAR_TO_OSCHAR( in_pszBasePath, basePathOsString );
		return g_lowLevelIO.AddBasePath(basePathOsString);
#else
		return AK_NotImplemented;
#endif
	}
	
	void SetDecodedBankPath(const MONOCHAR* in_DecodedPath)
	{
		CONVERT_MONOCHAR_TO_OSCHAR(in_DecodedPath, g_decodedBankPath);
		CreateDirectoryStructure(g_decodedBankPath);
	}

	AKRESULT SetCurrentLanguage( const MONOCHAR*	in_pszLanguageName)
	{
		AkOSChar* languageOsString = NULL;
		CONVERT_MONOCHAR_TO_OSCHAR( in_pszLanguageName, languageOsString );	
		if (g_decodedBankPath != NULL)
		{
			AkOSChar szDecodedPathCopy[AK_MAX_PATH];
			AKPLATFORM::SafeStrCpy(szDecodedPathCopy, g_decodedBankPath, AK_MAX_PATH);
			AKPLATFORM::SafeStrCat(szDecodedPathCopy, &AK_PATH_SEPARATOR[0], AK_MAX_PATH);
			AKPLATFORM::SafeStrCat(szDecodedPathCopy, languageOsString, AK_MAX_PATH);
			CreateDirectoryStructure(szDecodedPathCopy);
		}
		return AK::StreamMgr::SetCurrentLanguage( languageOsString );
	}

	AKRESULT LoadFilePackage(
		const MONOCHAR* in_pszFilePackageName,	// File package name. Location is resolved using base class' Open().
		AkUInt32 &		out_uPackageID,			// Returned package ID.
		AkMemPoolId		in_memPoolID)	// Memory pool in which the LUT is written. Passing AK_DEFAULT_POOL_ID will create a new pool automatically. 		
	{
		AkOSChar* osString = NULL;
		CONVERT_MONOCHAR_TO_OSCHAR( in_pszFilePackageName, osString );
		return g_lowLevelIO.LoadFilePackage(osString, out_uPackageID, in_memPoolID);
	}

	// Unload a file package.
	// Returns AK_Success if in_uPackageID exists, AK_Fail otherwise.
	// WARNING: This method is not thread safe. Ensure there are no I/O occurring on this device
	// when unloading a file package.
	AKRESULT UnloadFilePackage(AkUInt32 in_uPackageID)
	{
		return g_lowLevelIO.UnloadFilePackage(in_uPackageID);
	}

	// Unload all file packages.
	// Returns AK_Success;
	// WARNING: This method is not thread safe. Ensure there are no I/O occurring on this device
	// when unloading a file package.
	AKRESULT UnloadAllFilePackages()
	{
		return g_lowLevelIO.UnloadAllFilePackages();
	}

	AKRESULT RegisterGameObjInternal( int in_GameObj ) 
	{
		if (AK::SoundEngine::IsInitialized())
			return AK::SoundEngine::RegisterGameObj((AkGameObjectID)in_GameObj);
		return AK_Fail;
	}

	AKRESULT RegisterGameObjInternal_WithMask( int in_GameObj, AkUInt32 in_ulListenerMask ) 
	{
		if (AK::SoundEngine::IsInitialized())
			return AK::SoundEngine::RegisterGameObj((AkGameObjectID)in_GameObj, in_ulListenerMask);
		return AK_Fail;
	}

	AKRESULT RegisterGameObjInternal_WithName( int in_GameObj, const MONOCHAR* in_pszObjName ) 
	{
		if (AK::SoundEngine::IsInitialized())
		{
			char* szName;
			CONVERT_MONOCHAR_TO_CHAR(in_pszObjName, szName);
			return AK::SoundEngine::RegisterGameObj((AkGameObjectID)in_GameObj, szName);
		}
		return AK_Fail;
	}

	AKRESULT RegisterGameObjInternal_WithName_WithMask( int in_GameObj, const MONOCHAR* in_pszObjName, AkUInt32 in_ulListenerMask ) 
	{
		if (AK::SoundEngine::IsInitialized())
		{
			char* szName;
			CONVERT_MONOCHAR_TO_CHAR(in_pszObjName, szName);
			return AK::SoundEngine::RegisterGameObj((AkGameObjectID)in_GameObj, szName, in_ulListenerMask);
		}
		return AK_Fail;
	}

	AKRESULT UnregisterGameObjInternal( int in_GameObj ) 
	{
		if (AK::SoundEngine::IsInitialized())
			return AK::SoundEngine::UnregisterGameObj((AkGameObjectID)in_GameObj);
		return AK_Fail;
	}

	AKRESULT LoadAndDecodeInternal(void* in_BankData, AkUInt32 in_BankDataSize, bool in_bSaveDecodedBank, AkOSChar* in_szDecodedBankName, bool in_bIsLanguageSpecific, AkBankID& out_bankID)
	{
		AKRESULT eResult = AK_Fail;

		// Get the decoded size
		AkUInt32 decodedSize = 0;
		void * pDecodedData = NULL;
		eResult = AK::SoundEngine::DecodeBank(in_BankData, in_BankDataSize, AK_DEFAULT_POOL_ID, pDecodedData, decodedSize); // get the decoded size
		if( eResult == AK_Success )
		{
			pDecodedData = malloc(decodedSize);

			// Actually decode the bank
			if( pDecodedData != NULL )
			{
				eResult = AK::SoundEngine::DecodeBank(in_BankData, in_BankDataSize, AK_DEFAULT_POOL_ID, pDecodedData, decodedSize); 
				if( eResult == AK_Success )
				{
					// TODO: We could use the async load bank to Load and Save at the same time.

					// 3- Load the bank from the decoded memory pointer.
					eResult = AK::SoundEngine::LoadBank(pDecodedData, decodedSize, AK_DEFAULT_POOL_ID, out_bankID);

					// 4- Save the decoded bank to disk. 
					if (in_bSaveDecodedBank)
					{
						AKRESULT eSaveResult = SaveDecodedBank(in_szDecodedBankName, pDecodedData, decodedSize, in_bIsLanguageSpecific);
						if (eSaveResult != AK_Success)
						{
							eResult = eSaveResult;
							AK::Monitor::PostString("Could not save the decoded bank !", AK::Monitor::ErrorLevel_Error);
						}
					}
				}

				free(pDecodedData);
			}
			else
			{
				eResult = AK_InsufficientMemory;
			}
		}

		return eResult;
	}
	
	void SanitizeBankNameWithoutExtension(MONOCHAR* in_BankName, AkOSChar* out_SanitizedString)
	{
		AkOSChar* tempStr = NULL;
		CONVERT_MONOCHAR_TO_OSCHAR(in_BankName, tempStr);
		AKPLATFORM::SafeStrCpy(out_SanitizedString, tempStr, AK_MAX_PATH - 1);
		out_SanitizedString[AK_MAX_PATH - 1] = '\0';
	}

	void SanitizeBankName(MONOCHAR* in_BankName, AkOSChar * out_SanitizedString)
	{
		const AkOSChar* ext = AKTEXT(".bnk");
		SanitizeBankNameWithoutExtension(in_BankName, out_SanitizedString);
		AKPLATFORM::SafeStrCat(out_SanitizedString, ext, AK_MAX_PATH - 1);
	}

	AKRESULT LoadAndDecodeBankFromMemory( void* in_BankData, AkUInt32 in_BankDataSize, bool in_bSaveDecodedBank, MONOCHAR* in_DecodedBankName, bool in_bIsLanguageSpecific, AkBankID& out_bankID )
	{
		AkOSChar osString[AK_MAX_PATH];
		SanitizeBankName(in_DecodedBankName, osString);
		return LoadAndDecodeInternal(in_BankData, in_BankDataSize, in_bSaveDecodedBank, osString, in_bIsLanguageSpecific, out_bankID);
	}

	AKRESULT LoadAndDecodeBank( MONOCHAR* in_pszString, bool in_bSaveDecodedBank, AkBankID& out_bankID )
	{
		AKRESULT eResult = AK_Fail;
		AkOSChar osString[AK_MAX_PATH];
		SanitizeBankName(in_pszString, osString);

		if( in_bSaveDecodedBank )
		{
			// 1- Open and read the file ourselves using the StreamMgr
			AkFileSystemFlags flags;
			AK::IAkStdStream *	pStream;
			flags.uCompanyID = AKCOMPANYID_AUDIOKINETIC;
			flags.uCodecID = AKCODECID_BANK;
			flags.uCustomParamSize = 0;
			flags.pCustomParam = NULL;
			flags.bIsLanguageSpecific = true;
			flags.bIsFromRSX = false;

			eResult = AK::IAkStreamMgr::Get()->CreateStd(
				osString,
				&flags,
				AK_OpenModeRead,
				pStream,
				true );
			
			if( eResult != AK_Success ) // TODO: Verify in the file location resolver
			{
				flags.bIsLanguageSpecific = false; 
				eResult = AK::IAkStreamMgr::Get()->CreateStd(
					osString,
					&flags,
					AK_OpenModeRead,
					pStream,
					true );
			}

			if( eResult == AK_Success )
			{
				AkStreamInfo info;
				pStream->GetInfo( info );
				AkUInt8 * pFileData = (AkUInt8*)malloc((size_t)info.uSize);
				if( pFileData != NULL )
				{
					AkUInt32 outSize;
					eResult = pStream->Read(pFileData, (AkUInt32)info.uSize, true, AK_DEFAULT_PRIORITY, info.uSize/AK_DEFAULT_BANK_THROUGHPUT, outSize);
					if( eResult == AK_Success )
					{
						pStream->Destroy();
						pStream = NULL;

						eResult = LoadAndDecodeInternal(pFileData, outSize, in_bSaveDecodedBank, osString, flags.bIsLanguageSpecific, out_bankID);
					}

					free(pFileData);
					pFileData = NULL;
				}
				else
				{
					eResult = AK_InsufficientMemory;
				}
			}
		}
		else
		{
			// Use the Prepare Bank mechanism. This will decode on load, but not save anything
			eResult = AK::SoundEngine::PrepareBank(AK::SoundEngine::Preparation_LoadAndDecode, osString);

			AkOSChar osBankName[AK_MAX_PATH];
			SanitizeBankNameWithoutExtension(in_pszString, osBankName);
			out_bankID = AK::SoundEngine::GetIDFromString(osBankName);
		}

		return eResult;
	}
}




AKRESULT SaveDecodedBank(AkOSChar * in_OrigBankFile, void * in_pDecodedData, AkUInt32 in_decodedSize, bool in_bIsLanguageSpecific)
{
	AkOSChar szDecodedBankPath[AK_MAX_PATH];
	const AkOSChar* tmpStr = AKTEXT("DecodedBanks") AK_STRING_PATH_SEPERATOR;
	AKPLATFORM::SafeStrCpy(szDecodedBankPath, tmpStr, AK_MAX_PATH - 1);
	AKPLATFORM::SafeStrCat(szDecodedBankPath, in_OrigBankFile, AK_MAX_PATH - 1 );
	szDecodedBankPath[AK_MAX_PATH - 1] = '\0';

	AkFileSystemFlags flags;
	AK::IAkStdStream *	pStream;
	flags.uCompanyID = AKCOMPANYID_AUDIOKINETIC;
	flags.uCodecID = AKCODECID_BANK;
	flags.uCustomParamSize = 0;
	flags.pCustomParam = NULL;
	flags.bIsLanguageSpecific = in_bIsLanguageSpecific;
	flags.bIsFromRSX = false;

	AKRESULT eResult = AK::IAkStreamMgr::Get()->CreateStd(
		szDecodedBankPath,
		&flags,
		AK_OpenModeWrite,
		pStream,
		true );

	if( eResult == AK_Success )
	{
		AkUInt32 outSize = 0;
		eResult = pStream->Write(in_pDecodedData, in_decodedSize, true, AK_DEFAULT_PRIORITY, in_decodedSize/AK_DEFAULT_BANK_THROUGHPUT, outSize);

		pStream->Destroy();
		pStream = NULL;
	}

	return eResult;
}


//Find the path of *this* DLL so we load the others from the same directory.
#if defined AK_WIN && !defined AK_USE_METRO_API
void FindDllPath(AkInitSettings * in_pSettings, AkOSChar* out_szTemp)
{		
	HMODULE hLib = ::GetModuleHandle(L"AkSoundEngine.dll");	//Find where this dll is.  Plugins will be at the same place.	
	::GetModuleFileName(hLib, out_szTemp, AK_MAX_PATH * 2 - 1);

	if (wcsstr(out_szTemp, L"Deployment") != NULL)
	{
		// Play in Editor, point to the DSP path
		*(wcsrchr(out_szTemp, AK_PATH_SEPARATOR[0])) = 0;
		*(wcsrchr(out_szTemp, AK_PATH_SEPARATOR[0]) + 1) = 0;
		wcscat(out_szTemp, L"DSP\\");
	}
	else
	{
		//Built game.  All binaries are in the same place.
		*(wcsrchr(out_szTemp, AK_PATH_SEPARATOR[0]) + 1) = 0;
	}    
	in_pSettings->szPluginDLLPath = out_szTemp;
}
#elif defined AK_MAC_OS_X
void FindDllPath(AkInitSettings * in_pSettings, AkOSChar* out_szTemp)
{
    Dl_info info;
    dladdr((const void*)Init, &info);
	strcpy(out_szTemp, info.dli_fname);
    
	char * pDeploy = strstr(out_szTemp, "Deployment");
    if (pDeploy != NULL)
    {
        // Play in Editor, point to the DSP path
        pDeploy += strlen("Deployment/Plugins/Mac/");
        *pDeploy = 0;
        // Play in Editor, point to the DSP path
		strcat(out_szTemp, "DSP/");
    }
    else
    {
        //Built game.  All binaries are in the same place.
		pDeploy = strstr(out_szTemp, "AkSoundEngine.bundle");
        *pDeploy = 0;
    }    
	in_pSettings->szPluginDLLPath = out_szTemp;
}
/*#elif defined AK_WIN_UNIVERSAL_APP
void FindDllPath()
{
	HMODULE hLib = ::GetModuleHandle(L"AkSoundEngine.dll");	//Find where this dll is.  Plugins will be at the same place.	
	::GetModuleFileName(hLib, g_szDLLPath, AK_MAX_PATH*2-1);

	*(wcsrchr(g_szDLLPath, AK_PATH_SEPARATOR[0])+1) = 0;
}*/
#elif defined AK_LINUX
void FindDllPath(AkInitSettings * in_pSettings, AkOSChar* out_szTemp)
{
	Dl_info info;
	dladdr((const void*)Init, &info);
	strcpy(out_szTemp, info.dli_fname);
	
	*(strrchr(out_szTemp, AK_PATH_SEPARATOR[0])+1) = 0;    
	in_pSettings->szPluginDLLPath = out_szTemp;
}

#else
void FindDllPath(AkInitSettings * in_pSettings, AkOSChar* out_szTemp)
{    
}
#endif
