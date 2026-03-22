//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CustomAlbums.Managers;
//using Il2CppAssets.Scripts.Database;
//using IronSearch.Patches;
//using IronSearch.Records;

//namespace IronSearch.Tags
//{
//    internal partial class BuiltIns
//    {

//        internal static bool EvalHide(MusicInfo musicInfo)
//        {
//            return RefreshPatch.hides.Contains(musicInfo.uid);
//        }
//        internal static bool EvalHide(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
//        {
//            ThrowIfNotEmpty(varArgs);
//            ThrowIfNotEmpty(varKwargs);
//            return EvalHide(M.I);
//        }
//    }
//}

////Doesn't work because charts that have been hidden cannot show up in search