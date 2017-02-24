#include "stdafx.h"
#include "AkCallbackSerializer.h"
#include <AK/Tools/Common/AkLock.h>
#include <stdio.h>
#include "ExtraCallbacks.h"
#include <AK/Tools/Common/AkAssert.h>

static CAkLock m_Lock;	//Defined here to avoid exposing the CAkLock class through SWIG.

#pragma pack(push, 4)
struct SerializedTypes
{
	typedef AkInt32 MarshalBool;

	struct AkCommonCallback
	{
		void* pPackage; //The C# CallbackPackage to return to C#
		AkCommonCallback* pNext; //Pointer to the next callback
		AkCallbackType eType; //The type of structure following
	};

	struct AkCallbackInfo
	{
		void* pCookie; ///< User data, passed to PostEvent()
		AkGameObjectID gameObjID; ///< Game object ID
	};

	struct AkEventCallbackInfo : AkCallbackInfo
	{
		AkPlayingID		playingID;		///< Playing ID of Event, returned by PostEvent()
		AkUniqueID		eventID;		///< Unique ID of Event, passed to PostEvent()
	};

	struct AkMIDIEventCallbackInfo : AkEventCallbackInfo
	{
		AkMIDIEvent		midiEvent;		///< MIDI event triggered by event.
	};

	struct AkMarkerCallbackInfo : AkEventCallbackInfo
	{
		AkUInt32	uIdentifier;		///< Cue point identifier
		AkUInt32	uPosition;			///< Position in the cue point (unit: sample frames)
		char strLabel[1];			///< Label of the marker, read from the file
	};

	struct AkDurationCallbackInfo : AkEventCallbackInfo
	{
		AkReal32	fDuration;				///< Duration of the sound (unit: milliseconds)
		AkReal32	fEstimatedDuration;		///< Estimated duration of the sound depending on source settings such as pitch. (unit: milliseconds)
		AkUniqueID	audioNodeID;			///< Audio Node ID of playing item
		AkUniqueID  mediaID;				///< Media ID of playing item. (corresponds to 'ID' attribute of 'File' element in SoundBank metadata file)
		MarshalBool bStreaming;				///< True if source is streaming, false otherwise.
	};

	struct AkDynamicSequenceItemCallbackInfo : AkCallbackInfo
	{
		AkPlayingID playingID;			///< Playing ID of Dynamic Sequence, returned by AK::SoundEngine:DynamicSequence::Open()
		AkUniqueID	audioNodeID;		///< Audio Node ID of finished item
		void*		pCustomInfo;		///< Custom info passed to the DynamicSequence::Open function
	};

	struct AkMusicSyncCallbackInfo : AkCallbackInfo
	{
		AkPlayingID playingID;			///< Playing ID of Event, returned by PostEvent()
		//AkSegmentInfo segmentInfo; ///< Segment information corresponding to the segment triggering this callback.

		// AkSegmentInfo expanded to prevent packing issues
		// BEGIN_AKSEGMENTINFO_EXPANSION
		AkTimeMs		segmentInfo_iCurrentPosition;		///< Current position of the segment, relative to the Entry Cue, in milliseconds. Range is [-iPreEntryDuration, iActiveDuration+iPostExitDuration].
		AkTimeMs		segmentInfo_iPreEntryDuration;		///< Duration of the pre-entry region of the segment, in milliseconds.
		AkTimeMs		segmentInfo_iActiveDuration;		///< Duration of the active region of the segment (between the Entry and Exit Cues), in milliseconds.
		AkTimeMs		segmentInfo_iPostExitDuration;		///< Duration of the post-exit region of the segment, in milliseconds.
		AkTimeMs		segmentInfo_iRemainingLookAheadTime;///< Number of milliseconds remaining in the "looking-ahead" state of the segment, when it is silent but streamed tracks are being prefetched.
		AkReal32		segmentInfo_fBeatDuration;			///< Beat Duration in seconds.
		AkReal32		segmentInfo_fBarDuration;			///< Bar Duration in seconds.
		AkReal32		segmentInfo_fGridDuration;			///< Grid duration in seconds.
		AkReal32		segmentInfo_fGridOffset;			///< Grid offset in seconds.
		// END_AKSEGMENTINFO_EXPANSION

