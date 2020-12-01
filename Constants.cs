using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armed
{
    /// <summary>
    /// Contains all non-mutable values within the Armed editor.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The name of a tab when the new file button is pressed.
        /// </summary>
        public static string NewFileName { get; set; } = "newFile.asm";

        /// <summary>
        /// The amount of right-padding given to the line number margin.
        /// </summary>
        public static int MarginPadding { get; set; } = 2;

        /// <summary>
        /// All base ARM instruction mnemonics available in ARM.
        /// </summary>
        public static string Instructions =

            //branch instructions
            "b bl blx bx bxj" +

            //general data processing
            "adc add bic cmn cmp eor mov mvn" +
            "orn orr rsb rsc sbc sub teq tst" +

            //shift
            "asr lsl lsr ror rrx" +

            //general multiply
            "mul mls mla" +

            //signed multiply
            "smlabb smlabt smlatb smlatt smlad smlal" +
            "smlalbb smlalbt smlaltb smlaltt smlald smlawb" +
            "smlawt smlsd smlsld smmla smmls smmul smuad" +
            "smulbb smulbt smultb smultt smull smulwb smulwt" +
            "smusd" +

            //unsigned multiply
            "umaal umlal umull" +

            //saturating
            "ssat ssat16 usat usat16 qadd qsub qdadd qdsub" +

            //packing and unpacking
            "pkh sxtab sxtab16 sxtah sxtb sxtb16 sxth" +
            "uxtab uxtab16 uxtah uxtb uxtb16 uxth" +

            //misc dpi
            "bfc bfi clz movt rbit rev rev16 revsh sbfx" +
            "sel ubfx usad8 usada8";
        
    }
}
