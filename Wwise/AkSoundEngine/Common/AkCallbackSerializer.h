
#include <AK/SoundEngine/Common/AkCallback.h>
#include <AK/Tools/Common/AkMonitorError.h>

#include "SwigExceptionSwitch.h"


/// This class allows the Sound Engine callbacks to be processed in the user thread instead of the audio thread.
/// This is done by accumulating the callback generating events until the game calls CallbackSerializer::PostCallbacks().
/// All pending callbacks will be done at that point.  This removes the need for external synchronization for the callback
/// functions that the game would need otherwise.
class AkCallbackSerializer
{
public:
	static AKRESULT Init(void * in_pMemory, AkUInt32 in_uSize);

#ifdef SWIG
PAUSE_SWIG_EXCEPTIONS
#endif // #ifdef SWIG
	static void Term();

#ifdef SWIG
RESUME_SWIG_EXCEPTIONS
#endif // #ifdef SWIG

	static void* Lock();

	static void SetLocalOutput(AkUInt32 in_uErrorLevel);
	
	static void Unlock();

	static void EventCallback(AkCallbackType in_eType, AkCallbackInfo* in_pCallbackInfo);		
	static void BankCallback( AkUInt32 in_bankID, const void* in_pInMemoryBankPtr, AKRESULT in_eLoadResult, AkMemPoolId in_memPoolId, void *in_pCookie );
#ifdef AK_IOS
	static AKRESULT AudioInterruptionCallbackFunc(bool in_bEnterInterruption, void* in_pCookie);
#endif // #ifdef AK_IOS
	static AKRESULT AudioSourceChangeCallbackFunc(bool in_bOtherAudioPlaying, void* in_pCookie);
};