		AkCallbackType musicSyncType;	///< Would be either AK_MusicSyncEntry, AK_MusicSyncBeat, AK_MusicSyncBar, AK_MusicSyncExit, AK_MusicSyncGrid, AK_MusicSyncPoint or AK_MusicSyncUserCue.
		char  userCueName[1];
	};

	struct AkBankCallbackInfo
	{
		AkUInt32 bankID;
		AkUIntPtr inMemoryBankPtr;
		AKRESULT loadResult;
		AkMemPoolId memPoolId;
	};

	struct AkMonitoringCallbackInfo
	{
		AK::Monitor::ErrorCode errorCode;
		AK::Monitor::ErrorLevel errorLevel;
		AkPlayingID playingID;
		AkGameObjectID gameObjID;
		char message[1];
	};

#ifdef AK_IOS
	struct AkAudioInterruptionCallbackInfo
	{
		MarshalBool bEnterInterruption;
	};
#endif // #ifdef AK_IOS

	struct AkAudioSourceChangeCallbackInfo
	{
		MarshalBool bOtherAudioPlaying;
	};
};
#pragma pack(pop)

SerializedTypes::AkCommonCallback* m_pLockedPtr = NULL;
SerializedTypes::AkCommonCallback** m_pLastNextPtr = NULL;
SerializedTypes::AkCommonCallback* m_pNextAvailable = NULL;
SerializedTypes::AkCommonCallback* m_pFirst = NULL;
AkEvent m_DrainEvent;
void* m_pBlockStart = NULL;
void* m_pBlockEnd = NULL;
AkThreadID m_idThread = 0;


SerializedTypes::AkCommonCallback* AllocNewCall(AkUInt32 uItemSize, bool in_bCritical)
{
retry:
	//If the current thread is the main thread that normally pumps the messages, then don't block if the buffer is full!
	in_bCritical = in_bCritical && m_idThread != AKPLATFORM::CurrentThread();

	m_Lock.Lock();
	void* pItemEnd = (char*)m_pNextAvailable + uItemSize;
	void* pReadPtr = m_pLockedPtr != NULL ? m_pLockedPtr : m_pFirst;

	if (m_pBlockStart == NULL || m_pBlockEnd == NULL || m_pNextAvailable == NULL)
	{
		AKPLATFORM::OutputDebugMsg("AkCallbackSerializer::AllocNewCall: Callback serializer terminated but still received event calls. Abort.\n");
		m_Lock.Unlock();
		return NULL;
	}

	//Is there enough space between the write head and the end of the buffer?
	if (pItemEnd >= m_pBlockEnd)
	{
		//Nope, need to wrap around
		//But is the read ptr in the way?
		if (pReadPtr > m_pNextAvailable)
		{
			//Queue is full, wait for the game to empty it to avoid losing information.
			m_Lock.Unlock();
			if (in_bCritical)
			{
				AKPLATFORM::AkWaitForEvent(m_DrainEvent);
				goto retry;
			}
			else
				return NULL;	//No memory for that, and we can't block the main game thread.  Expected if the ErrorLevel is set to ALL.
		}

		m_pNextAvailable = (SerializedTypes::AkCommonCallback*)m_pBlockStart;
		pItemEnd = (char*)m_pNextAvailable + uItemSize;
	}

	//Is there enough space up to the read pointer?
	if (m_pNextAvailable == pReadPtr || (m_pNextAvailable < pReadPtr && pItemEnd >= pReadPtr))
	{
		//Nope!  Queue is full, wait for the game to empty it to avoid losing information.
		m_Lock.Unlock();
		if (in_bCritical)
		{
			AKPLATFORM::AkWaitForEvent(m_DrainEvent);
			goto retry;
		}
		else
			return NULL;	//No memory for that, and we can't block the main game thread.  Expected if the ErrorLevel is set to ALL.
	}

	//Link the new item in the list.
	if (m_pFirst == NULL)
		m_pFirst = m_pNextAvailable;
	else
		*m_pLastNextPtr = m_pNextAvailable;

	m_pLastNextPtr = &(m_pNextAvailable->pNext);
	m_pNextAvailable->pNext = NULL;

	SerializedTypes::AkCommonCallback* pRet = m_pNextAvailable;
	m_pNextAvailable = (SerializedTypes::AkCommonCallback*)pItemEnd;

	m_Lock.Unlock();
	return pRet;
}


