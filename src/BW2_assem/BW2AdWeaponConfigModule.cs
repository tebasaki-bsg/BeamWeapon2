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
using skpCustomModule;

namespace BW2Space
{
    [XmlRoot("BW2AdWeaponConfigModule")]
    [Reloadable]

    public class BW2AdWeaponConfigModule : BlockModule
    {
        [XmlElement("DamageSlider")]    //XMLにはこの名前が使われる
        [RequireToValidate]
        public MSliderReference DamageSlider;

        [XmlElement("ExplodeRadius")]
        [RequireToValidate]
        public MSliderReference RadiusSlider;
    }

    public class BW2AdWeaponConfigBehaviour : BlockModuleBehaviour<BW2AdWeaponConfigModule>
    {
        private MSlider damageslider;
        private int Damage;
        private MSlider radiusslider;
        private int Radius;

        private Transform effectpool;
        private Transform adprojectile;

        private AdShootingBehavour adshootingbehavour;
        private AdShootingProp adshootingprop;
        private AdExplosionEffect adexplosioneffect;
        private AdProjectileScript adprojectilescript;
        private bool UseBeacon = false;

        private BW2AdExplosionController bw2adexplosioncontroller;


        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            adshootingbehavour = base.GetComponent<AdShootingBehavour>();
            adshootingprop = base.GetComponent<AdShootingProp>();
            damageslider = GetSlider(Module.DamageSlider);
            Damage = (int)damageslider.Value;
            radiusslider = GetSlider(Module.RadiusSlider);
            Radius = (int)radiusslider.Value;


            effectpool = GameObject.Find("PManager").transform.Find("EffectPool");

            foreach (Transform child in effectpool)
            {
                if (child.name == "ExplosionEffect")
                {
                    adexplosioneffect = child.gameObject.GetComponent<AdExplosionEffect>();

                    if (adshootingbehavour.BlockName == adexplosioneffect.BlockName)
                    {
                        bw2adexplosioncontroller = child.gameObject.GetComponent<BW2AdExplosionController>();

                        if (bw2adexplosioncontroller == null)
                        {
                            bw2adexplosioncontroller = child.gameObject.AddComponent<BW2AdExplosionController>();
                        }
                        bw2adexplosioncontroller.Damage = Damage;
                        bw2adexplosioncontroller.Radius = Radius;

                    }
                }

            }

        }

    }

    public class BW2AdExplosionController : AdExplosionEffect
    {
        public int Damage;
        public int Radius;
        public ushort ID;
        private LayerMask layermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        private string blockname;
        private bool init;
        public Dictionary<ushort, bool> ApplyDict = new Dictionary<ushort, bool>
        {
            [0] = false,
            [1] = false,
            [2] = false,
            [3] = false,
            [4] = false,
            [5] = false,
            [6] = false,
            [7] = false,
            [8] = false,
            [9] = false,
            [10] = false,
            [11] = false,
            [12] = false,
            [13] = false,
            [14] = false,
            [15] = false,
            [16] = false,
            [17] = false,
            [100] = true

        };


        public new void FixedUpdate()
        {
            if (!init)
            {
                if (!StatMaster.isMP || StatMaster.isHosting || StatMaster.isLocalSim)   //マルチでない or ホストである or ローカルシミュである
                {

                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius, layermask);
                    foreach (Collider collider in hitColliders)
                    {
                        blockname = collider.gameObject.transform.parent.name;
                        Mod.Log("Checking " + blockname);

                        switch (blockname)
                        {
                            case "Simulation Machine":  //コアブロ、ホイール、ユニバなど
                                ID = collider.gameObject.GetComponent<BlockBehaviour>().ParentMachine.PlayerID;
                                break;


                            case "Colliders": //Modブロック
                                ID = collider.gameObject.transform.parent.transform.parent.GetComponent<BlockBehaviour>().ParentMachine.PlayerID;
                                break;

                            default:
                                try
                                {
                                    ID = collider.gameObject.transform.parent.GetComponent<BlockBehaviour>().ParentMachine.PlayerID;
                                }
                                catch
                                {
                                    ID = 100;
                                }
                                break;
                        }

                        if (!ApplyDict[ID])
                        {
                            Mod.HPDict[ID] -= Damage;
                            ApplyDict[ID] = true;
                        }
                    }
                }
                init = true;

            }

            base.FixedUpdate();
        }

    }
}
