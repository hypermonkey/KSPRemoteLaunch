using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using UnityEngine;

namespace KSPRemoteLaunch
{
    ///<Summary>
    /// Extended version of MonoBehavoiur
    /// Handles logging to the console
    ///</Summary>
    public class MonoBehaviourExtended : MonoBehaviour
    {
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void LogDebugOnly(String Message, params object[] strParams)
        {
            Log(Message, strParams);
        }

        internal static void Log(String Message, params object[] strParams)
        {

            Debug.Log(String.Format(Message, strParams));
        }

        public static void h ()
        {
            
        }
    }
}