template<typename InfoStruct>
InfoStruct* AllocNewStruct(bool in_bCritical, void* pPackage, AkCallbackType eType, AkUInt32 uStringLength = 0)
{
	SerializedTypes::AkCommonCallback* common = AllocNewCall(sizeof(SerializedTypes::AkCommonCallback) + sizeof(InfoStruct) + uStringLength, in_bCritical);

	if (common == NULL)
		return NULL;

	common->eType = eType;
	common->pPackage = pPackage;

	return (InfoStruct*)(common + 1);
}

void LocalOutput(AK::Monitor::ErrorCode in_eErrorCode, const AkOSChar* in_pszError, AK::Monitor::ErrorLevel in_eErrorLevel, AkPlayingID in_playingID, AkGameObjectID in_gameObjID)
{
	//Ak_Monitoring isn't defined on the regular SDK.  It's a modification that only the C# side sees.
	const AkCallbackType eType = (AkCallbackType)AK_Monitoring_Val;
	const AkUInt32 uLen = (AkUInt32)(AKPLATFORM::OsStrLen(in_pszError) + 1) * sizeof(*in_pszError);

	SerializedTypes::AkMonitoringCallbackInfo* info = AllocNewStruct<SerializedTypes::AkMonitoringCallbackInfo>(false, NULL, eType, uLen);
	if (info != NULL)
	{
		info->errorCode = in_eErrorCode;
		info->errorLevel = in_eErrorLevel;
		info->playingID = in_playingID;
		info->gameObjID = in_gameObjID;

		memcpy(info->message, in_pszError, uLen);
	}
	else
	{
		//No space, can't log this.  Expected if the ErrorLevel is set to ALL as some logging is done on the game thread
	}
}


AKRESULT AkCallbackSerializer::Init(void * in_pMemory, AkUInt32 in_uSize)
{
	if (m_pBlockStart == NULL)
	{
		m_pBlockStart = in_pMemory;
		m_pBlockEnd = (char*)in_pMemory + in_uSize;
		m_pNextAvailable = (SerializedTypes::AkCommonCallback*)m_pBlockStart;
		AKPLATFORM::AkCreateEvent(m_DrainEvent);
		m_idThread = AKPLATFORM::CurrentThread();
	}
	return AK_Success;
}

void AkCallbackSerializer::Term()
{
	m_Lock.Lock();
	if (m_pBlockStart != NULL)
	{
		AKPLATFORM::AkSignalEvent(m_DrainEvent);
		AKPLATFORM::AkDestroyEvent(m_DrainEvent);
		m_pBlockStart = NULL;
		m_pBlockEnd = NULL;
		m_pNextAvailable = NULL;
	}

	AK::Monitor::SetLocalOutput();

	m_Lock.Unlock();
}

