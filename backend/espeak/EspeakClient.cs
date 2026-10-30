using System.Runtime.InteropServices;

namespace PhonemizerSharp.backend.espeak
{
    public class EspeakClient
    {
        [DllImport("libespeak-ng.dll", CharSet = CharSet.Auto)]
        static extern EspeakError espeak_SetVoiceByName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport("libespeak-ng.dll", CharSet = CharSet.Auto)]
        static extern int espeak_Initialize(EspeakAudioOutput output, int bufferLength, string path, int options);

        [DllImport("libespeak-ng.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr espeak_TextToPhonemes([MarshalAs(UnmanagedType.LPUTF8Str)] ref string text, int textmode, int phonemes_mode);

        [DllImport("libespeak-ng.dll", CharSet = CharSet.Auto)]
        static extern EspeakError espeak_Cancel();

        static bool CheckResult(EspeakError result)
        {
            if (result == EspeakError.EE_OK)
            {
                return true;
            }
            else if (result == EspeakError.EE_BUFFER_FULL)
            {
                return false;
            }
            else if (result == EspeakError.EE_INTERNAL_ERROR)
            {
                throw new Exception("Internal error in ESpeak.");
            }
            else
            {
                return false;
            }
        }

        public static void Initialize(string path)
        {
            var result = espeak_Initialize(EspeakAudioOutput.Retrieval, 0, path, 0);

            if (result == (int)EspeakError.EE_INTERNAL_ERROR)
            {
                throw new Exception(string.Format("Could not initialize ESpeak. Maybe there is no espeak data at {0}?", path));
            }
        }

        public static bool SetVoiceByName(string name)
        {
            var result = espeak_SetVoiceByName(name);
            return CheckResult(result);
        }

        public static string TextToPhonemes(string text, bool tie = false)
        {
            int phonemes_mode = 2;

            if(tie)
            {
                phonemes_mode = 0x02 | 0x01 << 7 | (int)('͡') << 8;
            }
            else
            {
                phonemes_mode = (int)('_') << 8 | 0x02;
            }

            IntPtr phonemePtr = espeak_TextToPhonemes(ref text, 1, phonemes_mode);

            return Marshal.PtrToStringUTF8(phonemePtr);
        }

        public static bool Stop()
        {
            var result = espeak_Cancel();
            return CheckResult(result);
        }
    }
}
