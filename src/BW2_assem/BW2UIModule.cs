using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using Modding.Serialization;
using Modding.Modules;
using Modding.Blocks;
using Vector3 = UnityEngine.Vector3;
using USlider = UnityEngine.UI.Slider;

namespace BW2Space
{
    [XmlRoot("BW2UIModule")]
    [Reloadable]
    public class BW2UIModule : BlockModule
    {
        [XmlElement("HPSlider")]
        [RequireToValidate]
        public MSliderReference HitPointSlider;

    }

    public class BW2UIBehaviour : BlockModuleBehaviour<BW2UIModule>
    {

        public static Font font;
        private Color32 UseColor = new Color32(0, 255, 200, 255);
        private Color32 HalfColor = new Color32(250, 200, 75, 255);
        private Color32 QuarterColor = new Color32(255, 100, 100, 255);
        private Color32 BackgroundColor = new Color32(230, 230, 230, 70);

        public MSlider HitPointSlider;  //"HPSlider"ではない

        private int MaxHP;
        private int HalfHP;
        private int QuarterHP;

        private int BlockPlayerID;

        private GameObject BW2UI;
        private GameObject BW2_HPcoreUI;
        private Text HPText;
        private bool isOwnerSame = false;

        private bool ClearNormal = false;
        private bool ClearHalf = false;

        private int previousHP = 100;
        private int previousHPinAlways = 100;

        public Joint joint;

        private Transform colliders;
        private BoxCollider boxcollider;
        private bool BreakBlock = false;


        void Awake()
        {
            font = Mod.modAssetBundle.LoadAsset<Font>("Orbitron Medium");
        }

        void Start()
        {
            colliders = transform.Find("Colliders");
            foreach(Transform child in colliders)
            {
                if(child.GetComponent<BoxCollider>() != null)
                {
                    boxcollider = child.GetComponent<BoxCollider>();
                }
            }
            joint = GetComponent<Joint>();


            UpdateOwnerFlag();

        }


        public override void SafeAwake()
        {
            base.SafeAwake();

        }

        public override void OnSimulateStart()  //シミュ開始時
        {
            base.OnSimulateStart();

            HitPointSlider = GetSlider(Module.HitPointSlider);
            MaxHP = (int)HitPointSlider.Value;
            HalfHP = MaxHP / 2;
            QuarterHP = MaxHP / 4;

            if (StatMaster.isMP)
            {
                BlockPlayerID = BlockBehaviour.ParentMachine.PlayerID;
            }
            else
            {
                BlockPlayerID = 0;
            }

            Mod.HPDict[BlockPlayerID] = (int)HitPointSlider.Value;

            UpdateOwnerFlag();
            if (!isOwnerSame) return;

            BW2UI = Mod.BW2_UI;

            GenerateUI();


            BW2_HPcoreUI.SetActive(true);
        }

        public override void OnSimulateStop()   //シミュ停止時
        {
            base.OnSimulateStop();
            if (!isOwnerSame) return;
            Destroy(BW2_HPcoreUI);
        }

        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            if (!isOwnerSame) return;

            if (Mod.HPDict[BlockPlayerID] < 0)
            {
                Mod.HPDict[BlockPlayerID] = 0;
            }

            if (previousHPinAlways != Mod.HPDict[BlockPlayerID])
            {
                if (Mod.HPDict[BlockPlayerID] < HalfHP && QuarterHP < Mod.HPDict[BlockPlayerID])
                {
                    if (!ClearNormal)
                    {
                        UseColor = HalfColor;
                        ClearNormal = true;
                    }

                }
                else if (Mod.HPDict[BlockPlayerID] < QuarterHP)
                {
                    if (!ClearHalf)
                    {
                        UseColor = QuarterColor;
                        ClearHalf = true;
                    }
                }
                Destroy(BW2_HPcoreUI);
                GenerateUI();
                previousHPinAlways = Mod.HPDict[BlockPlayerID];
            }

            HPText.text = Mod.HPDict[BlockPlayerID].ToString("D5");

        }

        public override void SimulateUpdateHost()   //ホスト側のブロックだけの動作：HPが0になったらコアを破壊 / HPが更新されたらクライアントにHPを送信
        {
            base.SimulateUpdateHost();

            if (Mod.HPDict[BlockPlayerID] <= 0)
            {
                Mod.HPDict[BlockPlayerID] = 0;

                if (!BreakBlock)
                {
                    Destroy(boxcollider);
                    Destroy(joint);

                    BreakBlock = true;
                }
            }

            if (previousHP != Mod.HPDict[BlockPlayerID])
            {
                Mod.message = Mod.messageType.CreateMessage(BlockPlayerID, Mod.HPDict[BlockPlayerID]);
                ModNetworking.SendToAll(Mod.message);
                previousHP = Mod.HPDict[BlockPlayerID];
            }
        }