void AkCallbackSerializer::EventCallback(AkCallbackType in_eType, AkCallbackInfo* in_pCallbackInfo)
{
	if (in_pCallbackInfo == NULL)
	{
		//There wasn't enough memory to store the callback when the user registered it.  We don't know where to call to.
		return;
	}

	switch (in_eType)
	{
	case AK_EndOfEvent:
	case AK_Starvation:
	case AK_MusicPlayStarted:
	{
		const AkEventCallbackInfo* copyFrom = (AkEventCallbackInfo*)in_pCallbackInfo;
		SerializedTypes::AkEventCallbackInfo* info = AllocNewStruct<SerializedTypes::AkEventCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;
		}
		break;
	}
	case AK_EndOfDynamicSequenceItem:
	{
		const AkDynamicSequenceItemCallbackInfo* copyFrom = (AkDynamicSequenceItemCallbackInfo*)in_pCallbackInfo;
		SerializedTypes::AkDynamicSequenceItemCallbackInfo* info = AllocNewStruct<SerializedTypes::AkDynamicSequenceItemCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->audioNodeID = copyFrom->audioNodeID;
			info->pCustomInfo = copyFrom->pCustomInfo;
		}
		break;
	}
	case AK_Marker:
	{
		const AkMarkerCallbackInfo* copyFrom = (AkMarkerCallbackInfo*)in_pCallbackInfo;
		const AkUInt32 uLength = copyFrom->strLabel != NULL ? (AkUInt32)strlen(copyFrom->strLabel) : 0;

		SerializedTypes::AkMarkerCallbackInfo* info = AllocNewStruct<SerializedTypes::AkMarkerCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, uLength);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;
			info->uIdentifier = copyFrom->uIdentifier;
			info->uPosition = copyFrom->uPosition;
			memcpy(info->strLabel, copyFrom->strLabel, uLength);
			info->strLabel[uLength] = 0;
		}
		break;
	}
	case AK_Duration:
	{
		const AkDurationCallbackInfo* copyFrom = (AkDurationCallbackInfo*)in_pCallbackInfo;
		SerializedTypes::AkDurationCallbackInfo* info = AllocNewStruct<SerializedTypes::AkDurationCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;
			info->fDuration = copyFrom->fDuration;
			info->fEstimatedDuration = copyFrom->fEstimatedDuration;
			info->audioNodeID = copyFrom->audioNodeID;
			info->mediaID = copyFrom->mediaID;
			info->bStreaming = (SerializedTypes::MarshalBool)copyFrom->bStreaming;
		}
		break;
	}
	case AK_MIDIEvent:
	{
		const AkMIDIEventCallbackInfo* copyFrom = (AkMIDIEventCallbackInfo*)in_pCallbackInfo;
		SerializedTypes::AkMIDIEventCallbackInfo* info = AllocNewStruct<SerializedTypes::AkMIDIEventCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;
			info->midiEvent = copyFrom->midiEvent; // @todo: (de)serialization of AkMIDIEvent needs to be tested
		}
		break;
	}
	case AK_MusicSyncUserCue:
	case AK_MusicSyncBeat:
	case AK_MusicSyncBar:
	case AK_MusicSyncEntry:
	case AK_MusicSyncExit:
	case AK_MusicSyncGrid:
	case AK_MusicSyncPoint:
	{
		const AkMusicSyncCallbackInfo* copyFrom = (AkMusicSyncCallbackInfo*)in_pCallbackInfo;
		const bool hasName = in_eType == AK_MusicSyncUserCue && copyFrom->pszUserCueName != NULL;
		const AkUInt32 uLength = hasName ? (AkUInt32)strlen(copyFrom->pszUserCueName) : 0;

		SerializedTypes::AkMusicSyncCallbackInfo* info = AllocNewStruct<SerializedTypes::AkMusicSyncCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, uLength);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;

			info->segmentInfo_iCurrentPosition = copyFrom->segmentInfo.iCurrentPosition;
			info->segmentInfo_iPreEntryDuration = copyFrom->segmentInfo.iPreEntryDuration;
			info->segmentInfo_iActiveDuration = copyFrom->segmentInfo.iActiveDuration;
			info->segmentInfo_iPostExitDuration = copyFrom->segmentInfo.iPostExitDuration;
			info->segmentInfo_iRemainingLookAheadTime = copyFrom->segmentInfo.iRemainingLookAheadTime;
			info->segmentInfo_fBeatDuration = copyFrom->segmentInfo.fBeatDuration;
			info->segmentInfo_fBarDuration = copyFrom->segmentInfo.fBarDuration;
			info->segmentInfo_fGridDuration = copyFrom->segmentInfo.fGridDuration;
			info->segmentInfo_fGridOffset = copyFrom->segmentInfo.fGridOffset;

			info->musicSyncType = copyFrom->musicSyncType;
			memcpy(info->userCueName, copyFrom->pszUserCueName, uLength);
			info->userCueName[uLength] = 0;
		}
		break;
	}
	default:
		break;
	}
}

