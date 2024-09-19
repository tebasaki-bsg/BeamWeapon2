using System;
using System.Collections;
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
using Vector3 = UnityEngine.Vector3;

namespace BW2Space
{
    [XmlRoot("BW2AdSaberModule")]
    [Reloadable]
    public class BW2AdSaberModule : BlockModule
    {
        [XmlElement("DamageSlider")]
        [RequireToValidate]
        public MSliderReference DamageSlider;

        [XmlElement("BW2SelfDamage")]
        [RequireToValidate]
        public MToggleReference bw2SelfDamage;

    }

    public class BW2AdSaberBehaviour : BlockModuleBehaviour<BW2AdSaberModule>
    {
        private MSlider damageslider;
        private int damage;
        private BW2AdSaberModuleBehaviour bw2adsabermodulebehaviour;
        private AdSaberModuleBehaviour adsabermodulebehaviour;

        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            damageslider = GetSlider(Module.DamageSlider);
            damage = (int)damageslider.Value;
            adsabermodulebehaviour = base.GetComponent<AdSaberModuleBehaviour>();

            bw2adsabermodulebehaviour = gameObject.AddComponent<BW2AdSaberModuleBehaviour>();
            bw2adsabermodulebehaviour.Damage = damage;
            bw2adsabermodulebehaviour.selfdamage = GetToggle(Module.bw2SelfDamage);
            bw2adsabermodulebehaviour.ownerid = adsabermodulebehaviour.OwnerID;
            bw2adsabermodulebehaviour.myblockname = adsabermodulebehaviour.name;
            Mod.Log(bw2adsabermodulebehaviour.myblockname);

        }

        public class BW2AdSaberModuleBehaviour : AdSaberModuleBehaviour
        {
            public int Damage;
            public int ownerid;
            public MToggle selfdamage;
            public string myblockname;
            public new void OnTriggerEnter(Collider other)
            {

                if (!(other.attachedRigidbody != null))
                {
                    return;
                }

                Rigidbody attachedRigidbody = other.attachedRigidbody;
                BasicInfo bInfo = attachedRigidbody.GetComponent<BasicInfo>();

                if (bInfo is BlockBehaviour)
                {
                    BlockBehaviour block = bInfo as BlockBehaviour;
                    ushort ID = block.ParentMachine.PlayerID;

                    if (block.isSimulating)
                    {
                        if(block.name != myblockname)
                        {
                            if (ID != ownerid)
                            {
                                Mod.HPDict[ID] -= Damage;

                            }
                            else if (selfdamage.IsActive)
                            {
                                Mod.HPDict[ID] -= Damage;

                            }
                        }

                    }
                }
                base.OnTriggerEnter(other);
            }
        }




    }

}