        private void UpdateOwnerFlag()
        {
            if (StatMaster.isMP)
            {
                ushort BlockPlayerID = BlockBehaviour.ParentMachine.PlayerID;
                ushort LocalPlayerID = PlayerMachine.GetLocal().Player.NetworkId;
                isOwnerSame = BlockPlayerID == LocalPlayerID;
            }
            else
            {
                isOwnerSame = true;
            }
        }

        private void GenerateUI()
        {
            BW2_HPcoreUI = new GameObject("BW2_HPcoreUI");  //HPcoreで表示させるUI全体の親
            BW2_HPcoreUI.transform.SetParent(BW2UI.transform);
            BW2_HPcoreUI.layer = LayerMask.NameToLayer("HUD");
            RectTransform BW2_HPcoreUITrans = BW2_HPcoreUI.AddComponent<RectTransform>();
            BW2_HPcoreUITrans.sizeDelta = new Vector2(300, 300);
            BW2_HPcoreUITrans.anchorMin = new Vector2(0.07f, 0.1f);
            BW2_HPcoreUITrans.anchorMax = new Vector2(0.08f, 0.1f);
            BW2_HPcoreUITrans.pivot = new Vector2(0.5f, 0.5f);
            BW2_HPcoreUITrans.anchoredPosition = new Vector2(0, 0);

            GameObject HPnumber = new GameObject("HPnumber");
            HPnumber.transform.SetParent(BW2_HPcoreUI.transform);
            HPnumber.layer = LayerMask.NameToLayer("HUD");
            RectTransform HPnumberRect = HPnumber.AddComponent<RectTransform>();
            HPnumberRect.anchorMin = new Vector2(0.5f, 0.5f);
            HPnumberRect.anchorMax = new Vector2(0.5f, 0.5f);
            HPnumberRect.pivot = new Vector2(0.5f, 0.5f);
            HPnumberRect.sizeDelta = new Vector2(700, 200);
            HPnumberRect.anchoredPosition = new Vector2(0, -5);
            HPnumberRect.localScale = new Vector3(0.6f, 0.6f, 1);
            HPText = HPnumber.AddComponent<Text>();
            HPText.text = "001";
            HPText.font = font;
            HPText.fontSize = 135;
            HPText.fontStyle = FontStyle.Normal;
            HPText.color = UseColor;
            HPText.alignment = TextAnchor.MiddleRight;

            //HPバーを作成
            GameObject HPSlider = new GameObject("HPbar");  //GameObjectの探索の練習用にずらしてみる
            HPSlider.transform.SetParent(BW2_HPcoreUI.transform);
            HPSlider.layer = LayerMask.NameToLayer("HUD");
            RectTransform reloadSliderRect = HPSlider.AddComponent<RectTransform>();
            reloadSliderRect.anchoredPosition = new Vector2(55, -55);
            reloadSliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            reloadSliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            reloadSliderRect.pivot = new Vector2(0.5f, 0.5f);
            reloadSliderRect.sizeDelta = new Vector2(300, 45);
            reloadSliderRect.localScale = new Vector3(1, 1, 1);
            USlider slider = HPSlider.AddComponent<USlider>();  //SliderにはBackGround,FillArea,Fillが必要
            GameObject background = new GameObject("Background");
            background.transform.SetParent(HPSlider.transform);
            RectTransform backgroundRect = background.AddComponent<RectTransform>();
            backgroundRect.offsetMin = new Vector2(-3.5f, -3.5f);
            backgroundRect.offsetMax = new Vector2(3.5f, 3.5f);
            backgroundRect.anchorMin = new Vector2(0, 0.25f);
            backgroundRect.anchorMax = new Vector2(1, 0.75f);
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            background.AddComponent<Image>().color = BackgroundColor;
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(HPSlider.transform);
            fillArea.layer = LayerMask.NameToLayer("HUD");
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.offsetMin = new Vector2(0, 0);
            fillAreaRect.offsetMax = new Vector2(0, 0);
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            GameObject fill = new GameObject("Fill");   //fillAreaの子オブジェクト
            fill.transform.SetParent(fillArea.transform);
            fill.layer = LayerMask.NameToLayer("HUD");
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.offsetMin = new Vector2(0, 0);
            fillRect.offsetMax = new Vector2(0, 0);
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fill.AddComponent<Image>().color = UseColor;

            slider.fillRect = fillRect;
            slider.value = (float)Mod.HPDict[BlockPlayerID] / (float)MaxHP;
        }

    }

}