void* AkCallbackSerializer::Lock()
{
	m_Lock.Lock();
	SerializedTypes::AkCommonCallback* pRead = NULL;
	if (m_pFirst != NULL)
	{
		//Terminate the linked list.
		*m_pLastNextPtr = NULL;
		m_pLastNextPtr = NULL;	
		m_pLockedPtr = m_pFirst;
		pRead = m_pLockedPtr;
		m_pFirst = NULL;
	}
	m_Lock.Unlock();
	
	return pRead;
}	

void AkCallbackSerializer::Unlock()
{
	m_Lock.Lock();
	m_pLockedPtr = NULL;
	m_Lock.Unlock();
	AKPLATFORM::AkSignalEvent(m_DrainEvent);
}

void AkCallbackSerializer::SetLocalOutput( AkUInt32 in_uErrorLevel )
{
	AK::Monitor::SetLocalOutput(in_uErrorLevel, (in_uErrorLevel != 0) ? (AK::Monitor::LocalOutputFunc)LocalOutput : NULL);
}

void AkCallbackSerializer::BankCallback( AkUInt32 in_bankID, const void* in_pInMemoryBankPtr, AKRESULT in_eLoadResult, AkMemPoolId in_memPoolId, void *in_pCookie )
{
	if (in_pCookie == NULL)
	{
		//There wasn't enough memory to store the callback when the user registered it.  We don't know where to call to.
		return;
	}

	//Ak_Bank isn't defined on the regular SDK.  It's a modification that only the C# side sees.
	const AkCallbackType eType = (AkCallbackType)AK_Bank_Val;
	SerializedTypes::AkBankCallbackInfo* info = AllocNewStruct<SerializedTypes::AkBankCallbackInfo>(false, in_pCookie, eType, 0);
	if (info != NULL)
	{
		info->bankID = in_bankID;
		info->inMemoryBankPtr = (AkUIntPtr)in_pInMemoryBankPtr;
		info->loadResult = in_eLoadResult;
		info->memPoolId = in_memPoolId;
	}
}

#ifdef AK_IOS
AKRESULT AkCallbackSerializer::AudioInterruptionCallbackFunc(bool in_bEnterInterruption, void* in_pCookie)
{
	// Customization for C# only.
	const AkCallbackType eType = (AkCallbackType)AK_AudioInterruption_Val;
	SerializedTypes::AkAudioInterruptionCallbackInfo* info = AllocNewStruct<SerializedTypes::AkAudioInterruptionCallbackInfo>(true, in_pCookie, eType, 0);
	if (info == NULL)
		return AK_Fail;

	info->bEnterInterruption = (SerializedTypes::MarshalBool)in_bEnterInterruption;
	return AK_Success;
}
#endif // #ifdef AK_IOS

AKRESULT AkCallbackSerializer::AudioSourceChangeCallbackFunc(bool in_bOtherAudioPlaying, void* in_pCookie)
{
	// On iOS, this user callback is triggered by the initial WakeupFromSuspend() call
	// This is before the sound engine is initialized. 
	// Bypass this call.
	if (m_pBlockStart == NULL || m_pBlockEnd == NULL || m_pNextAvailable == NULL)
		return AK_Cancelled;

	// Customization for C# only.
	const AkCallbackType eType = (AkCallbackType)AK_AudioSourceChange_Val;
	SerializedTypes::AkAudioSourceChangeCallbackInfo* info = AllocNewStruct<SerializedTypes::AkAudioSourceChangeCallbackInfo>(true, in_pCookie, eType, 0);
	if (info == NULL)
		return AK_Fail;

	info->bOtherAudioPlaying = (SerializedTypes::MarshalBool)in_bOtherAudioPlaying;
	return AK_Success;
}
