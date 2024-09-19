using System;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using UnityEngine.UI;


namespace BW2Space
{
	public class Mod : ModEntryPoint
	{
		public static GameObject BW2_UI;
		public static Dictionary<int, int> HPDict;
		public static MessageType messageType;
		public static Message message;

		public static ModAssetBundle modAssetBundle;

		public static void Log(string msg)
		{
			Debug.Log("BW2 Log: " + msg);
		}
		public static void Warning(string msg)
		{
			Debug.LogWarning("BW2 Warning: " + msg);
		}
		public static void Error(string msg)
		{
			Debug.LogError("BW2 Error: " + msg);
		}

		public override void OnLoad()
		{
			// Called when the mod is loaded.

			Modding.Modules.CustomModules.AddBlockModule<BW2UIModule, BW2UIBehaviour>("BW2UIModule", true);
			Modding.Modules.CustomModules.AddBlockModule<BW2AmmoUIModule, BW2AmmoUIBehaviour>("BW2AmmoUIModule", true);
			Modding.Modules.CustomModules.AddBlockModule<BW2AdWeaponConfigModule, BW2AdWeaponConfigBehaviour>("BW2AdWeaponConfigModule", true);
			Modding.Modules.CustomModules.AddBlockModule<BW2AdMissileConfigModule, BW2AdMissileConfigBehaviour>("BW2AdMissileConfigModule", true);
			Modding.Modules.CustomModules.AddBlockModule<BW2AdSaberModule, BW2AdSaberBehaviour>("BW2AdSaberModule", true);

			HPDict = new Dictionary<int, int> { };

			GameObject val = new GameObject("BW2 UI");
			BW2_UI = val;
			UnityEngine.Object.DontDestroyOnLoad(val);
			Canvas val2 = BW2_UI.AddComponent<Canvas>();
			val2.renderMode = (RenderMode)0;
			val2.sortingOrder = 0;
			((Component)val2).gameObject.layer = LayerMask.NameToLayer("HUD");
			BW2_UI.AddComponent<CanvasScaler>().scaleFactor = 1f;

			//AssetBundleの読み込み
			switch (Application.platform)   //OS毎に変更
			{
				case RuntimePlatform.WindowsPlayer:
					modAssetBundle = ModResource.GetAssetBundle("BW2");
					break;
				case RuntimePlatform.OSXPlayer:
					modAssetBundle = ModResource.GetAssetBundle("BW2Mac");
					break;
				case RuntimePlatform.LinuxPlayer:
					modAssetBundle = ModResource.GetAssetBundle("BW2Mac");
					break;
				default:
					modAssetBundle = ModResource.GetAssetBundle("BW2");
					break;
			}

			//クライアントに送信するメッセージの型と受理時動作の登録
			messageType = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer);
			ModNetworking.Callbacks[messageType] += new Action<Message>(ApplyHP);

		}

		public static void ApplyHP(Message message)
		{
			HPDict[(int)message.GetData(0)] = (int)message.GetData(1);
		}
	}
}
