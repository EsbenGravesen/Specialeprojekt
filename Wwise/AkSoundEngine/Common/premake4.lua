
newoption {
   trigger     = "akplatform",
   value       = "VALUE",
   description = "Choose one platform to generate",
   allowed = {
      { "Mac",		"Mac" },
      { "Linux",	"Linux" },
      { "iOS", 		"iOS" },
      { "Windows",  "Windows" },
	  { "Android",  "Android" },
	  { "WiiU",		"WiiU" },
	  { "nacl",		"nacl" },
	  { "Vita",  	"Vita" },
	  { "XboxOne",  "XboxOne" },
	  { "XboxOneADK",  "XboxOneADK" },
	  { "Metro",  "Metro" },
	  { "WinPhone",	"WinPhone" },
	  { "PS4",  "PS4" },
	  { "3DS",  "3DS" },
	  { "QNX",  "QNX" },
   }
}

function CreateCommonProject(platformName, suffix, motion)	 
	-- A project defines one build target
	local pfFolder = "../"..platformName.."/";
	local prj = project ("AkSoundEngine"..platformName)
		targetname "AkSoundEngine"
		kind "SharedLib"
		language "C++"
		files { 
			"../Common/AkCallbackSerializer.cpp" ,
			pfFolder .. "AkDefaultIOHookBlocking.cpp",
			"../Common/AkFileLocationBase.cpp",
			"../Common/AkFilePackage.cpp",
			"../Common/AkFilePackageLUT.cpp",
			"../Common/AkSoundEngineStubs.cpp",
			pfFolder .. "SoundEngine_wrap.cxx",
			pfFolder .. "stdafx.cpp",
			}
		defines {
			"AUDIOKINETIC",			
			}		
		links {
			"AkSoundEngine"..suffix,			
			"AkAudioInputSource"..suffix,
			"AkCompressorFX"..suffix,		
			"AkDelayFX"..suffix,
			"AkExpanderFX"..suffix,
			"AkFlangerFX"..suffix,
			"AkGainFX"..suffix,
			"AkGuitarDistortionFX"..suffix,
			"AkHarmonizerFX"..suffix,
			"AkMatrixReverbFX"..suffix,
			"AkMemoryMgr"..suffix,
			"AkMeterFX"..suffix,
			"AkMusicEngine"..suffix,
			"AkParametricEQFX"..suffix,
			"AkPeakLimiterFX"..suffix,
			"AkPitchShifterFX"..suffix,
			"AkRecorderFX"..suffix,
			"AkRoomVerbFX"..suffix,
			"AkSilenceSource"..suffix,
			"AkSineSource"..suffix,			
			"AkStereoDelayFX"..suffix,
			"AkStreamMgr"..suffix,
			"AkSynthOne"..suffix,
			"AkTimeStretchFX"..suffix,
			"AkToneSource"..suffix,
			"AkTremoloFX"..suffix,			
			"AkVorbisDecoder"..suffix			
			}
		if motion then
			links {
				"AkRumble"..suffix,
				"AkMotionGenerator"..suffix
				}
		end
			
		includedirs {
			"../"..platformName,
			"$(WWISESDK)/include",		
			"."
			}		
			
		configuration "Debug"
			defines { "_DEBUG" }
			flags { "Symbols" }
			links { "CommunicationCentral"..suffix }			

		configuration "Profile"
			defines { "NDEBUG" }			
			links { "CommunicationCentral"..suffix }

		configuration "Release"
			defines { "NDEBUG", "AK_OPTIMIZED" }
			flags { "Optimize" }   

		configuration "*"
	return prj
end

function CreateLinuxSln()
	-- A solution contains projects, and defines the available configurations
	solution "AkSoundEngineLinux"
	configurations { "Debug", "Profile", "Release" }
	platforms { "x64", "x32" }
	
	local prj = CreateCommonProject("Linux", "", false)	
		defines "__linux__"
		location "../Linux"
		links "SDL2"
			
		buildoptions {
			"-fvisibility=hidden",
			"-fdata-sections",
			"-ffunction-sections"			
			}
			
		linkoptions {
			"-Wl,--gc-sections"
		}
			
		flags {
			"EnableSSE2" 
			}
			
		local baseTargetDir = "../../Integration/Assets/Wwise/Deployment/Plugins/Linux/"			

		configuration { "Debug", "x32" }
			libdirs {
				"$(WWISESDK)/Linux_x32/Debug/lib"
				}
			targetdir( baseTargetDir .. "x86/Debug")

		configuration { "Profile", "x32" }
			libdirs {
				"$(WWISESDK)/Linux_x32/Profile/lib"
				}
			targetdir( baseTargetDir .. "x86/Profile")
			postbuildcommands ("strip " .. baseTargetDir .. "x86/Profile/libAkSoundEngine.so")

		configuration { "Release", "x32" }
			libdirs {
				"$(WWISESDK)/Linux_x32/Release/lib"
				}
			targetdir( baseTargetDir .. "x86/Release")
			postbuildcommands ("strip " .. baseTargetDir .. "x86/Release/libAkSoundEngine.so")

		configuration { "Debug", "x64" }
			libdirs {
				"$(WWISESDK)/Linux_x64/Debug/lib"
				}
			targetdir( baseTargetDir .. "x86_64/Debug")

		configuration { "Profile", "x64" }
			libdirs {
				"$(WWISESDK)/Linux_x64/Profile/lib"
				}
			targetdir( baseTargetDir .. "x86_64/Profile")
			postbuildcommands ( "strip " .. baseTargetDir .. "x86_64/Profile/libAkSoundEngine.so") 

		configuration { "Release", "x64" }
			libdirs {
				"$(WWISESDK)/Linux_x64/Release/lib"
				}
			targetdir( baseTargetDir .. "x86_64/Release")
			postbuildcommands ( "strip " .. baseTargetDir .. "x86_64/Release/libAkSoundEngine.so") 
				
		configuration "*x32"
			buildoptions { "-m32", "-msse" }

		configuration "*x64"
			buildoptions { "-m64" }		
end

function CreateWiiUSln()
	-- A solution contains projects, and defines the available configurations
	solution "AkSoundEngineWiiU"
	location "../WiiU"
	configurations { "Debug", "Profile", "Release" }
	platforms { "WiiU" }

	prebuildcommands{"python GenerateLibraryStubs.py"}
		
	local prj = CreateCommonProject("WiiU", ".a", true)				
		location "../WiiU"
		links 
		{ 
			"padscore.a",
			"nn_ac.a",
			"proc_ui.a",
			"vpad.a",
			"nn_save.a",
			"coredyn.a",
			"pad.a",
			"gfd.a",
			"mtx.a",			
			"snd_user.a",
			"gx2ut.a",
			"nsyshid.a",
			"nlibcurl.a",
			"nsysnet.a",
			"nsysccr.a",
			"nsysuvd.a",
			"gx2.a",
			"avm.a",
			"tcl.a",
			"tve.a",
			"dc.a",
			"snd_core.a",
			"uvc.a",
			"uvd.a",
			"camera.a",
			"dmae.a",
			"usb_mic.a"
		}
			
		files { "../WiiU/AkFileHelpers.cpp" }

			
		local baseTargetDir = "../../Integration/Assets/Wwise/Deployment/Plugins/WiiU/"
		
		configuration("*")				
				defines ("NDEV=1;CAFE=2;PLATFORM=CAFE;EPPC")				
				includedirs{"$(CAFE_ROOT)/system/include"}				
				flags("NoManifest")			
				
		-- Standard configuration settings.
		configuration ("*Debug*")
			libdirs{"$(CAFE_ROOT)/system/lib/ghs/cafe/DEBUG", "$(WWISESDK)/WiiU/Debug/lib"}	
			targetdir( baseTargetDir .. "Debug")			
									
		configuration ("*Profile*")
			flags ({"OptimizeSpeed"})
			libdirs{"$(CAFE_ROOT)/system/lib/ghs/cafe/NDEBUG", "$(WWISESDK)/WiiU/Profile/lib"}	
			buildoptions{"--inline_tiny_functions -OI -OB"}
			targetdir( baseTargetDir .. "Profile")		
			vs_propsheet("../WiiU/PostLink.props")
		
		configuration ("*Release*")
			defines ({"NDEBUG","AK_OPTIMIZED"})
			flags ({"OptimizeSpeed"})	
			libdirs{"$(CAFE_ROOT)/system/lib/ghs/cafe/NDEBUG", "$(WWISESDK)/WiiU/Release/lib"}	
			buildoptions{"--inline_tiny_functions -OI -OB"}			
			targetdir( baseTargetDir .. "Release")		
			vs_propsheet("../WiiU/PostLink.props")
end

if _OPTIONS["akplatform"] == "WiiU" then
	CreateWiiUSln();
end
if _OPTIONS["akplatform"] == "Linux" then
	CreateLinuxSln();
end


